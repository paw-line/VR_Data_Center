using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * \brief Класс, направляющий объект, которому привязан в сторону основной камеры
 * \authors Пивко Артём
 * \version 1.0
 * \date 13.05.20
 *
 *  Используется для направления иконок тревоги в сторону пользователя. 
 */
public class Billboard : MonoBehaviour
{

    [SerializeField]
    private bool isRotating3D = false; ///< Если этот параметр отмечен, вращение будет происходить по всем трём осям. Иначе только по оси Y.

    /** \brief Функция основного цикла.
     * 
     * В зависимости от параметра isRotating3D каждый кадр вращает объект либо по оси Y, либо по всем осям. 
     */
    void Update()
    {
        if (isRotating3D)
        {
            transform.LookAt(Camera.current.transform);
        }
        else
        {
            //Debug.Log(Camera.allCamerasCount);
            //Debug.Log(Camera.current.name);
            if (Camera.current != null)
            {
                Vector3 targetPosition = new Vector3(Camera.current.transform.position.x, transform.position.y, Camera.current.transform.position.z);
                transform.LookAt(targetPosition);
            }
        }
    }
}
