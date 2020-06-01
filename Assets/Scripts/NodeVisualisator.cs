using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * \brief Класс экземпляров объемных визуализаторов
 * \authors Пивко Артём
 * \version 1.1
 * \date 14.05.20
 * \warning В случае если на сцене будет отсутствовать дистрибутор данных, объект не будет функционировать. 
 *  
 * Этот класс занимается динамическим изменением цвета родительского объекта 
 * в соотвествии с данными из источника данных DataSource source и 
 * настроек конвертации температуры из данного файла.
 * 
 */
public class NodeVisualisator : Visualiser
{
    //[SerializeField]
    //private Distributor distributor = null;         ///< Ссылка на объект-дистрибутор источников данных

    //[SerializeField]
    //private List<DataSource> sources;               // Актуальный список всех источников

    //[SerializeField]
    private List<DataSource> validSources;          ///< Актуальный список источников, влияющих на данный визуализатор. 

    [SerializeField]
    private float refreshTime = 10f;                ///< Время обновления визуализатора

    [SerializeField]
    private float curData = 0;                      ///< Текущее значение визуализируемой информации на этом визуализаторе

    public string dataType = null;                  ///< Тип данных, визуализируемых данным объектов (какой параметр запрашивать у источника)

    /** \brief Ссылка на текущий материал данного визуализатора. 
     * Устанавливается не префабной связью а в Awake ввиду того что иначе изменяется глобальный материал.
    */
    private Material material = null;

    /** \brief Метод снятия показаний с визуализатора сканером
     * \param visType Тип визуализатора
     * \param dataType Тип данных
     * \param topic MQTT-топик данных
     * \return Визуализируемые данные в формате float
     * В случае если визуализатор находится вне зоны дейстия источников, в качестве типа данных будет передано сообщение об этом, а топик передан не будет.
     * В случае если визуализатор слушает только один источник, будет передан его топик. В случае если несколько, будет передано сообщение о смешанных топиках.\n
     * Внимание, передаваемые в метод параметры при его вызове должны стоять после ключегого слова out. 
     */
    public override string Scan(out string visType, out string dataType1, out string topic)
    {
        UniversalTranslator tr = UniversalTranslator.GetInstance();

        visType = tr.TranstypeToRussian(this.GetType().ToString());
        if (validSources.Count == 0)
        {
            dataType1 = "ERROR: No data source is connected to this visualiser";
            topic = "";
        }
        else
        {
            dataType1 = tr.TranstypeToRussian(dataType);
            if (validSources.Count == 1)
                topic = validSources[0].name;
            else
                topic = "Mixed topic";
        }


        
        return curData.ToString() + tr.TransGeneralTypeToUnit(dataType);
    }

    /** \brief Функция обновления списка актуальных источников
     * Обнуляет текущий список актуальных источников, получает из дистрибутора список из ВСЕХ источников.
     * Затем, в случае если этот список не пуст, сравнивает доверительный радиус источников с расстоянием до этого источника.
     * Если источник покрывает этот объект, то он считается для него актуальным и заносится в список validSources.
    */
    private void UpdateValidSources()
    {
        //List<DataSource> sources = distributor.sources;
        List<DataSource> sources = Distributor.GetInstance().GetSources();
        validSources = new List<DataSource>();


        if (sources.Count == 0)
        {
            Debug.Log("Node " + this.gameObject.name.ToString() + ": No sources detected at all");
            material.color = Color.gray;
            return;
        }
        else 
        {
            Vector3 myCoord = this.transform.position;
            foreach (DataSource source in sources)
            {
                //Рассмотреть ещё случай с радиусом в -1 значащим бесконечный.
                //Debug.Log("Source.Radius= " + source.GetRadius().ToString());
                //Debug.Log("Distance= " + Vector3.Distance(source.transform.position, myCoord));
                //Debug.Log("Node " + this.gameObject.name.ToString() + " : pos= " + source.transform.position.magnitude.ToString());
                string generalType = UniversalTranslator.GetInstance().TransTypeToGeneralType(source.GetType());
                //Debug.Log(source.GetType() + "=>" + generalType + "?=" + dataType);
                if ((source.GetRadius() >= Vector3.Distance(source.transform.position, myCoord)) && (generalType == dataType))
                {//valid source
                    validSources.Add(source);
                }
            }
        }

    }

    /** \brief Функция обновления отображаемого значения объекта
     * Функция использует данные из валидных источников для обновления значения отображаемых данных. \n
     * • Если подходящих источников источников нет, то объект окрашивается в серый цвет и Log выводится сообщение "Node [X,Y,Z]: No valid sources detected",
     * где X, Y, Z - номера объекта в матрице, созданной скриптом CubeCreator.cs. \n
     * • Если подходдящий источник один, то визуализатор отображает данный с него. \n
     * • Если подходящих источников несколько, то значение, отображаемое объектом вычисляется на основе удаленности от источников следующим образом: \n
     *      L - расстояние между объектом и источником \n
     *      R - Доверительный радиус источника \n
     *      X - Данные на источнике \n
     *      b = 1 - (L / R) - Ненормализованный вес источника для данного объекта \n
     *      a = b * Х - взвешенные ненормализованные данные с источника \n
     *      sumk - сумма b всех источников \n
     *      sum - сумма всех а \n
     *      k = 1 / sumk - коэфициент нормализации \n
     *      y = sum * k - Итоговое значение данных на источнике \n
     *      \n
     * 
     *      Пример: есть 2 источника с радиусами R1 = R2 = 7. Источник 1 показывает X1 = 10, источник 2 показывает X2 = 20. 
     *      Куб находится на расстоянии a1=4 от источника 1 и на расстоянии a2=6 от источника 2.  \n
     *      1)	Считаем долю от радиуса каждого куба. \n
     *      A1 = a1/R1 = 4/7 = 0.57 \n
     *      A2 = a2/R2 = 6/7 = 0.86 \n
     *      2)	Вычитаем их из единицы \n
     *      B1 = 1-A1 = 1-0.57 = 0.43 \n
     *      B2 = 1-A2 = 1-0.86 = 0.14 \n
     *      3)	Ищем коэффициент нормализации: \n
     *      K = 1/(B1+B2) = 1/(0.14+0.43) = 1/0.57 = 1.75 \n
     *      4) 	Считаем финальное значение: \n
     *      Y = (X1*B1 + X2*B2)*K = (10*0.43 + 20*0.14)*1.75 = 12.4. \n

    */
    private void UpdateObject()
    {
        if (validSources.Count == 0)
        {//Не было найдено подходящих источников
            Debug.Log("Node " + this.gameObject.name.ToString() + ": No valid sources detected");
            material.color = Color.gray;
            return;
        }
        else if (validSources.Count == 1)
        {//Найден один подходящий источник. Берем значение тупо из него. 
         //Debug.Log("Node " + this.gameObject.name.ToString() + ": Found one valid source");
         //material.color = ColorChanger.TempToColor(validSources[0].GetData()); //Старый код
            curData = validSources[0].GetData();
            material.color = UniversalConverter.GetInstance().TempToColor(curData, dataType);
        }
        else
        {
            // Debug.Log("Node " + this.gameObject.name.ToString() + ": Found multiple valid sources");
            float b = 0;
            float sumk = 0;
            float sum = 0;
            Vector3 myCoord = this.transform.position;

            foreach (DataSource source in validSources)
            {
                b = 1 - (Vector3.Distance(source.transform.position, myCoord) / source.GetRadius());
                sumk += b;  //(1 - Vector3.Distance(sources[i].transform.position, myCoord));
                sum += b * source.GetData();
                //Debug.Log("Data" + i.ToString() + ":" + sources[i].GetData().ToString());
            }
            float k = 1 / sumk;
            sum *= k;

            //Debug.Log("Overall sum:" + sum.ToString());
            //material.color = ColorChanger.TempToColor(sum); //Старый код

            material.color = UniversalConverter.GetInstance().TempToColor(sum, dataType);
            curData = sum;
        }
    }

    /** \brief Установочная функция, вызываемая в момент активации объекта на сцене.
    * Вызывает сопрограмму DelayedInit ввиду необходимости инициализации с задержкой. Подробнее в описании DelayedInit. 
   */
    void Awake()
    {
        StartCoroutine(DelayedInit());
    }

    /** \brief Установочная сопрограмма.
     * Выполняет получение ссылки на материал данного объекта, получает ссылку на экземпляр глобального дистрибутора данных
     * и запускает бесконечную сопрограмму DelayedRefresh. Перед выполнением всех вышеописанных действий ожидает 3 секунды 
     * для того чтобы дистрибутор успел собрать сведения об источниках на сцене. 
    */
    IEnumerator DelayedInit()
    {
        yield return new WaitForSeconds(3f); //Без этой задержки дистрибутор не успевает найти сурсы
        material = this.GetComponent<Renderer>().material;
        //distributor = GameObject.Find("Distributor228").GetComponent<Distributor>();
        /*
        distributor = Distributor.GetInstance();
        if (distributor == null)
            Debug.LogError("Node " + gameObject.name.ToString() + ": No distributor found in the scene");
        //sources = distributor.sources;
        */
        UpdateValidSources();

        //Debug.Log(sources[0].GetData());
        StartCoroutine(DelayedRefresh());

    }

    /** \brief Сопрограмма-таймер основного цикла.
     * Отвечает за регулярное обновление отображаемых данных в соотвествии с данными источника. \n
     * В бесконечном цикле внутри сопрограммы вызывается функция Init, 
     * после чего производится ожидание на время обновления до повторения цикла. 
    */
    IEnumerator DelayedRefresh()
    {
        //Debug.Log("In coroutine");
        while (true)
        {
            //Debug.Log("Goin into init");
            //Init();
            UpdateObject();

            //Debug.Log("Init Complete. Waiting");
            yield return new WaitForSeconds(refreshTime);
        }

    }

    /*//Старый код до разделения инита на случай если что-то сломается. 
private void Init()
{
    if (sources.Count == 0)
    {
        Debug.Log("Node " + this.gameObject.name.ToString() +  ": No sources detected at all");
        material.color = Color.gray;
        return;
    }
    {
        List<int> validSources = new List<int>();
        Vector3 myCoord = this.transform.position;
        int v = 0;
        foreach (DataSource source in sources)
        {
            //Рассмотреть ещё случай с радиусом в -1 значащим бесконечный.
            //Debug.Log("Source.Radius= " + source.GetRadius().ToString());
            //Debug.Log("Distance= " + Vector3.Distance(source.transform.position, myCoord));
            //Debug.Log("Node " + this.gameObject.name.ToString() + " : pos= " + source.transform.position.magnitude.ToString());
            if ((source.GetRadius() >= Vector3.Distance(source.transform.position, myCoord))&&(source.GetType() == dataType))
            {//valid source
                validSources.Add(v);
            }
            v++;
        }

        if (validSources.Count == 0)
        {//Не было найдено подходящих источников
            Debug.Log("Node " + this.gameObject.name.ToString() + ": No valid sources detected");
            material.color = Color.gray;
            return;
        }
        else if (validSources.Count == 1)
        {//Найден один подходящий источник. Берем значение тупо из него. 
            //Debug.Log("Node " + this.gameObject.name.ToString() + ": Found one valid source");
            material.color = ColorChanger.TempToColor(sources[validSources[0]].GetData());
        }
        else
        {
           // Debug.Log("Node " + this.gameObject.name.ToString() + ": Found multiple valid sources");
            float b = 0;
            float sumk = 0;
            float sum = 0;
            foreach (int i in validSources)
            {
                b = 1 - (Vector3.Distance(sources[i].transform.position, myCoord) / sources[i].GetRadius());
                sumk += b;  //(1 - Vector3.Distance(sources[i].transform.position, myCoord));
                sum += b * sources[i].GetData();
                //Debug.Log("Data" + i.ToString() + ":" + sources[i].GetData().ToString());
            }
            float k = 1 / sumk;
            sum *= k;

            //Debug.Log("Overall sum:" + sum.ToString());
            material.color = ColorChanger.TempToColor(sum);
            curData = sum;

        }
    }

}
*/

    /*
    IEnumerator DelayedInit2()
    {
        yield return new WaitForSeconds(3f); //Без этой задержки дистрибутор не успевает найти сурсы
        material = this.GetComponent<Renderer>().material;
        distributor = GameObject.Find("Distributor228").GetComponent<Distributor>();
        if (distributor == null)
            Debug.LogError("Node " + gameObject.name.ToString() + ": No distributor found in the scene");
        sources = distributor.sources;

        //Debug.Log(sources[0].GetData());
        StartCoroutine(DelayedRefresh());

    }

    IEnumerator DelayedRefresh2()
    {
        //Debug.Log("In coroutine");
        while (true)
        {
            //Debug.Log("Goin into init");
            Init();
            //Debug.Log("Init Complete. Waiting");
            yield return new WaitForSeconds(refreshTime);
        }

    }
    */



}
