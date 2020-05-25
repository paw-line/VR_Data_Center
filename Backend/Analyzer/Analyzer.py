## \file Analyzer.py
#  \brief Обрабатывает полученные данные, генерирует сигналы от триггеров, передает обработанную информацию в приложение визуализации.
#  \authors Стрельцов Г. А.
#  \version 2.0
#  \date 15.05.20
#
#  Данный скрипт предназначен для обработки и анализа получаемых данных от скрипта Collector.py, а также для  дальнейшей передачи
#  этих данных в приложение визуализации (конкретно в скрипт Receiver,cs). По сути своей данный скрипт является модулем, методы которого в последствии
#  используются в скрипте-сервисе AnalyzerDaemon.py. Алгоритм скрипта Analyzer.py следующий:
#  1. Инициализация. Данный этап будет пройден лишь один раз при запуске скрипта. На этом этапе происходит чтение из config-файла и инициализация переменных.
#  Config-файл содержит следующие поля: 
#  - MQTT_Brocker - адрес брокера MQTT;\n
#  - MQTTClient_ID - логин пользователя MQTT;\n
#  - MQTTClient_Password - пароль пользователя MQTT;\n
#  - MQTT_Port - порт MQTT;\n
#  - MQTT_Topic_Get - топик для получения данных;\n
#  - MQTT_Topic_Send - топик для отправки данных.\n
#  2. Подключение к MQTT-брокеру и подписка на топик для получения данных.\n
#  3. При получении сообщения обработка сообщения и генерация сигналов тревоги, а также запись этих сигналов в триггер. Данный этап проходит циклично.\n
#  4. Отправка новного сообщения по MQTT в приложение для визуализации. Данный этап проходит циклично.\n
#  5. Запись полученного сообщения в буффер (текстовый файл) для отладочных целей. Данный этап проходит циклично.\n

import re
import paho.mqtt.client as cl
import json
import paho.mqtt.publish as mqtt

## Метод инициализации.
#  \brief Метод для инициализации пользовательских параметров.
#	
#  \return Сообщение об ошибке или data (brocker, mqttClient, mqttPswd, mqttPort, mqttTopicGet, mqttTopicSend).
#  
#  Читает все данные из файла /Analyzer/analyzer.conf, затем обрабатывает их и записывает в переменные. Если в файле содержится какая-то ошибка, то
#  пользователю будет выведено соответствующее сообщение. Если же все данные считались корректно, то метод возвращает эти данные.
def Init():

	##  Адрес config-файла
	config = open("/Analyzer/analyzer.conf", "r")
	##  Адрес MQTT-брокера
	brocker = ""
	##  Логин пользователя MQTT
	mqttClient = ""
	##  Пароль пользователя MQTT
	mqttPswd = ""
	##  MQTT порт
	mqttPort = ""
	##  Топик получения данных
	mqttTopicGet = ""
	##  Топик передачи данных
	mqttTopicSend = ""
	
	#  Чтение данных из файла и их обработка
	for i in config:
		if i:
			data = re.split(r'\n', i)
			normData = re.split(r'=', data[0])
			if normData[0] == "MQTT_Brocker":
				brocker = normData[1]
			elif normData[0] == "MQTTClient_ID":
				mqttClient = normData[1]
			elif normData[0] == "MQTTClient_Password":
				mqttPswd = normData[1]
			elif normData[0] == "MQTT_Port":
				mqttPort = normData[1]	
			elif normData[0] == "MQTT_Topic_Get":
				mqttTopicGet = normData[1]	
			elif normData[0] == "MQTT_Topic_Send":
				mqttTopicSend = normData[1]	
	config.close()

	#  Вывод сообщения об ошибке или возвращение параметров
	if not brocker or not mqttClient or not mqttPswd or not mqttPort or not mqttTopicGet or not mqttTopicSend:
		print("Error: empty parameters in collector.conf")
	else:
		mqttPort = int(mqttPort)
		data = {"host": brocker, "client": mqttClient, "password": mqttPswd, "port": mqttPort, 
				"topicGet": mqttTopicGet, "topicSend": mqttTopicSend}
		return data

## Метод анализа полученного сообщения.
#  \brief Метод для анализа полученного сообщения, генерации сигналов триггеров и записи этих сигнало в исходное сообщение.
#  
#  \param data - тело сообщения.
#  \return data - измененное сообщение с вписанными в него сигналами триггеров.
#  
#  Производит чтение тела сообщения, а затем, на основании правил для триггеров, которые содержатся в данном сообщении, 
#  генерирует сигналы (0 - в случае, если значение с датчика не подходит под условия триггера, 1 - в случае, если 
#  значение с датчика не подходит под условия триггера). После этого в сообщении значения "value" триггера меняются, если была получена 1 или
#  остается нетронутыми, если триггер не был сработан. Метод возвращает измененное сообшение.
def Analizator(data):

	# Начало чтения сообщения
	for i in data:
		for j in i["items"]:
			if "trigger" in j:
				trigger = j["trigger"]
				# Проверка триггеров и генерация сигналов
				if "lower" in trigger:
					if float(j["value"]) < float(trigger["lower"]):
						trigger.update({"value": 1})
				if "upper" in trigger:
					if float(j["value"]) > float(trigger["upper"]):
						trigger.update({"value": 1})
				if "equal" in trigger:
					if j["value"] == trigger["equal"]:
						trigger.update({"value": 1})

	# Возвращение измененного сообщения
	return data

## Метод отправления сообщения.
#  \brief Метод для формирования JSON сообщения и последующей его отправки.
#  
#  \param msg - тело сообщения, userdata - пользовательские параметры, time - временная метка сообщения.
#  \return normMsg - сообщение в формате JSON.
#  
#  Формирует JSON сообщение из словаря, где тело сообщения включается в ключ "result", а временная метка в ключ "time".
#  После этого сообщение отправляется по MQTT.
def Publisher(msg, userdata, time):

	# Формирование JSON сообщения
	normMsg = {"result": msg, "time": time}
	normMsg = json.dumps(normMsg)
	mqtt.single(userdata["topicSend"], normMsg, hostname=userdata["host"], 
				port=userdata["port"], auth={'username': userdata["client"], 
				'password': userdata["password"]})

	# Возвращение готового JSON сообщения
	return normMsg

## Метод записи сообщения в буффер.
#  \brief Метод для записи обработанного сообщения в буффер.
#  
#  \param msg - сообщение.
#  
#  На вход получет готовое JSON сообщение, а затем записывает его в буффер (текстовый файл /Analyzer/DB_buffer.txt). Предназнначен для отладки.
def BufferizeInfo(msg):

	buf = open("/Analyzer/DB_buffer.txt", "w")
	buf.write(msg)
	buf.close()

## Метод подписки на топик.
#  \brief Метод для подписки на топик для получения сообщений.
#  
#  \param client, userdata, flags, rc (См. документацию Paho-MQTT).
#  
#  Привязывается к callback функции, вызывается при успешном подключении к брокеру MQTT. После подключения подписывает на нужный топик.
def on_connect(client, userdata, flags, rc):

    res = client.subscribe(userdata["topicGet"])

## Метод получения сообщения.
#  \brief Метод для получения и обработки полученных сообщений.
#  
#  \param client, userdata, msg (См. документацию Paho-MQTT).
#  
#  Привязывается к callback функции, вызывается при получении сообщения. Если поступило какое-либо сообшение, то оно парсится и передается
#  на анализ в метод Analizator(), затем вызывает методы Publisher() и BufferizeInfo().
def on_message(client, userdata, msg):

    message = json.loads(msg.payload)
    time = message["time"]
    data = message["result"]
    sendMsg = Analizator(data)
    BufferizeInfo(Publisher(sendMsg, userdata, time))

## Метод инициализации MQTT-подключения.
#  \brief Метод для инициализации переменных и функция для MQTT-подключения.
#  
#  \param userData - пользовательские параметры.
#  
#  Создает MQTT-клиента, инциализирует подключение по логину и паролю, инициализирует callback-функции on_connect() и on_message(), а затем
#  производит подключение к MQTT-брокеру и уходит в бесконечный цикл прослушивания приходящих сообщений.
def MQTTConnection(userData):
	
	# Создание клиента
	client = cl.Client()
	client.username_pw_set(username=userData["client"], password=userData["password"])
	client.user_data_set(userData)
	# Инициализация callback-функций
	client.on_connect = on_connect
	client.on_message = on_message
	# Подключение к брокеру
	client.connect(userData["host"], userData["port"])
	client.loop_forever()

# Основная программа
if __name__ == '__main__':
	#  Если инициализация прошла успешно
	if Init():
		#  Чтение параметров
		userData = Init()
		# Подключение по MQTT
		MQTTConnection(userData)