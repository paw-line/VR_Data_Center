## \file Collector.py
#  \brief Отвечает за сбор данных из системы мониторинга и передачу их на дальнейшую обработку.
#  \authors Стрельцов Г. А.
#  \version 1.0
#  \date 15.05.20
#
#  Данный скрипт предназначен для сбора данных из системы мониторинга Zabbix и последующей передачи
#  этих данных в скрипт Analyzer.py. По сути своей данный скрипт является модулем, методы которого в последствии
#  используются в скрипте-сервисе CollectorDaemon.py. Алгоритм скрипта Collector.py следующий:
#  1. Инициализация. Данный этап будет пройден лишь один раз при запуске скрипта. На этом этапе происходит чтение из config-файла и инициализация переменных.
#  Config-файл содержит следующие поля: 
#  - Server - адрес Zabbix-сервера;\n
#  - Login - логин пользователя Zabbix;\n
#  - Password - пароль пользователя Zabbix;\n
#  - MQTT_Brocker - адрес брокера MQTT;\n
#  - MQTTClient_ID - логин пользователя MQTT;\n
#  - MQTTClient_Password - пароль пользователя MQTT;\n
#  - MQTT_Port - порт MQTT;\n
#  - MQTT_Topic - топик для передачи данных.\n
#  2. Чтение правил для триггеров из файла. На данном этапе происходит чтение пользовательских правил для триггеров. Данный этап проходит циклично. 
#  Правила составляются в следующем формате: Имя узла сети#Имя элемента данных#< или > или =#значение#(возможны еще выражения).\n
#  3. Сбор данных из системы мониторинга Zabbix. На данном этапе происходит подключение к Zabbix API и чтение всех используемых узлов сети и их элементов 
#  данных. Данный этап проходит циклично.\n
#  4. Отправление одним пакетом данных, полученных из Zabbix API и пользовательских правил. Данный этап проходит циклично. 

from pyzabbix.api import ZabbixAPI 
import json
import paho.mqtt.publish as mqtt
import re
import datetime
import time
import os

## Метод инициализации.
#  \brief Метод для инициализации пользовательских параметров.
#	
#  \return Сообщение об ошибке или server, login, password, brocker, mqttClient, mqttPswd, mqttPort, mqttTopic.
#  
#  Читает все данные из файла /Collector/collector.conf, затем обрабатывает их и записывает в переменные. Если в файле содержится какая-то ошибка, то
#  пользователю будет выведено соответствующее сообщение. Если же все данные считались корректно, то метод возвращает эти данные.
def Init():

	##  Адрес config-файла
	config = open("/Collector/collector.conf", "r")
	##  Адрес Zabbix-сервера
	server = ""
	##  Логин пользователя Zabbix
	login = ""
	##  Пароль пользователя Zabbix
	password = ""
	##  Адрес MQTT-брокера
	brocker = ""
	##  Логин пользователя MQTT
	mqttClient = ""
	##  Пароль пользователя MQTT
	mqttPswd = ""
	##  MQTT порт
	mqttPort = ""
	##  Топик передачи данных
	mqttTopic = ""
	
	#  Чтение данных из файла и их обработка
	for i in config:
		if i:
			data = re.split(r'\n', i)
			normData = re.split(r'=', data[0])
			if normData[0] == "Server":
				server = normData[1]
			elif normData[0] == "Login":
				login = normData[1]
			elif normData[0] == "Password":
				password = normData[1]
			elif normData[0] == "MQTT_Brocker":
				brocker = normData[1]
			elif normData[0] == "MQTTClient_ID":
				mqttClient = normData[1]
			elif normData[0] == "MQTTClient_Password":
				mqttPswd = normData[1]
			elif normData[0] == "MQTT_Port":
				mqttPort = normData[1]	
			elif normData[0] == "MQTT_Topic":
				mqttTopic = normData[1]	
	config.close()

	#  Вывод сообщения об ошибке или возвращение параметров
	if not server or not login or not password or not brocker or not mqttClient or not mqttPswd or not mqttPort or not mqttTopic:
		print("Error: empty parameters in collector.conf")
	else:
		return server, login, password, brocker, mqttClient, mqttPswd, mqttPort, mqttTopic

## Метод сбора данных и формирования сообщения.
#  \brief Метод для сбора данных, а также для создания сообщения для передачи в формате JSON.
#  
#  \param triggers - список всех пользовательских правил, server - адрес Zabbix-сервера, login - логин пользователя Zabbix, password - пароль пользователя Zabbix.
#  \return msg - Сообщение с полученными данными в формате JSON.
#  
#  Производит подключение к Zabbix-серверу, затем последовательно делает запросы, из которых получает данные: \n
#  - Все рабочие узлы сети и их параметры;\n
#  - Все рабочие элементы данных и их параметры (в т.ч последнее полученное значение).\n
#  Далее составляется сообщение, где каждому рабочему хосту ставятся в соответствие все элементы данных, которые к нему относятся.
#  После этого происходит проверка наличия триггеров у каждого из элементов данных, и, в случае нахождения, каждому элементу данных 
#  приписывается соответствующий триггер. Все эти данные преобразуются в словарь и получить их можно по ключу "result". 
#  Также в сообщение добавляется отметка времени, когда было собрано сообщение. После этого все сообщение преобразуется в JSON. Далее
#  сформированное сообщение возвращается. 
def GetData(triggers, server, login, password):

	# Создание сущности ZabbixAPI
	zapi = ZabbixAPI(url=server, user=login, password=password)

	# Запрос на получение всех рабочих узлов сети.
	request1 = zapi.host.get(monitored_hosts=1, output='extend')
	hostIds = [i['hostid'] for i in request1]

	# Запрос на получение всех элементов данных.
	request2 = zapi.do_request('item.get',
	                          {
	                            "hostids": hostIds
	                          })
	hosts = []
	# Формирование сообщения
	for i in request1:
		items = []
		for j in request2['result']:
			if j['hostid'] == i['hostid']:
				buf = {"itemid": j['itemid'], "itemname": j['name'], "value_type": j['value_type'], 
						"value": j['lastvalue']}
				for k in triggers:
					if k['host'] == i['name'] and k['item'] == j['name']:
						buf.update({"trigger": k})
				items.append(buf)
		hosts.append({"hostname": i['name'], "hostid": i['hostid'], "items": items})

	time = str(datetime.datetime.now())
	msg = {"result": hosts, "time": time}

	# Преобразование сообщения в JSON
	msg = json.dumps(msg)

	# Выход из Zabbix
	zapi.user.logout()

	# Возвращение готового сообщения
	return msg

## Метод парсинга выражения для триггера.
#  \brief Метод для парсинга пользовательского правила для триггеров.
#  
#  \param exp - выражение (<, >, =).
#  \return "lower", "upper", "equal", "unknown".
#  
#  Возвращает "lower", если был получен символ "<"; "upper", если был получен символ ">"; 
#  "equal", если был получен символ "="; "unknown", если был получен неизвестный символ.
def DefineExp(exp):

	if exp == "<":
		return "lower"
	elif exp == ">":
		return "upper"
	elif exp == "=":
		return "equal"
	else:
		return "unknown"

## Метод считывания триггеров.
#  \brief Метод для чтения пользовательских правил для триггеров из файла trigger.txt.
#  
#  \return allTriggers - все триггеры в формате словаря.
#  
#  Производит чтение из файла всех пользовательских правил для триггеров, а затем записывает их в словарь в следующем формате:\n
#  - "host" - имя узла сети, где используется триггер;
#  - "item" - имя элемента данных, где используется триггер;
#  - "value" - значение триггера (0 - по умолчанию);
#  - "lower" - нижний порог триггера;
#  - "upper" - верхний порог триггера;
#  - "equal" - значения, при равенстве с которыми срабатывает триггер;
#  - "unknown" - неизвестный параметр;
def GetTriggers():

	##  Адрес trigger-файла
	file = open("/Collector/trigger.txt", "r")

	allTriggers = []

	for i in file:
		if i:
			data = re.split(r'\n', i)
			normData = re.split(r'#', data[0])
			if len(normData) == 4:
				allTriggers.append({"host": normData[0], "item": normData[1], "value": 0, DefineExp(normData[2]): normData[3]})
			elif len(normData) == 6:
				allTriggers.append({"host": normData[0], "item": normData[1], "value": 0, DefineExp(normData[2]): normData[3], 
									DefineExp(normData[4]): normData[5]})
			elif len(normData) == 8:
				allTriggers.append({"host": normData[0], "item": normData[1], "value": 0, DefineExp(normData[2]): normData[3], 
									DefineExp(normData[4]): normData[5], DefineExp(normData[6]): normData[7]})
	
	file.close()

	# Возвращает все триггеры  в формате словаря
	return allTriggers

#  Основная программа
if __name__ == '__main__':
	#  Если инициализация прошла успешно
	if Init():
		#  Чтение параметров
		server, login, password, brocker, mqttClient, mqttPswd, mqttPort, mqttTopic = Init()
		mqttPort = int(mqttPort)
		while True:
			#  Получение сформированного сообщения
			msg = GetData(GetTriggers(), server, login, password)
			#  Отправка сообщения по указанному топику
			mqtt.single(mqttTopic, msg, hostname=brocker, port=mqttPort, auth={'username': mqttClient, 'password': mqttPswd}) 	#Отправка данных на MQTT брокер
			time.sleep(1)
