using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonitorController : MonoBehaviour
{
    [SerializeField]
    private Canvas tv = null;

    private List<DataSource> sources;

    /**
     * Контролировать закрытие
     * Открываться по команде с контроллера на сервере. 
     * Собрать все ссылки на привязанные к стойке датасурсы
     * Генерироватц список
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

    public void OpenTV()
    {
        Debug.Log("Включаю телевизор");
        tv.enabled = true;
    }

    public void CloseTV()
    {
        Debug.Log("Выключаю телевизор");
        tv.enabled = false;
    }

    private void Awake()
    {
        CollectSources();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
