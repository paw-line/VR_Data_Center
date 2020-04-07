using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * \brief Класс экземпляров объемных визуализаторов
 * \authors Пивко Артём
 * \version 1.0
 * \date 7.04.20
 * \warning В случае если на сцене будет отсутствовать дистрибутор данных, объект не будет функционировать. 
 * \todo Разделить Init на получение списка валидных источников и обновление данных. 
 *  
 * Этот класс занимается динамическим изменением цвета родительского объекта 
 * в соотвествии с данными из источника данных DataSource source и 
 * настроек конвертации температуры из данного файла.
 * 
 */
public class NodeVisualisator : MonoBehaviour
{
    //[SerializeField]
    private Distributor distributor = null;         ///< Ссылка на объект-дистрибутор источников данных

    //[SerializeField]
    private List<DataSource> sources;               ///< Актуальный список источников, влияющих на данный визуализатор

    [SerializeField]
    private float refreshTime = 10f;                ///< Время обновления визуализатора

    [SerializeField]
    private float curData = 0;                      ///< Текущее значение визуализируемой информации на этом визуализаторе

    public string dataType = null;                  ///< Тип данных, визуализируемых данным объектов (какой параметр запрашивать у источника)

    /** \brief Ссылка на текущий материал данного визуализатора. 
     * Устанавливается не префабной связью а в Awake ввиду того что иначе изменяется глобальный материал.
    */
    private Material material = null;

    /** \brief Установочная функция, вызываемая в момент активации объекта на сцене.
     * Вызывает сопрограмму DelayedInit ввиду необходимости инициализации с задержкой. Подробнее в описании DelayedInit. 
    */
    void Awake()
    {
        StartCoroutine(DelayedInit());
    }


    /** \brief Функция, обновляющая значение визуализируемой информации по данным из источников. 
    * 
    */
    private void Init()
    {
        if (sources.Count == 0)
        {
            Debug.Log("Node " + this.gameObject.name.ToString() +  ": No sources detected at all");
            material.color = Color.gray;
            return;
        }else /*if (sources.Count == 1)
        {

        }else*/
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
            Init();
            //Debug.Log("Init Complete. Waiting");
            yield return new WaitForSeconds(refreshTime);
        }

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
        distributor = GameObject.Find("Distributor228").GetComponent<Distributor>();
        if (distributor == null)
            Debug.LogError("Node " + gameObject.name.ToString() + ": No distributor found in the scene");
        sources = distributor.sources;

        //Debug.Log(sources[0].GetData());
        StartCoroutine(DelayedRefresh());

    }

}
