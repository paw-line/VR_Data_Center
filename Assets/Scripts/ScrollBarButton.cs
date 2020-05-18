using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

/**
 * \brief Класс для прокручивания списка элементов интерфейса
 * \authors Пивко Артём
 * \version 1.0
 * \date 14.05.20
 *  
 * Этот класс используется как обработчик нажатия кнопки прокручивания списка элементов интерфейса. 
 * 
 */

public class ScrollBarButton : MonoBehaviour
{
    [SerializeField]
    private ScrollRect target = null;           ///< ScrollRect, который будет прокручивать объект. 

    public int scrollSpeed = 1;                 ///< Скорость прокручивания в количестве элементов интерфеса
    public RectTransform block = null;          ///< Ссылка на префаб элемента интерфейса. Нужна для рассчёта фактической скорости прокручивания. 
    public float blockSize = 50f;               ///< Высота элемента интерфейса. Используется если префаб не задан.
    public bool isScrollingDown = true;         ///< При истине элемент прокручивает список вних. При лжи - вверх. 

    /** \brief Метод прокручивания списка
    * Обычно вызывается кнопками прокручивания. \n
    * Проверяет задан ли подконтрольный ScrollRect, и если не задан выдает ошибку. 
    * В остальных случаях рассчитывает величину прокручивания как (высота элемента + расстояние между элементами) * скорость прокручивания. 
    */
    public void Scroll()
    {
        
        if (target == null)
        {
            Debug.LogError(transform.parent.parent.name + "/" + transform.parent.name + "/" + gameObject.name+": No target ScrollRect acquired");
            return;
        }

        if (block != null)
            blockSize = block.rect.height;

        float space = target.content.gameObject.GetComponent<VerticalLayoutGroup>().spacing;
        //Debug.Log("space=" + space.ToString());
        int blockCount = (int)Mathf.Floor(target.content.rect.height / (blockSize + space));
        //Debug.Log("blockCount=" + blockCount.ToString());
        float delta = (float)scrollSpeed / blockCount + space / target.content.rect.height;
        //Debug.Log("delta=" + delta.ToString());


        if (isScrollingDown)
        {
            target.verticalNormalizedPosition -= delta;
        }
        else
        {
            target.verticalNormalizedPosition += delta;
        }

        if (target.verticalNormalizedPosition > 1.0f)
            target.verticalNormalizedPosition = 1.0f;

        if (target.verticalNormalizedPosition < 0f)
            target.verticalNormalizedPosition = 0f;


    }
}
