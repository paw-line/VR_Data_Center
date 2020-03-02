using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization; //???
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;

using System;

public class Reciever : MonoBehaviour
{

    private MqttClient client1;

    public string brocker_ip = "134.209.224.248";
    private string topic1 = "/dc/serv1/+";
    private string topic2 = "/dc/serv2/+";
    private string topic = "/dc/serv";
    public string clientId = "qwerty";

    private string name;
    private float data;


    //[SerializeField]
    private Distributor distributor = null;
    //[SerializeField]
    private List<DataSource> sources;
    private List<string> sourcesnames = new List<string>();


    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(DelayedInit());
    }



    IEnumerator DelayedInit() //Без этой задержки дистрибутор не успевает найти сурсы
    {
        yield return new WaitForSeconds(3f);
        distributor = GameObject.Find("Distributor228").GetComponent<Distributor>();
        sources = distributor.sources;

        int c = 0;
        foreach (DataSource i in sources)
        {
            sourcesnames.Add(i.gameObject.name);
            c++;
        }




        client1 = new MqttClient(IPAddress.Parse(brocker_ip), 1883, false, null);
        client1.MqttMsgPublishReceived += client_MqttMsgPublishReceivedData;
        clientId = "qwerty";
        client1.Connect(clientId);
        for (int i  = 1; i <= 10; i++)
        {
            client1.Subscribe(new string[] { topic+i.ToString()+"/+"}, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }
        
    }

    void client_MqttMsgPublishReceivedData(object sender, MqttMsgPublishEventArgs e)
    {
        //Debug.Log("Received Data: [" + System.Text.Encoding.UTF8.GetString(e.Message) + "]");
        string mes = System.Text.Encoding.UTF8.GetString(e.Message);
        string topc = e.Topic;
        
        //data = mes;
        string[] temp = topc.Split('/');
        name = temp[3];
        switch (name)
        {
            case "servt":
                data = float.Parse(mes, CultureInfo.InvariantCulture.NumberFormat);
                break;
            case "hum":
                data = float.Parse(mes, CultureInfo.InvariantCulture.NumberFormat);
                data = data / 10 * 3 + 10;
                break;
            case "overallAlarm":
                data = float.Parse(mes, CultureInfo.InvariantCulture.NumberFormat);
                break;
        }

        int c = 0;
        int gotcha = 0;
        foreach (string i in sourcesnames)
        {
            
            if (i == topc)
            {
                Debug.Log(topc + " ?=" + i);
                //gotcha = c;
                sources[c].Set(data, name);
                break;
            }
            c++;
        }
        
    }
}
