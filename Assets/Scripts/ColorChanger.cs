using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    //Настройки цветов. Текущие - для теплого периода.
    static private float absCold = 11f;
    static private float allowedStart = 21f;
    static private float optimalStart = 23f;
    static private float optimalFinish = 25f;
    static private float allowedFinish = 28f;
    static private float absHeat = 38f;

    public float temp = 23f;
    private Material material = null; //Устанавливается не префабной связью а в Awake ввиду того что иначе изменяется глобальный материал
   
    public static Color TempToColor(float _temp)
    {
        Color newColor = new Color();

        if (_temp <= absCold) //Ниже предела
        {
            newColor.r = 0;
            newColor.g = 0;
            newColor.b = 1;
        }
        else if ((_temp > absCold) && (_temp <= allowedStart)) //Недопустимо холодно.
        {
            newColor.r = 0;
            newColor.g = (_temp - absCold) / (allowedStart - absCold); //Рост
            newColor.b = 1;
        }
        else if ((_temp > allowedStart) && (_temp <= optimalStart)) // допустимо холодно.
        {
            newColor.r = 0;
            newColor.g = 1;
            newColor.b = (_temp - optimalStart) / (allowedStart - optimalStart); //Падение
        }
        else if ((_temp > optimalStart) && (_temp <= optimalFinish)) //Ок температура. Зеленый?
        {
            newColor.r = 0;
            newColor.g = 1;
            newColor.b = 0;
        }
        else if ((_temp > optimalFinish) && (_temp <= allowedFinish)) //Допустимо тепло. Красные
        {
            newColor.r = (_temp - optimalFinish) / (allowedFinish - optimalFinish); //Рост
            newColor.g = 1;
            newColor.b = 0;
        }
        else if ((_temp > allowedFinish) && (_temp <= absHeat)) // Недопустимо жарко. Оранжевый
        {
            newColor.r = 1;
            newColor.g = (_temp - absHeat) / (allowedFinish - absHeat); //Падение
            newColor.b = 0;
        }
        else if (_temp > absHeat)//Выше предела
        {
            newColor.r = 1;
            newColor.g = 0;
            newColor.b = 0;
        }

        return (newColor);
    }

    public void SetTemp(float _temp)
    {
        temp = _temp;
        Color newColor;
        Color oldColor = material.color;
        newColor = TempToColor(temp);
        oldColor.r = newColor.r;
        oldColor.g = newColor.g;
        oldColor.b = newColor.b;
        material.SetColor("_Color", oldColor);
    }

    public void SetTempCold()
    {
        SetTemp(15f, 1f);
    }

    public void SetTempWarm()
    {
        SetTemp(30f, 10f);
    }

    public void SetTempOK()
    {
        SetTemp(24f, 5f);
    }

    public void SetTemp(float _temp, float _time)
    {
        if (_time <= 0)
            SetTemp(_temp);
        else
        {
            StartCoroutine(SmoothChange(_temp, _time));
        }
    }

    IEnumerator SmoothChange(float _temp, float _time)
    {
        //Вроде работает, но мне не нравится.
        //Debug.Log("In a Coroutine");
        float delta = 0;
        float initialTemp = temp;
        while (Mathf.Abs(temp - _temp) >= 0.1)
        {
            //Debug.Log("In a cycle");
            delta = Time.deltaTime * (_temp - initialTemp) / _time;
            //Debug.Log(delta);
            temp += delta;
            SetTemp(temp);
            yield return null;
        }

    }

    private void Awake()
    {
        material = this.GetComponent<Renderer>().material;
        SetTemp(temp); //Временный костыль для демонстрации
    }

}