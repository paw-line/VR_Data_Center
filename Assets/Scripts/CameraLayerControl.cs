using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * \brief Класс, регулирующий отображение слоев на картинке с главной камеры. 
 * \authors Пивко Артём
 * \version 1.0
 * \date 13.05.20
 *  
 * Этот визуализатор занимается динамическим изменением цвета родительского объекта 
 * в соотвествии с данными из источника данных DataSource source.
 * Для конверсии используется синглтон UniversalController
 * 
 */
public class CameraLayerControl : MonoBehaviour
{
    private Camera current = null; ///< Камера, с которой работает объект.

    /** \brief Метод, выполняющийся на старте программы. Устанавливает основную камеру сцены как текущую камеру.
     */
    void Awake()
    {
        current = Camera.main;
    }


    /** \brief Метод для включения/выключения отображения слоев на основной камере.
     * \param [int] n Номер переключаемого слоя. n должен принадлежать промежутку [0,31].
     * Если n - корректный номер слоя, включает его отображение если тот был выключен, и выключает если тот был включен. 
     */
    public void NotRender(int n)
    {
        current = Camera.main;
        

        if ((n < 0)||(n > 31))
        {
            Debug.Log("Попытка выключить слой с некорректным номером:" + n.ToString());
            return;
        }

      
        if (current != null)
        {

            int l = current.cullingMask;
            l = l ^ 1 << n;
            //current.cullingMask = l;
            Camera.main.cullingMask = l;
        }
        else
            Debug.Log("Текущая камера не установлена и не обнаружена.");
        
    }
}
