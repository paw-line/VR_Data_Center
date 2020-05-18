using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * \brief Класс для контроля цветовых визуализаторов
 * \authors Пивко Артём
 * \version 1.2
 * \date 13.05.20
 * \warning Если source не присвоить в сцене, объект не будет функционировать!
 * \todo ???
 *  
 * Этот визуализатор занимается динамическим изменением цвета родительского объекта 
 * в соотвествии с данными из источника данных DataSource source.
 * Для конверсии используется синглтон UniversalController
 * 
 */

public class ColorChanger : Visualiser//MonoBehaviour
{
    //Настройки цветов. Текущие - для теплого периода.
    static private float absCold = 11f;         ///< Температура, ниже которой не отслеживается
    static private float allowedStart = 21f;    ///< Граница нижней недопустимой температуры
    static private float optimalStart = 23f;    ///< Нижняя граница оптимальной температуры
    //---------------------MIDDLE LINE = 24--
    static private float optimalFinish = 25f;   ///< Верхняя граница оптимальной температуры
    static private float allowedFinish = 28f;   ///< Граница верхней недопустимой температуры
    static private float absHeat = 38f;         ///< Температура, выше которой не отслеживается

    static private float maxTransp = 0.4f;      ///< Максимальное значение прозрачности объекта

    private float updateTime = 5f;              ///< Время обновления объекта

    public float temp = 23f;                    ///< Текущее значение температуры объекта
    public DataSource source;                   ///< Источник данных, контролирующий цвет. Присваевается в сцене до начала работы скрипта.

    private Material material = null; ///< Ссылка на устанавливается не префабной связью а в Awake ввиду того что иначе изменяется глобальный материал

    /** \brief Метод снятия показаний с визуализатора сканером
     * \param [string] visType Тип визуализатора
     * \param [string] dataType Тип данных
     * \param [string] topic MQTT-топик данных
     * \return Визуализируемые данные в формате float
     * Внимание, передаваемые в метод параметры при его вызове должны стоять после ключегого слова out. 
     */
    public override float Scan(out string visType, out string dataType, out string topic)
    {
        visType = this.GetType().ToString();
        dataType = source.GetType();
        topic = source.name;
        return temp;
    }

    /** \brief Метод, устанавливающий температуру и, соотвественно, цвет на объекте
     * \param [in] _temp Устанавливаемая температура
     */
    public void SetTemp(float _temp)
    {
        temp = _temp;
        Color newColor;
        Color oldColor = material.color;
        //newColor = TempToColor(temp);
        newColor = UniversalConverter.GetInstance().TempToColor(temp);
        oldColor.r = newColor.r;
        oldColor.g = newColor.g;
        oldColor.b = newColor.b;

        oldColor.a = newColor.a;

        material.SetColor("_Color", oldColor);
    }

    ///Метод, быстро устанавливающий недопустимо холодную температуру. Для тестов. 
    public void SetTempCold()
    {
        SetTemp(absCold + (allowedStart - absCold)/2, 1f);
    }

    ///Метод, медленно устанавливающий недопустимо холодную температуру. Для тестов. 
    public void SetTempWarm()
    {
        SetTemp(allowedFinish + (absHeat - allowedFinish) / 2, 10f);
    }

    ///Метод, со средней скоростью устанавливающий оптимальную температуру. Для тестов. 
    public void SetTempOK()
    {
        SetTemp((optimalStart+optimalFinish)/2, 5f);
    }

    /** \brief Метод, устанавливающий температуру и, соотвественно, цвет на объекте за указанное время. Интерфейс для корутины SmoothChange.
     * \param [in] _temp Устанавливаемая температура
     * \param [in] _time Время изменения
     */
    public void SetTemp(float _temp, float _time)
    {
        if (_time <= 0)
            SetTemp(_temp);
        else
        {
            StartCoroutine(SmoothChange(_temp, _time));
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
        while (true) {
            SetTemp(source.GetData(), _time / 2);
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
        //SetTemp(temp); //Временный костыль для демонстрации
        StartCoroutine(DelayedUpdate(updateTime));
    }

}