using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class FontFitter : MonoBehaviour
{
    //[SerializeField]
    private TextMeshProUGUI text = null; 

    public string disptext = "Wololo";

    public const float magicFontResizingFactor = 1.882f; //Константа для перевода кегля в юниты Юнити. 1 кегль = 1.882 юнитов по горизонтали. 

    private void Fit()
    {
        //Computing cell cize
        int length = disptext.Length;//count.ToString("#0.##").Length;
        RectTransform rt = transform.GetComponent(typeof(RectTransform)) as RectTransform;
        float nsize = rt.rect.width / (length);
        //Debug.Log("Ширина родителя:");
        //Debug.Log(rt.rect.width);


        float heigthcap = rt.rect.height;// imgSize >= 1 ? imgSize * nsize : nsize;

        if (heigthcap < nsize) //Checking if we dont have enough vertical space
        {
            nsize = heigthcap;// rt.rect.height / 2;
            Debug.Log(nsize);
        }

        //Resizing
        //rt = img.gameObject.transform.GetComponent(typeof(RectTransform)) as RectTransform;
        //rt.sizeDelta = new Vector2(nsize * imgSize, nsize * imgSize);
        //rt.anchoredPosition = new Vector2(rt.rect.width / 2, 0);
        text.fontSize = nsize * magicFontResizingFactor;
        text.text = disptext; //count.ToString("#0.##");
        rt = text.gameObject.transform.GetComponent(typeof(RectTransform)) as RectTransform;
        rt.sizeDelta = new Vector2(nsize * length, heigthcap);// nsize * length);
        //rt.anchoredPosition = new Vector2(rt.rect.width / 2 + imgSize * nsize, 0);
    }


    /*
    private void FontFit(float fontsize)
    {
        RectTransform rt = transform.GetComponent(typeof(RectTransform)) as RectTransform;
        float nsize = fontsize / magicFontResizingFactor;
        int length = count.ToString("#0.##").Length;
        ///*
        if ((length + imgSize) * nsize > rt.rect.width) //Проверка влезет ли строка с данным шрифтом в родительский объект по ширине
        {
            Debug.LogError("При использовании данного размера шрифта строка не поместится в родительский объект");
            Fit();
        }
        else if (imgSize * nsize > rt.rect.height) //Проверка влезет ли картинка по высоте
        {
            Debug.LogError("При использовании данного размера шрифта и строка не поместится в родительский объект из-за размеров картинки.");
            Fit();
        }
        else //Всё нормально
        {//
            //imgSize = imgSize_;
            //Resizing
            rt = img.transform.GetComponent(typeof(RectTransform)) as RectTransform;
            rt.sizeDelta = new Vector2(nsize * imgSize, nsize * imgSize);
            rt.anchoredPosition = new Vector2(rt.rect.width / 2, 0);
            text.fontSize = fontsize;
            text.text = count.ToString("#0.##");
            rt = text.gameObject.transform.GetComponent(typeof(RectTransform)) as RectTransform;
            rt.sizeDelta = new Vector2(nsize * length, nsize * length);
            rt.anchoredPosition = new Vector2(rt.rect.width / 2 + imgSize * nsize, 0);
        }


    }
    */

    private void Awake()
    {
        text = this.GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        disptext = text.text;
        Fit();
    }

}