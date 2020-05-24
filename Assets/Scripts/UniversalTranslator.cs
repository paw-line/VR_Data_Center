using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable] public class MyDictionary2 : SerializableDictionary<string, string> { }
/*
 * 1) Переводить технические названия а ля SupplyC на русский в Температура источника словарем typeToRussianDictionary
 * 2) Превращать тип в символ после единицы измерения как для типов proc писать 10% словарем generaTypeToUnitDictionary
 * 3) В случае объемных визуализаторов переводить тип а ля PythonRoomT1 в temp словарем typeToGeneraTypeDictionary

     
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


    public MyDictionary2 typeToRussianDictionary;
    public MyDictionary2 generalTypeToUnitDictionary;
    public MyDictionary2 typeToGeneraTypeDictionary;

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
            return answer;
        }
        
    }

    public string TransGeneralTypeToUnit(string type)
    {
        if (generalTypeToUnitDictionary.ContainsKey(type))
        {
            return generalTypeToUnitDictionary[type];
        }
        else
        {
            Debug.LogError("General Type not found");
            return "";
        }
    }

    private void Start()
    {
        //string tp = "PythonRoomT3";
        //Debug.Log(TransTypeToGeneralType(tp));
    }


}
