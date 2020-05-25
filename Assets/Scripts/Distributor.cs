using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * \brief Синглтон-дистрибутор данных 
 * \authors Пивко Артём
 * \version 1.1
 * \date 19.05.20
 * \warning Этот скрипт должен быть размещен на сцене в ЕДИНСТВЕННОМ экземпляре. Если его не будет, не будут работать никакие визуализаторы. Если их будет много, останется только один. 
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

    //Конец Шаблона для Синглтона

    private List<DataSource> sources;                ///< Список всех источников данных на сцене
    private List<string> sourcesnames = new List<string>(); ///< Список имен источников на сцене
    //public static string rootTopic = "/dc";         ///< Название корня в MQTT теме 


    /** \brief Функция, инициализурующая синглтон дистрибутор. 
     * Если ссылка на синглтон ещё не была инициализированна, то инициализирует её этим объектом. 
     * Если же она уже не пуста, то это значит что синглтор уже иницализирован. 
     * Тогда функция уничтожает данный объект и выводит предупреждение. \n
     * После этого производится поиск всех источников данных на сцене и ссылки на них заносятся в список sources.
     * По списку источников формируется список MQTT-топиков этих источников. MQTT-топик источника это его имя. 
     * После этого подключается к MQTT брокеру по заданному в объекте IP-адресу с заданным в объекте идентификатором. Привязывает событие получения данных к обработчику. Затем подписывается на топики. \n
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


        foreach (DataSource i in sources)
        {
            sourcesnames.Add(i.gameObject.name);
        }
        StartCoroutine(DelayedRefresh(0.5f));
    }

    IEnumerator DelayedRefresh(float _time)
    {
        yield return new WaitForSeconds(_time);
        RefreshSources();
        //Debug.Log(GetSources().ToString());
    }

    /** \brief Возвращает список источников в сцене. */
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
        sourcesnames = new List<string>();
        foreach (DataSource i in sources)
        {
            sourcesnames.Add(i.gameObject.name);
        }
        return delta;
    }


    /** \brief Метод установки данных и типа источника
     * \param [string] topic MQTT-топик источника, он же его имя. 
     * \param [float] data Данные
     * \param [string] type Тип данных
     * Производит линейный поиск по списку имен источников и при нахождении устанавливает значение и тип источника равными аргументам.
     * Если источник был найден, возвращает истину, иначе ложь. 
     */
    public bool SourceSet(string topic, float data, string type)
    {
        int c = 0;
        foreach (string i in sourcesnames)
        {
            if (i == topic)
            {
                if (type == "NULL")
                {
                    sources[c].SetData(data);
                }
                else
                {
                    sources[c].Set(data, type);
                }
                //break;
                return true;
            }
            c++;
        }
        return false;
    }

    /** \brief Метод установки данных источника
     * \param [string] topic MQTT-топик источника, он же его имя. 
     * \param [float] data Данные
     * Производит линейный поиск по списку имен источников и при нахождении устанавливает значение источника.
     * Если источник был найден, возвращает истину, иначе ложь. 
     */
     /*
    public bool SourceSet(string topic, float data)
    {
        return SourceSet(topic, data, "NULL");
    }
    */

}

