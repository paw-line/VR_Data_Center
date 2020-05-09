using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonitorController : MonoBehaviour
{
    [SerializeField]
    private Canvas tv = null;
    [SerializeField]
    private GameObject element = null;

    private ScrollRect target  = null;

    private List<DataSource> sources;
    private List<ElementController> elements;

    private List<Callout> alarms;

    /**
     * Контролировать закрытие
     * Открываться по команде с контроллера на сервере. 
     * Собрать все ссылки на привязанные к стойке датасурсы
     * Отключать иконки тревоги при вызове экрана. 
     * 
     * Генерироватц список
     * 
     * ...
     * type
     * name
     * data
     */

    private void CollectSources()
    {
        DataSource[] temp = this.gameObject.transform.GetComponentsInChildren<DataSource>();
        sources = new List<DataSource>(temp);
        //Debug.Log(sources.Count);
    }

    private void CollectAlarms()
    {
        Callout[] temp = this.gameObject.transform.GetComponentsInChildren<Callout>(true);
        alarms = new List<Callout>(temp);
        //Debug.Log(alarms.Count);
    }

    public void TurnAlarms(bool on)
    {
        foreach (Callout i in alarms)
        {
            i.gameObject.SetActive(on);
        }
    }

    public void TurnTV(bool on)
    {
        tv.gameObject.SetActive(on);
        TurnAlarms(!on);
    }

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

    public void CreateElement()
    {
        CreateElement("default", "Some Name", "Some cool data");
    }

    private void ClearElements()
    {
        int c = target.content.gameObject.transform.childCount;
        for (int i = 0; i < c; i++)
        {
            Destroy(target.content.gameObject.transform.GetChild(i).gameObject);        
        }
        elements = new List<ElementController>();
    }

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
    }

    private void Update()
    {
        RefreshUI();
    }


}
