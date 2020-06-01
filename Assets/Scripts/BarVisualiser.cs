using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * \brief Класс для контроля столбчатых визуализаторов
 * \authors Пивко Артём
 * \version 1.0
 * \date 13.05.20
 * \warning Если source не присвоить в сцене, объект не будет функционировать!
 *  
 * Этот класс занимается контролем высоты и цвета столбца-визуализатора, которым должен являться объект, к которому прикремлен скрипт.
 * Контроль происходит в соотвествии с данными из источника данных DataSource source и внутренних настроек объекта. 
 * 
 */

public class BarVisualiser : Visualiser
{
    private GameObject target = null;       ///< Изменяемый объект
    [SerializeField]
    private float minHeight = 0.1f;         ///< Минимальная высота столбца в локальных единицах измерения. Регулируется из редактора.
    //[SerializeField]
    private float maxHeight = 1f;           ///< Максимальная высота столбца в локальных единицах измерения. Регулируется из редактора.

    [SerializeField]
    private float minData = 10f;            ///< Значение данных, при которых высота столбца будет минимальной. Регулируется из редактора.
    [SerializeField]
    private float maxData = 100f;           ///< Значение данных, при которых высота столбца будет максимальной. Регулируется из редактора.

    private float curData;                  ///< Текущее значение отображаемых данных.
    private Color defColor;                 ///< Цвет столбца в обычном состоянии. Берется из изначального цвета столбца в редакторе. 
    private Material material = null;       ///< Ссылка на материал изменяемого объекта.
    private float updateTime = 1f;          ///< Время обновления объекта
    public DataSource source;               ///< Источник данных, контролирующий цвет. Присваевается в сцене до начала работы скрипта.


    /** \brief Метод снятия показаний с визуализатора сканером
     * \param visType  Тип визуализатора
     * \param dataType Тип данных
     * \param topic MQTT-топик данных
     * \return Визуализируемые данные в формате string
     */
    public override string Scan(out string visType, out string dataType, out string topic)
    {
        UniversalTranslator tr = UniversalTranslator.GetInstance();

        visType = tr.TranstypeToRussian(this.GetType().ToString());
        dataType = tr.TranstypeToRussian(tr.TransTypeToGeneralType(source.GetType()));
        topic = source.name;
        
        return curData.ToString() + tr.TransGeneralTypeToUnit(tr.TransTypeToGeneralType(source.GetType()));
    }

    /** \brief Метод задания абсолютной высоты объекта в локальных координатах.
     * \param h Абсолютная высота
     * Используется только после предварительной обработки данных.
     */
    private void SetHardHeight(float h)
    {
        Vector3 scale = target.transform.localScale;
        scale.y = h;
        target.transform.localScale = scale;
    }

    /** \brief Метод задания высоты объекта по данным в локальных координатах.
    * \param h Входные данные
    * В случае если входные данные меньше или равны порогу minData, задается минимальная высота объекта и ему задается синий цвет. \n
    * В случае если входные данные находятся между minData и maxData, данные нормализуются относительно порогов и высот, и по получивнемуся значению задается высота объекта.
    * Цвет объекта устанавливается равным стандартному. \n
    * В случае если входные данные больше или равны порогу maxData, задается максимальная высота объекта и ему задается красный цвет. \n
    * По окончанию метода задается текущее показание визуализатора, равное входным данным.
    */
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
        curData = data;

    }

    /** \brief Метод плавного задания высоты объекта по данным в локальных координатах.
    * \param _h Входные данные
    * \param _time Время смены в секундах
    * Запускает сопрограмму плавной смены высоты SmoothChange.
    */
    public void SetHeight(float _h, float _time)
    {
        if (_h <= 0)
            SetHeight(_h);
        else
        {
            StartCoroutine(SmoothChange(_h, _time));
        }
    }

    /** \brief Сопрограмма, для плавного изменения высоты объекта.  
     * \param _h Входные данные
     * \param _time Время смены в секундах
     * 
     * Изменяет высоту диаграммы с начального до _h за _time секунд
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
      * \param _time Время обновления
      * 
      * Отвечает за регулярное обновление высоты объекта в соотвествии с данными источника. \n
      * В бесконечном цикле внутри сопрограммы из источника данных source берется значение данных, после чего вызываетася метод SetHeight.
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
     * 
    * В первую очередь в ней устанавливается ссылка на материал рендерера родительского объекта. 
    * Если это делать более традиционным путем - префабной связью - скриптом изменяется глобальный материал всех визуализаторов данного типа. \n
    * После этого стандартный цвет приравнивается цвету, которым обладает объект на начало исполнения программы. \n
    * Так же задается целевой объект изменения высоты, который в данной версии является объектом, к которому прикреплен скрипт, 
    * а так же максимальная высота объекта, равная высоте объекта до начала выполнения программы
    * После всех описанных действия начинается исполнение сопрограммы DelayedUpdate с частотой обновлений в updateTime. 
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
