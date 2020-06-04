using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * \brief Класс для контроля потоковых визуализаторов
 * \authors Пивко Артём
 * \version 1.0
 * \date 14.05.20
 * \warning Функционирует постоянно. Не только во время работы программы, но и в редакторе. 
 *  
 * Этот класс  визуализатор использует систему частиц для визуализации "потоковых" данных, таких как параметры электропроводки, водяного или воздушного охлаждения. 
 * Система частиц должна иметь включенными модули Emission, Shape, Trails и Renderer.
 * Рекомеднуется использовать только как часть префаба SteamVisualiser
 */

/*
 В случае водяного охлаждения:
1)	Скорость частиц - VlvPos% 
2)	Плотность частиц - FluidFlowL/s 
3)	Начальный цвет частиц - FluidInC 
4)	Конечный цвет частиц - FluidOutC 
В случае вентиляции:
1)	Скорость частиц - FanSpd% 
2)	Плотность частиц - AirFlwL/s 
3)	Начальный цвет частиц - SupplyC 
4)	Конечный цвет частиц - ReturnC 

 */

[ExecuteAlways]
public class StreamVisualiser : Visualiser
{
    [SerializeField]
    private ParticleSystem ps;              ///<Ссылка на контролируемую систему частиц.

    //public Vector3 streamSize;
    [SerializeField]
    private Transform start = null;         ///<Ссылка на точку начала потока. Задается в редакторе.
    [SerializeField]
    private Transform finish = null;        ///<Ссылка на точку окончания потока. Задается в редакторе.

    private GameObject collObj = null;      ///<Ссылка на объект, служащий коллайдером визуализатора для сканера. Задается в редакторе.

    private float speedData;                ///< Текущая скорость потока
    private float startColorData;           ///< Текущие данные, определяющие цвет начала потока
    private float finishColorData;          ///< Текущие данные, определяющие цвет конца потока
    private float dencData;                 ///< Текущая плотность потока

    public float speed = 5f;                ///< Скорость потока по-умолчанию. Задается в редакторе.
    public Color startColor;                ///< Цвет начала потока по-умолчанию. Задается в редакторе.
    public Color finishColor;               ///< Цвет конца потока по-умолчанию. Задается в редакторе.
    public float dencity = 2f;              ///< Множитель плотности потока по-умолчанию. Задается в редакторе.
    public float radius = 2f;               ///< Радиус потока
    public bool trails = false;             ///< Оставляют ли частицы хвосты?


    public bool isControlledBySources = true;   ///< Контролируется ли поток источниками данных?
    public float refreshTime = 1f;          ///< Время обновления данных с источников
    [SerializeField]
    private DataSource speedSource = null;  ///< Ссылка на источник данных о скорости потока
    [SerializeField]
    private DataSource startColorSource = null;  ///< Ссылка на источник данных о цвете потока
    [SerializeField]
    private DataSource finishColorSource = null;  ///< Ссылка на источник данных о цвете потока
    [SerializeField]
    private DataSource dencitySource = null;///< Ссылка на источник данных о плотности потока

    public float speedMultiplier = 1f;                ///< Скорость потока по-умолчанию. Задается в редакторе.
    public float dencityMultiplier = 1f;              ///< Множитель плотности потока по-умолчанию. Задается в редакторе.


    /** \brief Метод снятия показаний с визуализатора сканером
     * \param visType Возвращается конструкция вида Тип_источника_скорости:Значение
     * \param dataType Тип данных источника цвета
     * \param topic Возвращается конструкция вида Тип_источника_плотности:Значение
     * \return Визуализируемые данные цвета в формате float
     * Так как потоковый визуализатор это единстенный визуализатор, использующий несколько разных источников с разными независимыми данными,
     * его сканирование отличается от сканирования остальных визуализаторов. Из-за ограниченности сканера его текстовые строки приходится использовать
     * для отображения нескольких данных. \n
     * Внимание, передаваемые в метод параметры при его вызове должны стоять после ключегого слова out. 
     */
    public override string Scan(out string visType, out string dataType, out string topic)
    {
        UniversalTranslator tr = UniversalTranslator.GetInstance();

        if (speedSource != null)
        {
            visType = speedSource.GetType() + "=" + speedData.ToString() + tr.TransGeneralTypeToUnit(speedSource.GetType());
        }
        else
        {
            visType = "";
        }

        if (dencitySource != null)
        {
            topic = dencitySource.GetType() + "=" + dencData.ToString() + tr.TransGeneralTypeToUnit(dencitySource.GetType());
        }
        else
        {
            topic = "";
        }

        if (startColorSource != null)
        {
            dataType = startColorSource.GetType() + "=" + startColorData.ToString() + tr.TransGeneralTypeToUnit(startColorSource.GetType());
        }
        else
        {
            dataType = "";
        }

        if (finishColorSource != null)
        {
            return (finishColorSource.GetType() + "=" + finishColorData.ToString() + tr.TransGeneralTypeToUnit(finishColorSource.GetType()));
        }
        else
        {
            return "";
        }



        /*
            //Очень костыльный метод, так как у нас сразу 3 сурса.
            if ((startColorSource != null)&&(finishColorSource != null))
        {
            //visType = speedSource.GetType() + ":" + speedData;
            //visType = startColorSource.GetType() + "(start col) =" + startColorSource.GetData().ToString() + "|" + finishColorSource.GetType() + "(fin col) =" + finishColorSource.GetData().ToString();
            visType = startColorSource.GetType() + "(start col) =" + startColorData.ToString() + "|" + finishColorSource.GetType() + "(fin col) =" + finishColorData.ToString();
        }
        else
        {
            if (startColorSource != null)
            {
                visType = startColorSource.GetType() + "(Color) =" + startColorData.ToString();
            }
            else
            {
                if (finishColorSource != null)
                {
                    visType = finishColorSource.GetType() + "(Color) =" + finishColorData.ToString();
                }
                else
                {
                    visType = "";
                }
            }
        }

        if (dencitySource != null)
        {
            topic = dencitySource.GetType() + "(Color) =" + dencData.ToString();
        }
        else
        {
            topic = "";
        }

        if (speedSource != null)
        {
            dataType = speedSource.GetType() + "(speed):";
            UniversalTranslator tr = UniversalTranslator.GetInstance();
            return speedData.ToString() + tr.TransGeneralTypeToUnit(tr.TransTypeToGeneralType(speedSource.GetType()));
        }
        else
        {
            dataType = "";
            return "-";
        }
        */

    }

    /** \brief Метод преобразования данных в скорость потока
     * \param data Входные данные
     * \return Выходные данные
     */
    private float DataToSpeed(float data)
    {
        return data;
    }

    /** \brief Метод преобразования данных в цвет потока
     * \param data Входные данные
     * \return Выходные данные
     * Используется преобразование с помощью UniversalConverter
     */
    private Color DataToColor(float data)
    {
        Color t = UniversalConverter.GetInstance().TempToColor(data);
        t.a = 1f;
        return t;
    }

    /** \brief Метод преобразования данных в плотность потока
     * \param data Входные данные
     * \return Выходные данные
     */
    private float DataToDencity(float data)
    {
        return data;
    }

    /** \brief Метод обновления данных на основе источников     */
    private void RefreshFromSources()
    {
        if (isControlledBySources)
        {
            if (speedSource != null)
            {
                speedData = speedSource.GetData();
                //speed = DataToSpeed(speedData);
                speed = speedData;// UniversalConverter.GetInstance().Convert(speedData, speedSource.GetType());
            }
            if (startColorSource != null)
            {
                startColorData = startColorSource.GetData();
                string gentype = UniversalTranslator.GetInstance().TransTypeToGeneralType(startColorSource.GetType());
                //Debug.Log(gentype);
                finishColor = UniversalConverter.GetInstance().TempToColor(startColorData, gentype);
                finishColor.a = 1f;
                //startColor = DataToColor(startColorData);       
            }
            if (finishColorSource != null)
            {
                finishColorData = finishColorSource.GetData();
                string gentype = UniversalTranslator.GetInstance().TransTypeToGeneralType(finishColorSource.GetType());
                finishColor = UniversalConverter.GetInstance().TempToColor(finishColorData, gentype);
                finishColor.a = 1f;
                //finishColor = DataToColor(finishColorData); 
            }
            if (dencitySource != null)
            {
                dencData = dencitySource.GetData();
                //particleDencityMultiplier = DataToDencity(dencData);
                dencity = dencData;// UniversalConverter.GetInstance().Convert(dencData, dencitySource.GetType());
            }
        }
    }

    /** \brief Функция основного цикла. 
     * 1) Размещает систему частиц на точке начала и направляет её в точку конца \n
     * 2) Рассчитывает время жизни частиц так, чтобы они долетали от начала до конца потока \n
     * 3) Устанавливает частоту испускания частиц так, чтобы соблюдалась необходимая плотность потока \n
     * 4) Устанавливает цвет выпущенных частиц так, чтобы соблюдался необходимый цвет потока \n
     * 5) Устанавливает радиус основания излучателя частиц так, чтобы соблюдался необходимый радиус потока \n
     * 6) Устанавливает цвет выпущенных частиц так, чтобы соблюдался необходимый цвет потока \n
     * 7) Устанавливает хвосты у выпущенных частиц, если это опция активирована в настройках потока \n
     * 8) Устанавливает положение коллайдера вдоль потока\n
     */
    private void Update()
    {
        //Координаты начала и конца
        ps.transform.position = start.position;
        ps.transform.LookAt(finish);      

        //Длинна
        float L = Vector3.Distance(start.position, finish.position);
        var mn = ps.main;
        mn.startLifetime = L / (speed * speedMultiplier) / ps.transform.localScale.x ;
        mn.startSpeed = speed*speedMultiplier;

        //Плотность
        var em = ps.emission;
        em.rateOverTime = ps.main.startSpeed.constant * dencity * dencityMultiplier;

        //Цвет 
        var color = ps.colorOverLifetime;
        ParticleSystem.MinMaxGradient grad = color.color.gradient;
        GradientColorKey[] colorKey;
        GradientAlphaKey[] alphaKey;
        colorKey = new GradientColorKey[2];
        colorKey[0].color = startColor;
        colorKey[0].time = 0.0f;
        colorKey[1].color = finishColor;
        colorKey[1].time = 1.0f;
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;
        grad.gradient.SetKeys(colorKey, alphaKey);

        color.color = grad;
        /*
        ParticleSystem.MinMaxGradient grad = mn.startColor;
        grad.color = color;
        mn.startColor = grad;
        */


        //Радиус
        var sh = ps.shape;
        sh.radius = radius;

        //Хвосты
        var tr = ps.trails;
        tr.enabled = trails;

        //Коллайдер
        //Vector3 centre = new Vector3()
        collObj.transform.position = (finish.position + start.position)/2;
        collObj.transform.LookAt(finish);
        Vector3 sc = collObj.transform.localScale;
        sc.z = L;
        sc.y = radius * ps.transform.localScale.x;
        sc.x = radius * ps.transform.localScale.z;
        collObj.transform.localScale = sc;

    }

    /** \brief Сопрограмма-таймер основного цикла.
     * \param _time Время обновления
     * Отвечает за регулярное обновление данных потока с источников
     */
    IEnumerator DelayedUpdate(float _time)
    {
        yield return new WaitForSeconds(_time);
        while (true)
        {
            RefreshFromSources();
            //Debug.Log("Refreshing");
            var mn = ps.main;
            mn.prewarm = true;
            ps.Play();
            yield return new WaitForSeconds(_time);
        }

    }

    /** \brief Метод инициализации объекта
     * Находит в поддереве объекта систему частиц и коллайдер, затем запускает сопрограмму основного цикла.
     */
    void Awake()
    {
        ps = this.GetComponentInChildren<ParticleSystem>();
        ps.Stop();
        var mn = ps.main;
        mn.prewarm = false;
        collObj = this.GetComponentInChildren<Collider>().gameObject;
        if (collObj == null)
            Debug.LogError("В потоковом визуализаторе не обнаружен коллайдер.");
        StartCoroutine(DelayedUpdate(refreshTime));
    }
    
     /** \brief Служебная функция, необходимая для отображения границ системы частиц и координат начала и конца в редакторе  */
    void OnDrawGizmos()
    {
        //Рисование линиями границы системы
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start.position + new Vector3(0,0,ps.shape.radius)* ps.transform.localScale.z, finish.position + new Vector3(0, 0, ps.shape.radius) * ps.transform.localScale.z);
        Gizmos.DrawLine(start.position - new Vector3(0, 0, ps.shape.radius) * ps.transform.localScale.z, finish.position - new Vector3(0, 0, ps.shape.radius) * ps.transform.localScale.z);
        Gizmos.DrawLine(start.position + new Vector3(ps.shape.radius, 0, 0) * ps.transform.localScale.x, finish.position + new Vector3(ps.shape.radius, 0, 0) * ps.transform.localScale.x);
        Gizmos.DrawLine(start.position - new Vector3(ps.shape.radius, 0, 0) * ps.transform.localScale.x, finish.position - new Vector3(ps.shape.radius, 0, 0) * ps.transform.localScale.x);
        Gizmos.DrawLine(start.position + new Vector3(0, ps.shape.radius, 0) * ps.transform.localScale.y, finish.position + new Vector3(0, ps.shape.radius, 0) * ps.transform.localScale.y);
        Gizmos.DrawLine(start.position - new Vector3(0, ps.shape.radius, 0) * ps.transform.localScale.y, finish.position - new Vector3(0, ps.shape.radius, 0) * ps.transform.localScale.y);



        if ((start != null) && (finish != null))
        {
            Gizmos.DrawIcon(start.position, "start", true);
            Gizmos.DrawIcon(finish.position, "finish", true);
        }
    }

    /*
    void OnDrawGizmosSelected()
    {
        
        
        if ((start != null) && (finish != null))
        {
            Gizmos.DrawIcon(start.position, "start", true);
            Gizmos.DrawIcon(finish.position, "finish", true);
        }
        
    }
    */
}