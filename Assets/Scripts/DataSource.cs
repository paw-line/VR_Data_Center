using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * \brief Класс источника данных для визуализаторов.
 * \authors Пивко Артём
 * \version 1.1
 * \date 19.05.20
 * \warning Для работы объекта на сцене требуется объект Distributor
 *  
 * Класс, хранящий текущие данные с сервера. По сути является интерфейсом ко всей системе получения данных для визуализаторов. \n
 * Данные источника задаются присутствующим на сцене объектом Distributor в соответствии с MQTT-топиком источника, который является именем этого объекта. 
 * Может функционировать в двух вариантах: в качестве источника данных непосредственно по прямой ссылке с визуализатора, либо как объемный источник данных. 
 * Для работы во втором режиме задается доверительный радиус источника, который определяет в сфере какого радиуса данные с этого источника актуальны. \n
 * Тип данных - дополнительный фильтр, позволяющим разным объемным визуализаторам отображать разные типы данных. 
 */

public class DataSource : MonoBehaviour
{
    [SerializeField]
    private float data = 0; ///< Текущие данные источника
    [SerializeField]
    private float radius = 0; ///< Доверительный радиус источника
    [SerializeField]
    private string type = null; ///< Тип данных источника
    [SerializeField]
    private bool isAutonomous = false; ///< Если истинна, то объект не пытается переименоваться в Awake

    /** \brief Метод установки значения и типа источника.
     * \param [float] _data Данные
     * \param [string] _type Тип данных
     */
    public void Set(float _data, string _type)
    {
        data = _data;
        type = _type;
    }

    /** \brief Метод установки значения источника.
     * \param _data Устанавливаемые данные
     */
    public void SetData(float _data)
    {
        data = _data;
    }

    /** \brief Метод получения текущих данных источника
     * \return Данные источника 
     */
    public float GetData()
    {
        return data;
    }

    /** \brief Метод получения доверительного радиуса источника
     * \return Доверительный радиус источника 
     */
    public float GetRadius()
    {
        return radius;
    }

    /** \brief Метод получения типа данных источника
     * \return Тип данных источника 
     */
    public string GetType()
    {
        return type;
    }

    /** \brief Метод инициализации объекта
     * В случае если источник является дочерним объектом, то объект переименовывается в полный путь до объекта. 
     */
    private void Awake()
    {
        if (!isAutonomous)
        {
            if (transform.parent != null)
            {
                //this.gameObject.name = Distributor.rootTopic + "/" + transform.parent.gameObject.name + "/" + this.name;
                this.gameObject.name = transform.parent.gameObject.name + "/" + this.name;
            }
            else
            {
                Debug.LogError("Object" + this.name + " isn't marked as autonomous, but dont have a parent.");
            }
        }
        
        //StartCoroutine(DelayedInit(3f));
    }

    /** \brief Метод отложенной инициализации объекта
     * Требуется в случае если дистрибутор не успевает инициализироваться до источника данных. В данной версии не применяется. 
     */
    IEnumerator DelayedInit(float _time)
    {
        yield return new WaitForSeconds(_time);
        this.gameObject.name = transform.parent.gameObject.name + "/" + this.name;
    }

    /** \brief Служебная функция, рисующая каркас сферы действия источника. */
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
