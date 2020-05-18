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

        boxCollider.size = rectTransform.sizeDelta;
    }
}
