using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * \brief Класс для контроля визуализаторов сигналов тревоги. 
 * \authors Пивко Артём
 * \version 1.0
 * \date 13.05.20
 * \warning Если source не присвоить в сцене, объект не будет функционировать!
 *  
 * Этот визуализатор занимается активацией и деактивацией объекта сигнала тревоги
 * в соотвествии с данными из источника данных DataSource source.
 * Так же осуществляет динамичное расположение цилиндра-тела выноски между начальной и конечной её точками, 
 * что избавляет от необходимости выставлять его позицию вручную.\n
 * Важно: работает не только во время работы приложения, но и в редакторе. 
 * 
 */

[ExecuteInEditMode]
public class Callout : Visualiser
{
    [SerializeField]
    private Transform start = null; ///< Точка начала выноски (обычно утапливается в объект, к которому применена выноска).
    [SerializeField]
    private Transform finish = null; ///< Точка конца выноски.
    [SerializeField]
    private GameObject cylinder = null; ///< Цилиндр, соединяющий начало и конец выноски. 
    [SerializeField]
    private GameObject model = null; ///< Компонент, который выключается и включается в зависимости от данных источника. Рекоммендуется задавать здесь сам корневой объект выноски.
    [SerializeField]
    private DataSource source = null; ///< Источник данных, в соответствии с которым работает визуализатор.


    /** \brief Метод снятия показаний с визуализатора сканером
     * \param [string] visType Тип визуализатора
     * \param [string] dataType Тип данных
     * \param [string] topic MQTT-топик данных
     * \return 1 в случае если объект активен, 0 в случае если нет. 
     * Внимание, передаваемые в метод параметры при его вызове должны стоять после ключегого слова out. 
     */
    public override string Scan(out string visType, out string dataType, out string topic)
    {
        visType = this.GetType().ToString();
        dataType = source.GetType();
        topic = source.name;
        if (model.active)
            return "Active";
        else
            return "Off";

    }

    /** \brief Функция основного цикла.
     * Отвечает за покадровое обновление позиции цилиндра, а так же за включение и выключение объекта в зависимости от данных с источника.
     */
    void Update()
    {
        UpdateCylinderPosition(cylinder, start.position, finish.position);
        if (source.GetData() == 0)
            model.SetActive(false);
        else
            model.SetActive(true);
    }


    /** \brief Метод установки позиции цилиндра между начальной и конечной точкой.  
     * \param [GameObject] cylinder Устанавливаемый цилиндр.
     * \param [Vector3] beginPoint Координаты начала
     * \param [Vector3] endPoint Координаты конца
     * 
     * Устанавливает объект cylinder на середину между точкой начала и конца, затем поворачивает его, чтобы его ось Y совпала с линией между точками, 
     * затем растягивает его так, чтобы основания объекта касались начальной и конечной точек. 
     */
    private void UpdateCylinderPosition(GameObject cylinder, Vector3 beginPoint, Vector3 endPoint)
    {
        Vector3 offset = endPoint - beginPoint;
        Vector3 position = beginPoint + (offset / 2.0f);

        cylinder.transform.position = position;
        cylinder.transform.LookAt(beginPoint);
        Vector3 localScale = cylinder.transform.localScale;
        localScale.z = (endPoint - beginPoint).magnitude;
        cylinder.transform.localScale = localScale;
    }

}
