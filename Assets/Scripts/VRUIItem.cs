using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * \brief Класс для автоматизированного создания коллайдеров на элементах интерфейса
 * \authors Пивко Артём
 * \version 1.0
 * \date 14.05.20
 * \warning Требует компонента RectTransform на объекте для работы
 *
 *  Коллайдеры требуются для управления интерфейсом VR контроллерами. 
 *
 */
[RequireComponent(typeof(RectTransform))]
public class VRUIItem : MonoBehaviour
{
    private BoxCollider boxCollider;         ///< Ссылка на создаваемый коллайдер
    private RectTransform rectTransform;     ///< Ссылка на RectTransform объекта, к которому создается коллайдер

    /** \brief Установочная функция */
    private void OnEnable()
    {
        ValidateCollider();
    }

    /** \brief Установочная функция при обновлении объекта*/
    private void OnValidate()
    {
        ValidateCollider();
    }

    /** \brief Установочная функция создания или проверки правильности коллайдера. */
    private void ValidateCollider()
    {
        rectTransform = GetComponent<RectTransform>();
        

        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        if (this.gameObject.name == "ButtonInspect")
        {
            // Debug.Log(rectTransform.parent.parent.name + rectTransform.parent.GetComponent<Canvas>().ToString());
            //Debug.Log(rectTransform.parent.GetComponent<RectTransform>().rect.width);
            boxCollider.size = new Vector2(rectTransform.GetComponentInParent<RectTransform>().rect.width, rectTransform.GetComponentInParent<RectTransform>().rect.height);
        }
        else
            boxCollider.size = rectTransform.sizeDelta;
    }
}
