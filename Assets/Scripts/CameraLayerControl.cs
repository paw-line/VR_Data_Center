using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLayerControl : MonoBehaviour
{
    private Camera current = null;

    void Awake()
    {
        current = Camera.main;
    }

    public void NotRender(int n)
    {
        current = Camera.main;
        

        if ((n < 0)||(n > 31))
        {
            Debug.Log("Попытка выключить слой с некорректным номером:" + n.ToString());
            return;
        }

      
        if (current != null)
        {
            //Debug.Log("Начинаю резать нэверных " +  Camera.main.name);

            int l = current.cullingMask;
            l = l ^ 1 << n;
            //current.cullingMask = l;
            Camera.main.cullingMask = l;
            //Debug.Log("Парезал");
        }
        else
            Debug.Log("Текущая камера не установлена и не обнаружена.");
        
    }
}
