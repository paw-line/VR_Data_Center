using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * \brief Класс для изменения цвета кубов-индикаторов
 * \authors Пивко Артём
 * \version 1.0
 * \date 6.04.20
 * \warning Если source не присвоить в сцене, объект не будет функционировать!
 * \todo Сделать универсальный конвертер отдельным классом. С настройками из интерфейса юнити. 
 *  
 * Этот класс занимается динамическим изменением цвета родительского объекта 
 * в соотвествии с данными из источника данных DataSource source и 
 * настроек конвертации температуры из данного файла.
 * 
 */

public class ColorChanger : MonoBehaviour
{
    //Настройки цветов. Текущие - для теплого периода.
    static private float absCold = 11f;         ///< Температура, ниже которой не отслеживается
    static private float allowedStart = 21f;    ///< Граница нижней недопустимой температуры
    static private float optimalStart = 23f;    ///< Нижняя граница оптимальной температуры
    //---------------------MIDDLE LINE = 24--
    static private float optimalFinish = 25f;   ///< Верхняя граница оптимальной температуры
    static private float allowedFinish = 28f;   ///< Граница верхней недопустимой температуры
    static private float absHeat = 38f;         ///< Температура, выше которой не отслеживается

    static private float maxTransp = 0.4f;      ///< Максимальное значение прозрачности объекта

    private float updateTime = 5f;              ///< Время обновления объекта

    public float temp = 23f;                    ///< Текущее значение температуры объекта
    public DataSource source;                   ///< Источник данных, контролирующий цвет. Присваевается в сцене до начала работы скрипта.
    
    


    private Material material = null; ///< Ссылка на устанавливается не префабной связью а в Awake ввиду того что иначе изменяется глобальный материал

    /** \brief Метод-конвертер значения температуры в цвет. 
     * \param [in] _temp Конвертируемая температура
     * \return Конвертированный в соответствии с температурой цвет
     * Метод доступен извне класса, так как им так же пользуется для изменения цветов NodeVisualiser. \n
     * На самом деле это костыль и надо сделать универсальный конвертер отдельным классом. \n 
     * \n
     * Метод использует значения настроек цветов из этого файла как границы перехода цветов: \n 
     * • Если температура <= absCold, то цвет будет чистым синим с обычной прозрачностью \n 
     * • Если absCold <= температура < allowedStart, то цвет будет голубым и приблежаться к бирюзовому с ростом температуры. Прозрачность нормальная \n 
     * • Если allowedStart <= температура < optimalStart, то цвет будет бирюзовым и приблежаться к зеленому с ростом температуры. Прозрачность тоже будет расти до полного исчезновения \n 
     * • Если optimalStart <= температура < optimalFinish, Объект полностью прозрачен\n 
     * • Если optimalFinish <= температура < allowedFinish, то цвет будет желтым и приблежаться к оранжевому с ростом температуры. Прозрачность тоже будет падать до нормального значения \n 
     * • Если allowedFinish <= температура < absHeat, то цвет будет оранжевым и приблежаться к красному с ростом температуры. Прозрачность нормальная \n 
     * • Если температура > absCold, то цвет будет чистым красным с обычной прозрачностью \n 
     * 
     * */
    public static Color TempToColor(float _temp)
    {
        Color newColor = new Color();

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
            newColor.a = (_temp - optimalStart) / (allowedStart - optimalStart)* maxTransp;
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
            newColor.a = (_temp - optimalFinish) / (allowedFinish - optimalFinish)*maxTransp;
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

    /** \brief Метод, устанавливающий температуру и, соотвественно, цвет на объекте
     * \param [in] _temp Устанавливаемая температура
     */
    public void SetTemp(float _temp)
    {
        temp = _temp;
        Color newColor;
        Color oldColor = material.color;
        newColor = TempToColor(temp);
        oldColor.r = newColor.r;
        oldColor.g = newColor.g;
        oldColor.b = newColor.b;

        oldColor.a = newColor.a;

        material.SetColor("_Color", oldColor);
    }

    ///Метод, быстро устанавливающий недопустимо холодную температуру. Для тестов. 
    public void SetTempCold()
    {
        SetTemp(absCold + (allowedStart - absCold)/2, 1f);
    }

    ///Метод, медленно устанавливающий недопустимо холодную температуру. Для тестов. 
    public void SetTempWarm()
    {
        SetTemp(allowedFinish + (absHeat - allowedFinish) / 2, 10f);
    }

    ///Метод, со средней скоростью устанавливающий оптимальную температуру. Для тестов. 
    public void SetTempOK()
    {
        SetTemp((optimalStart+optimalFinish)/2, 5f);
    }

    /** \brief Метод, устанавливающий температуру и, соотвественно, цвет на объекте за указанное время. Интерфейс для корутины SmoothChange.
     * \param [in] _temp Устанавливаемая температура
     * \param [in] _time Время изменения
     */
    public void SetTemp(float _temp, float _time)
    {
        if (_time <= 0)
            SetTemp(_temp);
        else
        {
            StartCoroutine(SmoothChange(_temp, _time));
        }
    }

    /** \brief Сопрограмма, для плавного изменения цвета объекта.  
     * \param [in] _temp Устанавливаемая температура
     * \param [in] _time Время изменения
     * 
     * Что у нас есть:\n
     *   •	Целевой RGB цвет \n
     *   •	Время смены \n
     *   •	Изначальный RGB цвет \n
     *   •	DeltaTime – время смены кадра в Unity \n
     *   
     *   RGB цвет состоит из трёх компонент, собственно R G и B (ещё есть альфа-канал, но в данном случае он не важен). \n
     *   Для плавной смены цвета нужно чтобы каждый кадр значение цвета менялось в сторону целевого цвета на величину 
     *   пропорциональную DeltaTime. Должна соблюдаться пропорция: \n
     *      DeltaColor/(ЦелевойЦвет-ИзначальныйЦвет) = DeltaTime / ВремяСмены \n
     *   Где DeltaColor – изменение цвета за кадр \n
     *   Таким образом  \n
     *   DeltaColor = DeltaTime * (ЦелевойЦвет-ИзначальныйЦвет) / ВремяСмены \n
     *   При линейной плавной смене цвета таких дельты будет 3: для каждого из каналов RGB. Если же смена нелинейная, 
     *   а идет как, например, в нашем случае через зеленый, то дельтировать нужно контролирующий параметр, то есть температуру. 
     */
    IEnumerator SmoothChange(float _temp, float _time)
    {
        //Вроде работает, но мне не нравится.
        //Debug.Log("In a Coroutine");
        float delta = 0;
        float initialTemp = temp;
        while (Mathf.Abs(temp - _temp) >= 0.1)
        {
            //Debug.Log("In a cycle");
            delta = Time.deltaTime * (_temp - initialTemp) / _time;
            //Debug.Log(delta);
            temp += delta;
            SetTemp(temp);
            yield return null;
        }

    }


    /** \brief Сопрограмма-таймер основного цикла.
     * \param [in] _time Время обновления
     * 
     * Отвечает за регулярное обновление температуры и цвета объекта в соотвествии с данными источника. \n
     * В бесконечном цикле внутри сопрограммы из источника данных source берется значение температуры, 
     * после чего вызываетася метод SetTemp на время равное половине времени обновления. 
     * После этого производится ожидание на время обновления до повторения цикла. 
     */
    IEnumerator DelayedUpdate(float _time)
    {
        while (true) {
            SetTemp(source.GetData(), _time / 2);
            yield return new WaitForSeconds(_time);
        }

    }


    /** \brief Установочная функция, вызываемая в момент активации объекта на сцене. \n
    * В первую очередь в ней устанавливается ссылка на материал ренерера родительского объекта. 
    * Если это делать более традиционным путем - префабной связью - скриптом изменяется глобальный материал всех визуализаторов данного типа. \n
    * После установки материала начинается исполнение сопрограммы DelayedUpdate с частотой обновлений в updateTime. 
    */
    private void Awake()
    {
        material = this.GetComponent<Renderer>().material;
        //SetTemp(temp); //Временный костыль для демонстрации
        StartCoroutine(DelayedUpdate(updateTime));
    }

}