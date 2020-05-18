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
    private float colorData;                ///< Текущие данные, определяющие цвет потока
    private float dencData;                 ///< Текущая плотность потока
    public float speed = 5f;                ///< Скорость потока по-умолчанию. Задается в редакторе.
    public Color color;                     ///< Цвет потока по-умолчанию. Задается в редакторе.
    public float particleDencityMultiplier = 2f;    ///< Множитель плотности потока по-умолчанию. Задается в редакторе.
    public float radius = 2f;               ///< Радиус потока
    public bool trails = false;             ///< Оставляют ли частицы хвосты?


    public bool isControlledBySources = true;   ///< Контролируется ли поток источниками данных?
    public float refreshTime = 1f;          ///< Время обновления данных с источников
    [SerializeField]
    private DataSource speedSource = null;  ///< Ссылка на источник данных о скорости потока
    [SerializeField]
    private DataSource colorSource = null;  ///< Ссылка на источник данных о цвете потока
    [SerializeField]
    private DataSource dencitySource = null;///< Ссылка на источник данных о плотности потока


    /** \brief Метод снятия показаний с визуализатора сканером
     * \param [string] visType Возвращается конструкция вида Тип_источника_скорости:Значение
     * \param [string] dataType Тип данных источника цвета
     * \param [string] topic Возвращается конструкция вида Тип_источника_плотности:Значение
     * \return Визуализируемые данные цвета в формате float
     * Так как потоковый визуализатор это единстенный визуализатор, использующий несколько разных источников с разными независимыми данными,
     * его сканирование отличается от сканирования остальных визуализаторов. Из-за ограниченности сканера его текстовые строки приходится использовать
     * для отображения нескольких данных. \n
     * Внимание, передаваемые в метод параметры при его вызове должны стоять после ключегого слова out. 
     */
    public override float Scan(out string visType, out string dataType, out string topic)
    {
        //Очень костыльный метод, так как у нас сразу 3 сурса.
        if (speedSource != null)
        {
            visType = speedSource.GetType() + ":" + speedData;
        }
        else
        {
            visType = "";
        }

        if (dencitySource != null)
        {
            topic = dencitySource.GetType() + ":" + dencData; ;
        }
        else
        {
            topic = "";
        }

        if (colorSource != null)
        {
            dataType = colorSource.GetType() + ":";
            return colorData;
        }
        else
        {
            dataType = "";
            return 0;
        }
        
    }

    /** \brief Метод преобразования данных в скорость потока
     * \param [float] data Входные данные
     * \return Выходные данные
     */
    private float DataToSpeed(float data)
    {
        return data;
    }

    /** \brief Метод преобразования данных в цвет потока
     * \param [float] data Входные данные
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
     * \param [float] data Входные данные
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
                speed = DataToSpeed(speedData);
            }
            if (colorSource != null)
            {
                colorData = colorSource.GetData();
                color = DataToColor(colorData);
            }
            if (dencitySource != null)
            {
                dencData = dencitySource.GetData();
                particleDencityMultiplier = DataToDencity(dencData);
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
        mn.startLifetime = L / speed / ps.transform.localScale.x ;
        mn.startSpeed = speed;

        //Плотность
        var em = ps.emission;
        em.rateOverTime = ps.main.startSpeed.constant * particleDencityMultiplier;

        //Цвет
        ParticleSystem.MinMaxGradient grad = mn.startColor;
        grad.color = color;
        mn.startColor = grad;

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
     * \param [in] _time Время обновления
     * Отвечает за регулярное обновление данных потока с источников
     */
    IEnumerator DelayedUpdate(float _time)
    {
        yield return new WaitForSeconds(_time);
        while (true)
        {
            RefreshFromSources();
            yield return new WaitForSeconds(_time);
        }

    }

    /** \brief Метод инициализации объекта
     * Находит в поддереве объекта систему частиц и коллайдер, затем запускает сопрограмму основного цикла.
     */
    void Awake()
    {
        ps = this.GetComponentInChildren<ParticleSystem>();
        collObj = this.GetComponentInChildren<Collider>().gameObject;
        if (collObj == null)
            Debug.LogError("В потоковом визуализаторе не обнаружен коллайдер.");
        StartCoroutine(DelayedUpdate(refreshTime));
    }
    
    /*
    void Start()
    {
        ps = this.GetComponentInChildren<ParticleSystem>();
        StartCoroutine(DelayedUpdate(refreshTime));
    }
    */

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