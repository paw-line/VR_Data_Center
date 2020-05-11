using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(SteamVR_LaserPointer))]
public class LaserPointerController : MonoBehaviour
{
    private SteamVR_LaserPointer target;
    private SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean input;

    private string visType, dataType, topic;
    private float data;

    [SerializeField]
    private Canvas targetCanvas = null;
    private TextMeshProUGUI[] text = null;
    private int n = 4;
    
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
        
        if (targetCanvas.transform.childCount < n)
        {
            Debug.LogError("Наручный канвас не имеет достаточного количества текстовых блоков");
            text = null;
        }
        else
        {
            text = new TextMeshProUGUI[n];
            
            for (int i = 0; i < n; i++)
            {
                text[i] = targetCanvas.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
                if (text[i] == null)
                {
                    Debug.LogError("В блоках наручного канваса не обнаружены компоненты TextMeshProUGUI");
                    text = null;
                    break;
                }
                text[i].text = "";
            }
            
        }

        targetCanvas.gameObject.SetActive(false);

    }

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

    // Update is called once per frame
    void Update()
    {
        if (input.GetState(handType))
        {
            target.active = true;
            //Debug.Log("Включаю лазер");
            targetCanvas.gameObject.SetActive(true);
            Scan();
            
        }
        else
        {
            target.active = false;
            //targetCanvas.gameObject.SetActive(false);
            //Debug.Log("Выключаю лазер");
        }

    }
}
