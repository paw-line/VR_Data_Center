using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;
using UnityEngine.UI;
using TMPro;

/**
 * \brief Класс для контроля лазерного указателя, сканера и меню.
 * \authors Пивко Артём
 * \version 1.0
 * \date 13.05.20
 * \warning  ???
 *  
 * Этот класс занимается контролем инструментов взаимодействия пользователя с моделью: лазерного указателя для управления меню,
 * сканера показаний визуализаторов и наручного меню. Для корректной работы скрипт требуется разместить на том же объекте, где находится скрипт руки виртуального аватара. 
 * 
 */

[RequireComponent(typeof(SteamVR_LaserPointer))]
public class LaserPointerController : MonoBehaviour
{
    private SteamVR_LaserPointer target; 		///< Ссылка на скрипт SteamVR_LaserPointer на данной руке
    private SteamVR_Input_Sources handType;		///< Ссылка на текущую руку

    public SteamVR_Action_Boolean laserInput;	///< Действие, включающее лазер
    [SerializeField]
    private Canvas scanerCanvas = null;			///< -Ссылка на канвас сканера на данной руке. Должна быть задана в редакторе. 

    public SteamVR_Action_Boolean menuInput;	///< Действие, включающее меню
    [SerializeField]
    private Canvas menuCanvas = null;			///< Ссылка на канвас меню на данной руке. Должна быть задана в редакторе. 

    private string visType, dataType, topic;	///< Переменыне для временного хранения данных сканера. 
    private float data;							///< Переменыне для временного хранения данных сканера. 

   
    private TextMeshProUGUI[] text = null;		///< Массив ссылок на блоки интерфейса сканера.
    private int n = 4;							///< Размер массива ссылок
	
	/** \brief Метод инициализации объекта
     * В методе получется ссылка на SteamVR_LaserPointer, на скрипт VR-руки Hand и на текстовые блоки интерфейса сканера.
     */
    void Awake()
    {
        target = this.GetComponent<SteamVR_LaserPointer>();
        if (target == null)
            Debug.LogError("На объекте не обнаружено лазерного указателя.");
        target.active = false;

        Hand hand = this.GetComponent<Hand>();
        if (hand == null)
            Debug.LogError("На объекте не обнаружено Руки O_o");
        handType = hand.handType;
        
        if (scanerCanvas.transform.childCount < n)
        {
            Debug.LogError("Наручный канвас не имеет достаточного количества текстовых блоков");
            text = null;
        }
        else
        {
            text = new TextMeshProUGUI[n];
            
            for (int i = 0; i < n; i++)
            {
                text[i] = scanerCanvas.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
                if (text[i] == null)
                {
                    Debug.LogError("В блоках наручного канваса не обнаружены компоненты TextMeshProUGUI");
                    text = null;
                    break;
                }
                text[i].text = "";
            }
            
        }
        scanerCanvas.gameObject.SetActive(false);
    }

	/** \brief Метод сканирования визуализаторов
     * Метод производит проекцию луча вдоль отображаемого SteamVR_LaserPointer луча, 
	 * вычленяет из объекта, пересеченного лучом компонент, наследованный от Visualiser. 
	 * После чего производится вызов метода Scan и вывод полученных данных на интерфейс сканера.
     */
    private void Scan()
        {
        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        bool bHit = Physics.Raycast(raycast, out hit);
        //string visType, dataType, topic;

        Visualiser targetedVis = hit.transform.gameObject.GetComponent<Visualiser>();
        if (targetedVis == null)
        {
            targetedVis = hit.transform.GetComponentInParent<Visualiser>();
        }


        if (targetedVis == null)
        {
            Debug.Log("Сканируется не визуализатор");
            for (int i = 0; i < n; i++)
            {
               text[i].text = "";
            }
            return;
        }
        else
        {
            data = targetedVis.Scan(out visType, out dataType, out topic);
            text[0].text = visType;
            text[1].text = dataType;
            text[2].text = data.ToString();
            text[3].text = topic;
        }

    }

	/** \brief Метод основного цикла
     * Производит включение лазера и меню при нажатии на кнопки и выключение при отпускании.
     */
    void Update()
    {
        if (laserInput.GetState(handType))
        {
            target.active = true;
            //Debug.Log("Включаю лазер");
            scanerCanvas.gameObject.SetActive(true);
            Scan();
            
        }
        else
        {
            target.active = false;
            //targetCanvas.gameObject.SetActive(false);
            //Debug.Log("Выключаю лазер");
        }

        if (menuInput.GetState(handType))
        {
            menuCanvas.gameObject.SetActive(true);
        }
        else
        {
            menuCanvas.gameObject.SetActive(false);
        }



    }
}
