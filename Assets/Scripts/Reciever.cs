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
 * \brief Класс Data для хранения полученных JSON данных
 * \authors Пивко Артём, Стрельцов Григорий
 *  
 * Данный класс предназначен для хранения после получения пакета данных
 * в формате JSON. Она является верхним в иерархии пакета JSON
 * и содержит параметр типа класса Result, класса ниже по иерархии.
 * 
 */
[Serializable]
public class Data
{
    public Result[] result;         ///< Парметр, предназанченный для хранения тела сообщения
    public string time;             ///< Парметр, предназанченный для хранения метки времени
}

/**
 * \brief Класс Result для хранения полученных JSON данных
 * \authors Пивко Артём, Стрельцов Григорий
 */
[Serializable]
public class Result
{
    public string hostname;         ///< Парметр, предназанченный для хранения имени устройства
    public string hostid;           ///< Парметр, предназанченный для хранения ID устройства
    public Items[] items;           ///< Парметр, предназанченный для хранения датчиков устройства
}

/**
 * \brief Класс Items для хранения полученных JSON данных
 * \authors Пивко Артём, Стрельцов Григорий
 */
[Serializable]
public class Items
{
    public string itemid;           ///< Парметр, предназанченный для хранения ID датчика
    public string itemname;         ///< Парметр, предназанченный для хранения имени датчика
    public string value;            ///< Парметр, предназанченный для хранения последнего значения, полученного с датчика
    public string value_type;       ///< Парметр, предназанченный для хранения типа присылаемого значения
    public Trigger trigger;         ///< Парметр, предназанченный для хранения триггера датчика
}

/**
 * \brief Класс Trigger для хранения полученных JSON данных
 * \authors Пивко Артём, Стрельцов Григорий
 */
[Serializable]
public class Trigger
{
    public string host;             ///< Парметр, предназанченный для хранения устройства, к которому привязан триггер
    public string item;             ///< Парметр, предназанченный для хранения датчика, к которому привязан триггер
    public int value;               ///< Парметр, предназанченный для хранения значения триггера
    public string upper;            ///< Парметр, предназанченный для хранения верхнего порога разрешенных значений
    public string lower;            ///< Парметр, предназанченный для хранения нижнего порога разрешенных значений
    public string equal;            ///< Парметр, предназанченный для хранения неразрешенного значения
    public string unknown;          ///< Парметр, предназанченный для хранения неизвестных значений
}

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
    private Distributor distributor;                ///< Ссылка на глобальный дистрибутор данных
    private MqttClient client1;                     ///< Объект клиента MQTT

    public string brocker_ip = "134.209.224.248";   ///< IP-адрес MQTT брокера. Может задаваться через редактор.
    private string topic = "/analyzer_data";        ///< Топик данных, поступающих в реальном времени
    public string clientId = "qwerty";              ///< ID MQTT клиента.  Может задаваться через редактор.
    public string username = "collector";           ///< Логин MQTT клиента.  Может задаваться через редактор.
    public string password = "qwerty123456";        ///< Пароль MQTT клиента.  Может задаваться через редактор.
    public string keyword = "Python";               ///< Ключевое слово, которое идентифицирует датчики, которые необходимо визуализировать. Может задаваться через редактор.

    //private List<DataSource> sources;               ///< Список источников на сцене
    //private List<string> sourcesnames = new List<string>(); ///< Список топиков источников на сцене


    /** \brief Метод инициализаии объекта
     * Вызывает сопрограмму отложенной инициализации. 
     */
    void Awake()
    {
        StartCoroutine(DelayedInit());
        Connect();
    }



    /** \brief Сопрограмма отложенной инициализаии объекта
     * Получает ссылку на глобальный дистрибутор данных и запускает подключение к серверу по MQTT.
     * Задержка в инициализации требуется чтобы а) Дистрибутор успел инициализироваться и б) чтобы тот успел собрать все источники на сцене. 
     */
    IEnumerator DelayedInit()
    {
        yield return new WaitForSeconds(1f); //Без этой задержки дистрибутор не успевает найти сурсы
        //distributor = GameObject.Find("Distributor228").GetComponent<Distributor>();
        distributor = Distributor.GetInstance();
        //sources = distributor.sources;
        //ources = Distributor.GetInstance().GetSources();
        /*
        int c = 0;
        foreach (DataSource i in sources)
        {
            sourcesnames.Add(i.gameObject.name);
            c++;
        }
        */

        Connect();
    }

    /** \brief Функция создания клиента MQTT и подключения его к брокеру
     * Задает такие параметры подключения, как IP адрес MQTT брокера, порт, ID клиента, его догин и пароль, а также топик, на которых необходимо подписаться. 
     * Далее по заданным параметрам создает MQTT клиента, привязывает Callback функцию получения сообщений к функции OnMessage, затем 
     * производит подключение клиента к брокеру и подписывается на необходимый топик.
     */
    private void Connect()
    {
        client1 = new MqttClient(IPAddress.Parse(brocker_ip), 1883, false, null);
        client1.MqttMsgPublishReceived += OnMessage;                               //привязка Callback функции
        client1.Connect(clientId, username, password);
        client1.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    /** \brief Функция обработки полученных сообщений
     * По приходу нового JSON сообщения парсит его, а затем производит поиск по датчикам, имена которых содержат специальное ключевое слово, 
     * которое позволяет идентифицировать те датчики, которые необходимо визуализировать. Если имя датчика содержит данное слово, то, значение с 
     * этого датчика, имя устроства, на котором стоит датчик, а также имя самого датчика передаются в функцию SourceSet(). Затем происходит 
     * проверка наличия триггера у датчика и, при наличии триггера, передает в функцию SourceSet() значение триггера, а также его имя и имя устройства, 
     * на котором возникла ошибка.
     */
    void OnMessage(object sender, MqttMsgPublishEventArgs e)
    {
        Debug.Log("Server Online, Reciever got data.");
        string mes = System.Text.Encoding.UTF8.GetString(e.Message);
        Data data = JsonUtility.FromJson<Data>(mes);

        foreach(Result device in data.result)
        {
            foreach (Items sensor in device.items)
            {
                if (sensor.itemname.Contains(keyword))                            /// Если имя датчика содержит ключевое слово
                {
                    int valType = int.Parse(sensor.value_type);

                    if ((valType == 0) || (valType == 3))                       /// Если тип сообщения цифровой
                    {
                        float floatVal = float.Parse(sensor.value, CultureInfo.InvariantCulture.NumberFormat);
                        distributor.SourceSet(device.hostname + "/" + sensor.itemname, floatVal);
                    }
                    if (sensor.trigger != null)                                  /// Если к датчику привязан триггер
                    {
                        float floatVal = sensor.trigger.value;             
                        string alarmName = sensor.itemname + "_alarm";
                        distributor.SourceSet(device.hostname + "/" + alarmName, floatVal);
                    }
                }
            }
        }
    }
    /*
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
    */
}
