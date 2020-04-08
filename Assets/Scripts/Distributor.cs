using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * \brief Синглтон-дистрибутор данных 
 * \authors Пивко Артём
 * \version 1.0
 * \date 8.04.20
 * \warning Этот скрипт должен быть размещен на сцене в ЕДИНСТВЕННОМ экземпляре. Если его не будет, не будут работать никакие визуализаторы. Если их будет много, останется только один. 
 * \todo ???
 *  
 * Этот класс составляет список всех источников данных на сцене и хранит её для передачи визуализаторам. Преимущественно объемным.
 * 
 */
public class Distributor : MonoBehaviour
{
    //Шаблон для Синглтона
    private static Distributor instance; ///< Статичная ссылка на единственный экземпляр класса

    private Distributor() ///< Конструктор 
    { }

    /** \brief Функция-интерфейс для получения синглтона
     * Возвращает ссылку на синглтон.
    */
    public static Distributor GetInstance()
    {
        if (instance == null)
            Debug.LogWarning("Tried to return");
        return instance;
    }

    /** \brief Функция, инициализурующая синглтон дистрибутор. 
     * Если ссылка на синглтон ещё не была инициализированна, то инициализирует её этим объектом. 
     * Если же она уже не пуста, то это значит что синглтор уже иницализирован. 
     * Тогда функция уничтожает данный объект и выводит предупреждение. \n
     * После этого производится поиск всех источников данных на сцене и ссылки на них заносятся в список sources
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
        DataSource[] temp;
        temp = FindObjectsOfType<DataSource>();
        sources = new List<DataSource>(temp);
    }


    private List<DataSource> sources;                ///< Список всех источников данных на сцене

    public static string rootTopic = "/dc";         ///< Название корня в MQTT теме 

    /** \brief Возвращает список источников в сцене. 
    */
    public List<DataSource> GetSources()
    {
        return sources;
    }

    /** \brief Функция, обновляющая список источников на сцене. 
     * Возвращает разницу в длинне старого и нового списка. 
     */
    public int RefreshSources()
    {
        Debug.Log("Refreshing the source list. A minor lag may happen now.");
        DataSource[] temp = FindObjectsOfType<DataSource>();
        int delta = sources.Count - temp.Length;
        sources = new List<DataSource>(temp);
        return delta;
    }

    /*
    private void Awake()
    {
        DataSource[] temp;
        temp = FindObjectsOfType<DataSource>();
        sources = new List<DataSource>(temp);
    }*/
    //public List<Point> sources;
    //public List<DataSource> sources;

    //public Dictionary<string, DataSource> countries;   
}
/*
[System.Serializable]
public class Point
{
    public List<DataSource> sources;
}
*/
