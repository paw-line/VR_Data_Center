using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeVisualisator : MonoBehaviour
{
    //[SerializeField]
    private Distributor distributor = null;

    //[SerializeField]
    private List<DataSource> sources;

    [SerializeField]
    private float refreshTime = 10f;

    [SerializeField]
    private float curData = 0;

    public string dataType = null;

    private Material material = null; //Устанавливается не префабной связью а в Awake ввиду того что иначе изменяется глобальный материал

    void Awake()
    {
        material = this.GetComponent<Renderer>().material;
        distributor = GameObject.Find("Distributor228").GetComponent<Distributor>();
        sources = distributor.sources;

        Debug.Log(sources[0].GetData());
        StartCoroutine(DelayedRefresh());
    }

    private void Init()
    {
        if (sources.Count == 0)
        {
            Debug.Log("Node " + this.gameObject.name.ToString() +  ": No sources detected");
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

}
