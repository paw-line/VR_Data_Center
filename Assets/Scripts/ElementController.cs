using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;

public class ElementController : MonoBehaviour
{
    [SerializeField]
    private Image icon = null;
    [SerializeField]
    private TextMeshProUGUI nameText = null;
    [SerializeField]
    private TextMeshProUGUI dataText = null;


    [SerializeField]
    private Sprite tempIcon = null;
    [SerializeField]
    private Sprite humIcon = null;
    [SerializeField]
    private Sprite alarmIcon = null;
    [SerializeField]
    private Sprite defaultIcon = null;

    public void Set(string type, string name, string data)
    {
        nameText.text = name;
        dataText.text = data;
        switch (type)
        {
            case "servt":
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
    }

    public void Set()
    {
        Set("default", "Name", "Data 228.322");
    }
}
