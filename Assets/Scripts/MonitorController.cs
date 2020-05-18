using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * \brief Класс для контроля интерфейса монитора оборудования
 * \authors Пивко Артём
 * \version 1.0
 * \date 14.05.20
 *  
 * Этот класс занимается контролем текстового интерфейса, отображающего все данные, имеющиеся о единице оборудования. \n
 * Для включения/выключения интерфейса используйте метод TurnTV через кнопки на оборудовании. 
 */

public class MonitorController : MonoBehaviour
{
    [SerializeField]
    private Canvas tv = null;                       ///< Ссылка на канвас монитора. Должна быть задана в редакторе.
    [SerializeField]
    private GameObject element = null;              ///< Ссылка на префаб элемента интерфейса. Должна быть задана в редакторе. 

    private ScrollRect target  = null;              ///< Ссылка ScrollRect, в котором размещены элементы интерфейса. Должна быть задана в редакторе. 

    private List<DataSource> sources;               ///< Список источников в поддереве этого объекта
    private List<ElementController> elements;       ///< Список элементов интерфейса, подконтрольных объекту.
    private List<Callout> alarms;                   ///< Список сигналов тревоги, которые могут перекрывать монитор 

    [SerializeField]
    private bool isChild = false;                   ///< Отметка является ли данный монитор подчиненным другому монитору

    /** \brief Метод получения списка источников в поддереве  */
    private void CollectSources()
    {
        DataSource[] temp = this.gameObject.transform.GetComponentsInChildren<DataSource>();
        sources = new List<DataSource>(temp);
        //Debug.Log(sources.Count);
    }

    /** \brief Метод получения списка тревог в поддереве  */
    private void CollectAlarms()
    {
        Callout[] temp = this.gameObject.transform.GetComponentsInChildren<Callout>(true);
        alarms = new List<Callout>(temp);
        //Debug.Log(alarms.Count);
    }

    /** \brief Метод включения/выключения тревог в поддереве 
     *  \param [bool] on Включить (True) или выключить (False)
     */
    public void TurnAlarms(bool on)
    {
        foreach (Callout i in alarms)
        {
            i.gameObject.SetActive(on);
        }
    }

    /** \brief Метод включения/выключения экрана монитора оборудования
     *  \param [bool] on Включить (True) или выключить (False) 
     */
    public void TurnTV(bool on)
    {
        tv.gameObject.SetActive(on);
        TurnAlarms(!on);
    }

    /** \brief Метод создания и начальной инициализации элемента интерфейса на экране
     * \param [string] type Начальный отображаемый тип данных
     * \param [string] name Начальное отображаемое название измерения
     * \param [string] data Начальные отображаемые данные
     * Создает элемент интерфейса на экране, установка его полей переданными в функцию параметрами и регулировка высоты зоны, в которой элемент размещается.
     */
    private void CreateElement(string type, string name, string data)
    {
        //Создание нужного элемента там где нужно
        GameObject temp = Instantiate(element);
        temp.transform.SetParent(target.content.gameObject.transform);
        temp.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        Vector3 coord = temp.GetComponent<RectTransform>().anchoredPosition3D;
        coord.z = 0;
        //Debug.Log("Wanna: " + coord.ToString());
        temp.GetComponent<RectTransform>().anchoredPosition3D = coord;
        //Debug.Log("Gotta: " + temp.GetComponent<RectTransform>().position.ToString());

        temp.SetActive(true);
        temp.name = name;

        //Инициализация элемента
        ElementController tempCon = temp.GetComponent<ElementController>();
        tempCon.Set(type, name, data);
        elements.Add(tempCon);

        //Задание высоты зоне
        float l = element.GetComponent<RectTransform>().rect.height + target.GetComponentInChildren<VerticalLayoutGroup>().spacing;
        if (l * elements.Count > target.content.GetComponent<RectTransform>().rect.height)
        {
            Vector2 a = target.content.GetComponent<RectTransform>().sizeDelta;
            a.y = l * elements.Count;
            target.content.GetComponent<RectTransform>().sizeDelta = a;
        }
    }

    /** \brief Метод создания и инициализации элемента интерфейса заданными данными */
    public void CreateElement()
    {
        CreateElement("default", "Some Name", "Some cool data");
    }

    /** \brief Метод очистки всех подъобъектов списка элементов интерфейса.*/
    private void ClearElements()
    {
        int c = target.content.gameObject.transform.childCount;
        for (int i = 0; i < c; i++)
        {
            Destroy(target.content.gameObject.transform.GetChild(i).gameObject);        
        }
        elements = new List<ElementController>();
    }

    /** \brief Метод полного обновления элементов интерфейса.
     * Полностью уничтожает и реинициализирует все элементы интерфейса монитора.
     * Применяется для инициализации монитора.
     * Не рекомендуется использовать во время работы программы из-за медлительности, используйте RefreshUI()
     */
    private void UpdateList()
    {
        ClearElements();
        string type, name, data;
        foreach (DataSource i in sources)
        {
            type = i.GetType();
            name = i.gameObject.name;
            data = i.GetData().ToString();
            CreateElement(type, name, data);
        }
    }

    /** \brief Метод обновления элементов интерфейса.
     * В отличии UpdateList() не уничтожает элементы перед реинициализацией, а только обновляет их значения.
     * Осторожно, не добавляет новых элементов, но может удалять старые. 
     * Можно использовать в процессе работы программы. 
     */
    private void RefreshUI()
    {
        string type, name, data;
        int cs = sources.Count;
        int ce = elements.Count;
        int i = 0;
        //Проходим по источникам и обновляем в соответствии с ними элементы интерфейса
        for (i = 0; i < cs; i++)
        {
            type = sources[i].GetType();
            name = sources[i].gameObject.name;
            data = sources[i].GetData().ToString();
            if (i < ce) //Если элемент существует, задаем
            {
                elements[i].Set(type, name, data);
            }
            else //Если элемента не существует, создаем
            {
                Debug.Log("Создаю элемент");
                CreateElement(type, name, data);
            }
        }

        ce = elements.Count;
        while (i < ce - 1)//Если остались лишние элементы, удаляем их
        {
            Debug.Log("Удаляю элемент");
            Destroy(elements[i].gameObject);
            i++;
        }

    }

    /** \brief Метод получения списка визуализаторов тревог данного объекта
     * Используется дочерними мониторами. 
     */
    public List<Callout> GetAlarms()
    {
        return alarms;
    }

    /** \brief Сопрограмма отложенного сбора списка тревог
     * Используется дочерними мониторами для получения полного списка тревог, перекрывающих монитор. 
     * Задержка нужна для того чтобы вышестоящий объект успел собрать свой список тревог.
     */
    IEnumerator DelayedAlarmCollect()
    {
        yield return new WaitForSeconds(3f);
        MonitorController parent = transform.parent.GetComponentInParent<MonitorController>();
        if (parent == null)
        {
            Debug.LogError("No parent found for server monitor controller. " + this.name + " cannot deactivate alarms");
        }
        else
        {
            alarms = parent.GetAlarms();
        }
        //Debug.Log(parent.gameObject.name);
        tv.transform.position = parent.tv.transform.position;

    }

    /** \brief Метод инициализации объекта. 
     * Собирает списки источников и тревог, ищет в поддереве подконтрольный ScrollRect и запоминает ссылку на него, затем запускает метод обновления интерфеса.
     * В случае если объект дочерний, запускает сопрограмму отложенного сбора списка тревог.
     */
    private void Awake()
    {
        CollectSources();
        CollectAlarms();
        target = tv.GetComponentInChildren<ScrollRect>();
        ClearElements();
        elements = new List<ElementController>();

        //Для тестов
        CreateElement("servt", "First Temp", "228322");
        CreateElement("hum", "First Humility", "666%");
        CreateElement("overallAlarm", "ALARM!!!1!", "BROKEN!");
        CreateElement("something", "Idk", "data");
        //Debug.Log("Gotta for real: " + elements[0].GetComponent<RectTransform>().anchoredPosition3D.ToString());

        UpdateList();
        //RefreshUI();

        if (isChild)
        {
            StartCoroutine(DelayedAlarmCollect());
        }
    }

    /** \brief Метод основного цикла.
     * Каждый кадр обновляет значения элементов интерфейса. 
     */
    private void Update()
    {
        RefreshUI();
    }


}
