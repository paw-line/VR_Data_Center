﻿using UnityEngine;
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

public class MqttData
{
    public string result { get; set; }
    public string hostName { get; set; }
    public string hostId { get; set; }
}

public class RecieverJR : MonoBehaviour
{

    private MqttClient client1;

    public string brocker_ip = "134.209.224.248";   ///< IP-адрес MQTT брокера. Может задаваться через редактор.
    private string topic1 = "/dc/serv1/+";          ///< ???
    private string topic2 = "/dc/serv2/+";          ///< ???
    private string topic = "/analyzer_data";              ///< Корневой топик MQTT
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

        client1 = new MqttClient(IPAddress.Parse(brocker_ip), 1883, false, null);
        client1.MqttMsgPublishReceived += client_MqttMsgPublishReceivedData;
        clientId = "qwerty";
        username = "collector";
        password = "qwerty123456";
        client1.Connect(clientId, username, password);
        client1.Subscribe(topic, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

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

        JArray mqttDataArray = JArray.Parse(mes);

        IList<MqttData> data = mqttDataArray.Select(p => new MqttData
        {
            result = (string)p["result"],
            hostName = (string)p["result"]["hostname"],
            hostId = (string)p["result"]["hostid"]
        }).ToList();

        Debug.Log(data[0].hostName)
    }
}
