using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

/**
 * \brief Класс для перемещения аватара игрока полётом
 * \authors Пивко Артём
 * \version 1.0
 * \date 13.05.20
 *  
 * Перемещение осуществляется нажатием на заданную кнопку. 
 * При её нажатии аватар игрока начинает перемещаться по направлению от шлема к руке, на которой нажата кнопка. 
 * Чем дальше рука от шлема, тем быстрее летит аватар. 
 * 
 */

public class Flymode : MonoBehaviour
{
    [SerializeField]
    private Hand leftHand;                  ///< Ссылка на левую руку аватара
    [SerializeField]
    private Hand rightHand;                 ///< Ссылка на правую руку аватара
    [SerializeField]
    private Camera VRCamera;                ///< Ссылка камеру виртуальной реальности

    public SteamVR_Action_Boolean flyAction;///< Действие SteamVR, отвечающее за полет. 
    public float speed = 10f;               ///< Скорость полёта
    public bool allowFlight = true;         ///< Разрешить/запретить передвижение полётом

    /** \brief Функция основного цикла.
     * Если полёт разрешен, то по активации действия лететь в сторону от шлема к руке. 
     */
    void Update()
    {
        if (allowFlight)
        {
            if (flyAction.GetState(leftHand.handType))
            {
                this.transform.position += (leftHand.transform.position - VRCamera.transform.position) * Time.deltaTime * speed;
            }
            if (flyAction.GetState(rightHand.handType))
            {
                this.transform.position += (rightHand.transform.position - VRCamera.transform.position) * Time.deltaTime * speed;
            }
        }
    }
}
