using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * \brief Родительский класс всех объектов для визуализации
 * \authors Пивко Артём
 * \version 1.0
 * \date 13.05.20
 *  
 * От этого абстрактного класса наследуются все объекты визуализации. 
 * 
 */

public abstract class Visualiser : MonoBehaviour
{
    /** \brief Универсальный метод снятия показаний с визуализатора 
     * \param [string] visType Тип визуализатора
     * \param [string] dataType Тип данных
     * \param [string] topic MQTT-топик данных
     * \return Визуализируемые данные в формате float
     * 
     * Унаследованные реализации функции обязаны будут возвращать 4 параметра: опорные данные визуализатора, тип визуализатора, тип данных, MQTT-топик данных. \n
     * Функция необходима для работы сканера визуализаторов.
     * Внимание, передаваемые в метод параметры при его вызове должны стоять после ключегого слова out. 
     */
    public abstract float Scan(out string visType, out string dataType, out string topic);

}
