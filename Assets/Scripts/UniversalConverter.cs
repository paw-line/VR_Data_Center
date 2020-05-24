using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/**
 * \brief Синглтон-конвертер входных данных в настройки визуализаторов
 * \authors Пивко Артём
 * \version 1.0
 * \date 8.04.20
 * \warning Этот скрипт должен быть размещен на сцене в ЕДИНСТВЕННОМ экземпляре. Если его не будет, не будут работать никакие визуализаторы. Если их будет много, останется только один. 
 * \todo Добавить другие температурные режимы
 *  
 * Этот класс занимается превращением цифровых данных с источников (температуры, влажности и т. д.) 
 * в параметры для визуализатров в Unity (Материалы различных цветов, ... ). 
 * 
 */

[Serializable] public class MyDictionary1 : SerializableDictionary<string, float> { }

[Serializable]
public struct Pairs
{
    public float threshold;           ///< Т
    public Color col;
}

[Serializable]
public struct TempSettings
{
    public string name;           ///< Т
    public List<Pairs> set;
    public Gradient grad;
}

/*
[Serializable]
public struct TempSettingsOld
{
    public string name;           ///< Т

    public float absCold;         ///< Температура, ниже которой не отслеживается
    public float allowedStart;    ///< Граница нижней недопустимой температуры
    public float optimalStart;    ///< Нижняя граница оптимальной температуры
    //---------------------MIDDLE LINE----------------
    public float optimalFinish;   ///< Верхняя граница оптимальной температуры
    public float allowedFinish;   ///< Граница верхней недопустимой температуры
    public float absHeat;         ///< Температура, выше которой не отслеживается
}

[Serializable]
public struct Pairs
{
    public float threshold;           ///< Т
    public Color col;
}

[Serializable]
public struct TempSettings
{
    public string name;           ///< Т
    public List<Pairs> set;
}
*/

public class UniversalConverter : MonoBehaviour
{
    //Шаблон для Синглтона
    private static UniversalConverter instance; ///< Статичная ссылка на единственный экземпляр класса

    private UniversalConverter() ///< Конструктор 
    { }

    /** \brief Функция-интерфейс для получения синглтона
     * Возвращает ссылку на синглтон.
    */
    public static UniversalConverter GetInstance()
    {
        //if (instance == null)
        //    instance = new UniversalConverter();
        if (instance == null)
            Debug.LogWarning("Tried to return");
        return instance;
    }

    /** \brief Функция, инициализурующая статическую ссылку на данный объект. 
     * Если ссылка ещё не была инициализированна, то инициализирует её этим объектом. 
     * Если же она уже не пуста, то это значит что синглтор уже иницализирован. 
     * Тогда функция уничтожает данный объект и выводит предупреждение.
     */
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Debug.LogWarning("Object " + this.gameObject.name + "tried to create another instance of " + this.GetType().ToString() + "singleton. " +
                "It will be destroyed. Try using singleton from gameObject" + instance.gameObject.name);
            Destroy(this);
        }
        ValidateSettings();
    }

    /** \brief Функция, возвращающая строку с локацией синглтона. 
     * Требуется для обнаружения несанкционированных экземпляров синглтона в случае если самоуничтожился неправильный синглтон. 
     */
    public string GetLocation()
    {
        Debug.Log("Singleton is located at " + instance.gameObject.name + "on coordinates: " + instance.transform.position.ToString());
        return ("Singleton is located at " + instance.gameObject.name + "on coordinates: " + instance.transform.position.ToString());
    }

    //Конец шаблона для синглтона

    //public MyDictionary1 coefficients;
    //public List<TempSettings> settings; 
    //public TempSettingsOld[] settings;
    public TempSettings[] tSettings;
    ///*
    //Настройки цветов. 
    //[SerializeField]
    private float absCold = 20f;         ///< Температура, ниже которой не отслеживается
    //[SerializeField]
    private float allowedStart = 35f;    ///< Граница нижней недопустимой температуры
    //[SerializeField]
    private float optimalStart = 50f;    ///< Нижняя граница оптимальной температуры
    //---------------------MIDDLE LINE = 60--
    //[SerializeField]
    private float optimalFinish = 70f;   ///< Верхняя граница оптимальной температуры
    //[SerializeField]
    private float allowedFinish = 85f;   ///< Граница верхней недопустимой температуры
    //[SerializeField]
    private float absHeat = 100f;         ///< Температура, выше которой не отслеживается
    //*/
    //[SerializeField]
    private float maxTransp = 0.4f;      ///< Максимальное значение прозрачности объекта




    /** \brief Конвертер значения температуры в цвет. 
     * \param [in] _temp Конвертируемая температура
     * \return Конвертированный в соответствии с температурой цвет
     * Метод использует значения настроек цветов из интерфейса Unity как границы перехода цветов: \n 
     * • Если температура <= absCold, то цвет будет чистым синим с обычной прозрачностью \n 
     * • Если absCold <= температура < allowedStart, то цвет будет голубым и приблежаться к бирюзовому с ростом температуры. Прозрачность нормальная \n 
     * • Если allowedStart <= температура < optimalStart, то цвет будет бирюзовым и приблежаться к зеленому с ростом температуры. Прозрачность тоже будет расти до полного исчезновения \n 
     * • Если optimalStart <= температура < optimalFinish, Объект полностью прозрачен\n 
     * • Если optimalFinish <= температура < allowedFinish, то цвет будет желтым и приблежаться к оранжевому с ростом температуры. Прозрачность тоже будет падать до нормального значения \n 
     * • Если allowedFinish <= температура < absHeat, то цвет будет оранжевым и приблежаться к красному с ростом температуры. Прозрачность нормальная \n 
     * • Если температура > absCold, то цвет будет чистым красным с обычной прозрачностью \n 
     * 
     * */
    public Color TempToColorOld(float _temp/*, string generalType*/)
    {
       
        Color newColor = new Color();
        /*
        TempSettingsOld actset = new TempSettingsOld();
        actset.name = "NULL";
        foreach (TempSettingsOld set in settings)
        {
            if (set.name == generalType)
            {
                actset = set;
            }
        }
        if (actset.name == "NULL")
        {
            Debug.LogError("Tried to use invalid generalType.");
            return newColor;
        }

        
        float absCold = actset.absCold;
        float allowedStart = actset.allowedStart;
        float optimalStart = actset.optimalStart;
        float optimalFinish = actset.optimalFinish;
        float allowedFinish = actset.allowedFinish;
        float absHeat = actset.absHeat;
        */


        if (_temp <= absCold) //Ниже предела
        {
            newColor.r = 0;
            newColor.g = 0;
            newColor.b = 1;
            newColor.a = maxTransp;
        }
        else if ((_temp > absCold) && (_temp <= allowedStart)) //Недопустимо холодно.
        {
            newColor.r = 0;
            newColor.g = (_temp - absCold) / (allowedStart - absCold); //Рост
            newColor.b = 1;
            newColor.a = maxTransp;
        }
        else if ((_temp > allowedStart) && (_temp <= optimalStart)) // допустимо холодно.
        {
            ///*
            newColor.r = 0;
            newColor.g = 1;
            newColor.b = (_temp - optimalStart) / (allowedStart - optimalStart); //Падение
            newColor.a = (_temp - optimalStart) / (allowedStart - optimalStart) * maxTransp;
            //*/

        }
        else if ((_temp > optimalStart) && (_temp <= optimalFinish)) //Ок температура. Прозрачный
        {
            ///*
            newColor.r = 0;
            newColor.g = 1;
            newColor.b = 0;
            newColor.a = 0;
            //*/

        }
        else if ((_temp > optimalFinish) && (_temp <= allowedFinish)) //Допустимо тепло. Красные
        {
            ///*
            newColor.r = (_temp - optimalFinish) / (allowedFinish - optimalFinish); //Рост
            newColor.g = 1;
            newColor.b = 0;
            newColor.a = (_temp - optimalFinish) / (allowedFinish - optimalFinish) * maxTransp;
            //*/

        }
        else if ((_temp > allowedFinish) && (_temp <= absHeat)) // Недопустимо жарко. Оранжевый
        {

            newColor.r = 1;
            newColor.g = (_temp - absHeat) / (allowedFinish - absHeat); //Падение
            newColor.b = 0;
            newColor.a = maxTransp;
        }
        else if (_temp > absHeat)//Выше предела
        {
            newColor.r = 1;
            newColor.g = 0;
            newColor.b = 0;
            newColor.a = maxTransp;
        }

        return (newColor);
    }

    public Color TempToColor(float _temp)
    {
        return TempToColor(_temp, "servt");
    }

    /*
    public float Convert (float data, string dataType)
    {
        return data * coefficients[dataType];
        /*
        foreach (KeyValuePair<string, float> keyValue in coefficients)
        {
            
            Console.WriteLine(keyValue.Key + " - " + keyValue.Value);
        }
        
    }
    */

    public void ValidateSettings()
    {
        for (int j = 0; j < tSettings.Length; j++)
        {
            List<Pairs> set = tSettings[j].set;
            GradientColorKey[] colorKey = new GradientColorKey[set.Capacity];
            GradientAlphaKey[] alphaKey = new GradientAlphaKey[set.Capacity];
            
            for (int i = 0; i < set.Capacity; i++)
            {
                Pairs t = set[i];
                t.threshold -= set[0].threshold;
                t.threshold /= set[set.Capacity - 1].threshold - set[0].threshold;
                if (t.threshold < 0f)
                    t.threshold = 0f;
                if (t.threshold > 1.0f)
                    t.threshold = 1.0f;
                colorKey[i].color = t.col;
                colorKey[i].time = t.threshold;
                alphaKey[i].alpha = t.col.a;
                alphaKey[i].time = t.threshold;
            }
            tSettings[j].grad.SetKeys(colorKey, alphaKey);
        }
    }

    public Color TempToColor(float _temp, string generalType)
    {
        TempSettings foundSettings =  new TempSettings();
        foundSettings.name = "NULL";
        foreach (TempSettings ts in tSettings)
        {
            if (ts.name == generalType)
            {
                foundSettings = ts;
                break;
            }
        }
        if (foundSettings.name == "NULL")
        {
            Debug.LogError("Can't find settings named "+ generalType + ". Using default " + tSettings[0].name);
            foundSettings = tSettings[0];
        }

        List<Pairs> set = foundSettings.set;
        float f = (_temp - set[0].threshold) / (set[set.Capacity - 1].threshold - set[0].threshold);
        //Debug.Log(_temp.ToString() + "-" + set[0].threshold.ToString() + ")/(" + set[set.Capacity - 1].threshold.ToString() + "-" + set[0].threshold.ToString() + ") = " + f);
        return foundSettings.grad.Evaluate(f);
    }

}
