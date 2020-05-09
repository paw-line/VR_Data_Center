using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarVisualiser : MonoBehaviour
{
    /*
    Визуализатор-столбец, меняющий свою высоту (и мб цвет) в зависимости от значения данных
    У него есть минимальная и максимальная высота, за которые будет выходить хуже.
         
        Увеличивать куб нужно будет только 
    */

    private GameObject target = null;
/*
    [SerializeField]
    private float absMinHeight = 0.05f;
    [SerializeField]
    private float absMaxHeight = 1.5f;
    */
    [SerializeField]
    private float minHeight = 0.1f;
    //[SerializeField]
    private float maxHeight = 1f;

    [SerializeField]
    private float minData = 10f;
    [SerializeField]
    private float maxData = 100f;

    private Color defColor;
    private Material material = null;
    private float updateTime = 1f;              ///< Время обновления объекта
    public DataSource source;                   ///< Источник данных, контролирующий цвет. Присваевается в сцене до начала работы скрипта.
    

    private void SetHardHeight(float h)
    {
        Vector3 scale = target.transform.localScale;
        scale.y = h;
        target.transform.localScale = scale;
    }

    private void SetHeight(float data)
    {
        
        if (data <= minData)
        {
            SetHardHeight(minHeight);
            Color low = new Color(0, 1, 1);
            material.SetColor("_Color", low);
        }
        else if ((data > minData) && (data < maxData))
        {
            float normData = data / (maxData + minData);
            float h = normData * (maxHeight + minHeight) + minHeight;
            SetHardHeight(h);
            material.SetColor("_Color", defColor);

        }
        else if (data >= maxData)
        {
            SetHardHeight(maxHeight);
            Color high = new Color(1, 0, 0);
            material.SetColor("_Color", high);
        }

    }

    public void SetHeight(float _h, float _time)
    {
        if (_h <= 0)
            SetHeight(_h);
        else
        {
            StartCoroutine(SmoothChange(_h, _time));
        }
    }

    /** \brief Сопрограмма, для плавного изменения цвета объекта.  
     * \param [in] _temp Устанавливаемая температура
     * \param [in] _time Время изменения
     * 
     * Что у нас есть:\n
     *   •	Целевой RGB цвет \n
     *   •	Время смены \n
     *   •	Изначальный RGB цвет \n
     *   •	DeltaTime – время смены кадра в Unity \n
     *   
     *   RGB цвет состоит из трёх компонент, собственно R G и B (ещё есть альфа-канал, но в данном случае он не важен). \n
     *   Для плавной смены цвета нужно чтобы каждый кадр значение цвета менялось в сторону целевого цвета на величину 
     *   пропорциональную DeltaTime. Должна соблюдаться пропорция: \n
     *      DeltaColor/(ЦелевойЦвет-ИзначальныйЦвет) = DeltaTime / ВремяСмены \n
     *   Где DeltaColor – изменение цвета за кадр \n
     *   Таким образом  \n
     *   DeltaColor = DeltaTime * (ЦелевойЦвет-ИзначальныйЦвет) / ВремяСмены \n
     *   При линейной плавной смене цвета таких дельты будет 3: для каждого из каналов RGB. Если же смена нелинейная, 
     *   а идет как, например, в нашем случае через зеленый, то дельтировать нужно контролирующий параметр, то есть температуру. 
     */
    IEnumerator SmoothChange(float _h, float _time)
    {
        //Вроде работает, но мне не нравится.
        //Debug.Log("In a Coroutine");
        float delta = 0;
        float initialH = target.transform.localScale.y;
        float h = target.transform.localScale.y;
        while (Mathf.Abs(h - _h) >= 0.01)
        {
            //Debug.Log("In a cycle");
            delta = Time.deltaTime * (_h - initialH) / _time;
            //Debug.Log(delta);
            h += delta;
            SetHeight(h);
            yield return null;
        }

    }

    /** \brief Сопрограмма-таймер основного цикла.
      * \param [in] _time Время обновления
      * 
      * Отвечает за регулярное обновление температуры и цвета объекта в соотвествии с данными источника. \n
      * В бесконечном цикле внутри сопрограммы из источника данных source берется значение температуры, 
      * после чего вызываетася метод SetTemp на время равное половине времени обновления. 
      * После этого производится ожидание на время обновления до повторения цикла. 
    */
    IEnumerator DelayedUpdate(float _time)
    {
        yield return new WaitForSeconds(_time);
        while (true)
        {
            //SetHeight(source.GetData(), _time / 2);
            SetHeight(source.GetData());
            yield return new WaitForSeconds(_time);
        }

    }


    /** \brief Установочная функция, вызываемая в момент активации объекта на сцене. \n
    * В первую очередь в ней устанавливается ссылка на материал ренерера родительского объекта. 
    * Если это делать более традиционным путем - префабной связью - скриптом изменяется глобальный материал всех визуализаторов данного типа. \n
    * После установки материала начинается исполнение сопрограммы DelayedUpdate с частотой обновлений в updateTime. 
    */
    private void Awake()
    {
        material = this.GetComponent<Renderer>().material;
        defColor = this.GetComponent<Renderer>().material.color;
        target = this.gameObject;
        maxHeight = target.transform.localScale.y;
        StartCoroutine(DelayedUpdate(updateTime));
    }
}
