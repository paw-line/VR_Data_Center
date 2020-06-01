using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;
using System;

[Serializable] public class MyDictionary3 : SerializableDictionary<string, Sprite> { }

/**
 * \brief Класс для контроля элементов интерфейса, отображающих данные
 * \authors Пивко Артём
 * \version 1.0
 * \date 13.05.20
 *  
 * Интерфейс для установки параметров элементов интерфейса. \n
 * В случае, если требуется добавление типа даных, это делается в switch'e функции set и добавлением новой иконки в блоке Icon.
 * 
 */

public class ElementController : MonoBehaviour
{
    [SerializeField]
    private Image icon = null; ///< Ссылка на контролируемую иконку. Должна быть задана в редакторе.
    [SerializeField]
    private TextMeshProUGUI nameText = null; ///< Ссылка на текстовый объект Text Mesh Pro, отображающий название. Должна быть задана в редакторе.
    [SerializeField]
    private TextMeshProUGUI dataText = null; ///< Ссылка на текстовый объект Text Mesh Pro, отображающий значение. Должна быть задана в редакторе.

    public MyDictionary3 icons;

    [SerializeField]
    private Sprite tempIcon = null; ///< Ссылка на иконку температуры. Должна быть задана в редакторе.
    [SerializeField]
    private Sprite humIcon = null; ///< Ссылка на иконку влажности. Должна быть задана в редакторе.
    [SerializeField]
    private Sprite alarmIcon = null; ///< Ссылка на иконку тревоги. Должна быть задана в редакторе.
    [SerializeField]
    private Sprite defaultIcon = null; ///< Ссылка на иконку по-умолчанию. Должна быть задана в редакторе.


    /** \brief Метод установки элемента
    * \param type Тип данных, определяющий иконку.
    * \param name Отображаемое название данных
    * \param data Отображаемые данные
    */
    public void Set(string type, string name, string data)
    {
        nameText.text = name;
        dataText.text = data;

        if (icons.ContainsKey(type))
        {
            icon.sprite = icons[type];
        }
        else
        {
            icon.sprite = defaultIcon;
        }
        /*
        switch (type)
        {
            case "servtemp":
                if (tempIcon != null)
                    icon.sprite = tempIcon;
                else
                    icon.sprite = defaultIcon;
                break;
            case "hum":
                if (tempIcon != null)
                    icon.sprite = humIcon;
                else
                    icon.sprite = defaultIcon;
                break;
            case "overallAlarm":
                if (tempIcon != null)
                    icon.sprite = alarmIcon;
                else
                    icon.sprite = defaultIcon;
                break;
            default:
                 icon.sprite = defaultIcon;
                break;
        }
        */
    }

    public void Set()
    {
        Set("default", "Name", "Data 228.322");
    }
}
