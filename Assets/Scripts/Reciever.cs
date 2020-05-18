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

/**
 * \brief Класс для получения данных с сервера через MQTT
 * \authors Пивко Артём, Стрельцов Григорий
 * \version 1.0
 * \date 14.05.20
 * \warning  ???
 *  
 * Этот визуализатор занимается динамическим изменением цвета родительского объекта 
 * в соотвествии с данными из источника данных DataSource source.
 * Для конверсии используется синглтон UniversalController
 * 
 */

public class Reciever : MonoBehaviour
{

    private MqttClient client1;

    public string brocker_ip = "134.209.224.248";   ///< IP-адрес MQTT брокера. Может задаваться через редактор.
    private string topic1 = "/dc/serv1/+";          ///< ???
    private string topic2 = "/dc/serv2/+";          ///< ???
    private string topic = "/dc/serv";              ///< Корневой топик MQTT
    public string clientId = "qwerty";              ///< ID MQTT клиента.  Может задаваться через редактор.

    private string name;                            ///< Переменная для временного хранения типа данных???
    private float data;                             ///< Переменная для временного хранения данных

    private List<DataSource> sources;               ///< Список источников на сцене
    private List<string> sourcesnames = new List<string>(); ///< Список топиков источников на сцене


    /** \brief Метод инициализаии объекта
     * Вызывает сопрограмму отложенной инициализации. 
     */
    void Awake()
    {
        StartCoroutine(DelayedInit());
    }



    /** \brief Сопрограмма отложенной инициализаии объекта
     * Получает от глобального дистрибутора список источников и по нему формирует список MQTT-топиков этих источников. MQTT-топик источника это его имя. 
     * После этого подключается к MQTT брокеру по заданному в объекте IP-адресу с заданным в объекте идентификатором. Привязывает событие получения данных к обработчику. Затем подписывается на топики. \n
     * Задержка в инициализации требуется для дистрибутора чтобы тот успел собрать все источники на сцене. 
     */
    IEnumerator DelayedInit()
    {
        yield return new WaitForSeconds(3f); //Без этой задержки дистрибутор не успевает найти сурсы
        //distributor = GameObject.Find("Distributor228").GetComponent<Distributor>();
        //distributor = Distributor.GetInstance();
        //sources = distributor.sources;
        sources = Distributor.GetInstance().GetSources();

        int c = 0;
        foreach (DataSource i in sources)
        {
            sourcesnames.Add(i.gameObject.name);
            c++;
        }

        Connect();
    }

    private void Connect()
    {
        client1 = new MqttClient(IPAddress.Parse(brocker_ip), 1883, false, null);
        client1.MqttMsgPublishReceived += client_MqttMsgPublishReceivedData;
        clientId = "qwerty";
        client1.Connect(clientId);
        for (int i = 1; i <= 10; i++)
        {
            client1.Subscribe(new string[] { topic + i.ToString() + "/+" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }
    }

    /** \brief Метод-обработчик события получения
    *  \param [object] sender ???
    *  \param [MqttMsgPublishEventArgs] e ???
    *  ???
    */
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


        SourceSet(topc, data, name);
    }

    private void SourceSet(string topic, float data, string name)
    {
        int c = 0;
        foreach (string i in sourcesnames)
        {

            if (i == topic)
            {
                //Debug.Log(topc + " ?=" + i);
                //gotcha = c;
                sources[c].Set(data, name);
                break;
            }
            c++;
        }
    }
}
