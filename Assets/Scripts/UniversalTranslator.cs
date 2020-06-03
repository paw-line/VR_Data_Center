using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable] public class MyDictionary2 : SerializableDictionary<string, string> { }

/**
* \brief Класс для преобразования типов, получения единиц измерения и переводов.
* \authors Пивко Артём
* \version 1.1
* \date 27.05.20
* \warning Этот скрипт должен быть размещен на сцене в ЕДИНСТВЕННОМ экземпляре. Если его не будет, не будут работать никакие визуализаторы. Если их будет много, останется только один. \n Для корректной работы компонента требутеся заполнить словари в редакторе. 4
* \todo Сделать TranstypeToRussian
*  
* Этот класс-синглтон используется как глобальный сервис, выполняющий 3 функции: \n
* 1) Преобразование имени датчика в имя типа \n
* 2) Преобразование имени типа в символ, отражающий единицу измерения (например, символ процента "%")\n
* 3) Перевод имени датчика в понятное название
*/
public class UniversalTranslator : MonoBehaviour
{
    //Шаблон для Синглтона
    private static UniversalTranslator instance; ///< Статичная ссылка на единственный экземпляр класса

    private UniversalTranslator() ///< Конструктор 
    { }

    /** \brief Функция-интерфейс для получения синглтона
    * Возвращает ссылку на синглтон.
   */
    public static UniversalTranslator GetInstance()
    {
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
    }

    //Конец Шаблона для Синглтона


    public MyDictionary2 typeToRussianDictionary;       ///< Словарь перевода названия датчика в описание. Заполняется в редакторе
    public MyDictionary2 generalTypeToUnitDictionary;   ///< Словарь перевода типа в символ единицы измерения. Заполняется в редакторе
    public MyDictionary2 typeToGeneraTypeDictionary;    ///< Словарь перевода названия датчика в его тип. Заполняется в редакторе

    //То, что я не могу инициализировать в инспекторе(
    public TextAsset importAsset = null;
    public string exportFileName = "";
    public bool typeToRussian = false;
    public bool generalTypeToUnit = false;
    public bool typeToGeneraType = false;

    
    /*
    [SerializeField]
    private bool readFromFiles = false;
    public TextAsset fileTypeToRussianDictionary = null;
    public TextAsset fileGeneralTypeToUnitDictionary = null;
    public TextAsset fileTypeToGeneraTypeDictionary = null;
    [SerializeField]
    private bool writeToFiles = false;
    public string filename1 = "typeToRussianDictionary.txt";
    public string filename2 = "generalTypeToUnitDictionary.txt";
    public string filename3 = "typeToGeneraTypeDictionary.txt";
    */

    /** \brief Метод преобразования имени датчика в тип
     * \param type Имя датчика
     * \return Общий тип данных датчика string
     * Ищет в словаре typeToGeneraTypeDictionary ключ, полученный в аргументе type и возвращает его значение.\n
     * При заполнении словаря можно использовать специальный синтаксис: если ключ словаря начинается с символа вопросительного знака "?", 
     * всё что находится после него испольщуется не как ключ, а как строка для поиска. Если она присутствует в полученном в метод типе, возвращается значение этой пары. \n
     * Например, если как ключ поставить "?PythonRoomT", а как значение roomtemp, то при принятии аргумента "PythonRoomT5" будет возвращено roomtemp.
     */
    public string TransTypeToGeneralType(string type)
    {
        if (typeToGeneraTypeDictionary.ContainsKey(type))
        {
            return typeToGeneraTypeDictionary[type];
        }
        else
        {
            string answer = "";
            foreach (KeyValuePair<string, string> keyValue in typeToGeneraTypeDictionary)
            {
                if (keyValue.Key.Contains("?"))
                {
                    string filter = keyValue.Key.Substring(1);
                    if (type.Contains(filter))
                    {
                        answer = keyValue.Value;
                        break;
                    }
                }
            }
            if (answer == "")
            {
                Debug.LogError($"TypeToGeneral: General Type '{type}' not found, returning input type");
                return type;
            }

            return answer;
        }
        
    }


    /** \brief Метод преобразования типа в единицу измерения
     * \param type Тип данных
     * \return символ(ы) единицы измерения в формате string
     * Ищет в словаре generalTypeToUnitDictionary ключ, полученный в аргументе type и возвращает его значение.\n
     * В случае, если ключ не был найден, выводит ошибку в консоль и возвращает пустую строку, что позволяет оторбражать величину просто как числовое значение. 
     */
    public string TransGeneralTypeToUnit(string type)
    {
        if (generalTypeToUnitDictionary.ContainsKey(type))
        {
            return generalTypeToUnitDictionary[type];
        }
        else
        {
            Debug.LogError($"GeneralToUnit: General Type '{type}' not found, returning null string");
            return "";
        }
    }

    /** \brief Метод преобразования типа в единицу измерения
     * \param type Имя датчика
     * \return Переведенная строка в формате string
     * Ищет в словаре typeToRussianDictionary ключ, полученный в аргументе type и возвращает его значение.\n
     * В случае, если ключ не был найден, выводит ошибку в консоль и возвращает входную строку, что позволяет оторбражать имя датчика без перевода. 
     */
    public string TranstypeToRussian(string type)
    {
        if (typeToRussianDictionary.ContainsKey(type))
        {
            return typeToRussianDictionary[type];
        }
        else
        {
            Debug.LogError($"TypeToRussian: String '{type}' not found in dictionary, returning input");
            return type;
        }
        
    }


    private void Start()
    {
        /*
        string wololo = JsonUtility.ToJson(typeToGeneraTypeDictionary);
        Debug.Log("JSON stuff:" + wololo);
        typeToRussianDictionary = JsonUtility.FromJson<MyDictionary2>(wololo);
        string filePath = Path.Combine(Application.dataPath, "somefilename.txt");
        File.WriteAllText(filePath, "Hey look here is some text.");
        Debug.Log($"Some text has been written into the file {filePath}.");
        string contents = File.ReadAllText(filePath);
        Debug.Log($"The file's contents are {contents}.");
        */
        /*
        if (writeToFiles)
        {
            if (filename1 != "")
            {
                Debug.Log($"Writing typeToRussianDictionary into file {filename1}");
                WriteDicConfig(typeToRussianDictionary, filename1);
            }
            if (filename2 != "")
            {
                Debug.Log($"Writing generalTypeToUnitDictionary into file {filename2}");
                WriteDicConfig(generalTypeToUnitDictionary, filename2);
            }
            if (filename3 != "")
            {
                Debug.Log($"Writing typeToGeneraTypeDictionary into file {filename3}");
                WriteDicConfig(typeToGeneraTypeDictionary, filename3);
            }
        }

        if (readFromFiles)
        {
            if (fileTypeToRussianDictionary != null)
            {
                typeToRussianDictionary = ReadDicConfig(fileTypeToRussianDictionary);
            }
            if (fileGeneralTypeToUnitDictionary != null)
            {
                generalTypeToUnitDictionary = ReadDicConfig(fileGeneralTypeToUnitDictionary);
            }
            if (fileTypeToGeneraTypeDictionary != null)
            {
                typeToGeneraTypeDictionary = ReadDicConfig(fileTypeToGeneraTypeDictionary);
            }
        }
        */
    }

    public void WriteDicConfig(MyDictionary2 dic, string filename)
    {
        string serString = JsonUtility.ToJson(dic);
        string filePath = Path.Combine(Application.dataPath, filename);
        File.WriteAllText(filePath, serString);
        Debug.Log($"Dictionary has been written in file {filePath}.");
    }

    public MyDictionary2 ReadDicConfig(TextAsset inputfile)
    {
        return JsonUtility.FromJson<MyDictionary2>(inputfile.text); 
    }


}
