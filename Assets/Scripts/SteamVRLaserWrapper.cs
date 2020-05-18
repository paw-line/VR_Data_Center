using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR.Extras;

/**
 * \brief Класс, исправляющий работу SteamVR_LaserPointer
 * \authors Пивко Артём
 * \version 1.0
 * \date 14.05.20
 *  
 * Этот класс используется для исправления не полностью функционирующего лазерного указателя SteamVR. 
 * Должен быть размещен на том же объекте что и SteamVR_LaserPointer. 
 * 
 */

[RequireComponent(typeof(SteamVR_LaserPointer))]
public class SteamVRLaserWrapper : MonoBehaviour
{
    private SteamVR_LaserPointer steamVrLaserPointer;       ///< Ссылка на SteamVR_LaserPointer

    /** \brief Установочная функция, вызываемая в момент активации объекта на сцене. \n
    * Получает ссылку на SteamVR_LaserPointer и привязывает кастомные обработчики событий.
    */
    private void Awake()
    {
        steamVrLaserPointer = gameObject.GetComponent<SteamVR_LaserPointer>();
        steamVrLaserPointer.PointerIn += OnPointerIn;
        steamVrLaserPointer.PointerOut += OnPointerOut;
        steamVrLaserPointer.PointerClick += OnPointerClick;
    }

    /** \brief Обработчик события клика лазером    */
    private void OnPointerClick(object sender, PointerEventArgs e)
    {
        IPointerClickHandler clickHandler = e.target.GetComponent<IPointerClickHandler>();
        if (clickHandler == null)
        {
            return;
        }


        clickHandler.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    /** \brief Обработчик события окончания касания интерактивного элемента и лазера*/
    private void OnPointerOut(object sender, PointerEventArgs e)
    {
        IPointerExitHandler pointerExitHandler = e.target.GetComponent<IPointerExitHandler>();
        if (pointerExitHandler == null)
        {
            return;
        }

        pointerExitHandler.OnPointerExit(new PointerEventData(EventSystem.current));
    }

    /** \brief Обработчик события начала касания интерактивного элемента и лазера*/
    private void OnPointerIn(object sender, PointerEventArgs e)
    {
        IPointerEnterHandler pointerEnterHandler = e.target.GetComponent<IPointerEnterHandler>();
        if (pointerEnterHandler == null)
        {
            return;
        }

        pointerEnterHandler.OnPointerEnter(new PointerEventData(EventSystem.current));
    }
}
