## \file DBwriter.py
#  \brief Предназначен для работы с базой данных.
#  \authors Стрельцов Г. А.
#  \version 1.0
#  \date 15.05.20
#
#  Данный скрипт предназначен для получения данных от скрипта Analyzer.py, а также для работы с базой данных (БД) и записи 
#  полученных параметров в БД. По сути своей данный скрипт является модулем, методы которого в последствии
#  используются в скрипте-сервисе DBwriterDaemon.py. Алгоритм скрипта DBwriter.py следующий:
#  1. Инициализация. Данный этап будет пройден лишь один раз при запуске скрипта. На этом этапе происходит чтение из config-файла и инициализация переменных.
#  Config-файл содержит следующие поля: 
#  - MQTT_Brocker - адрес брокера MQTT;\n
#  - MQTTClient_ID - логин пользователя MQTT;\n
#  - MQTTClient_Password - пароль пользователя MQTT;\n
#  - MQTT_Port - порт MQTT;\n
#  - MQTT_Topic - топик для получения данных;\n
#  - DB_Time - время хранения данных в БД в днях;\n
#  - DB_Name - имя БД;\n
#  - DB_User_Name - имя пользователя БД;\n
#  - DB_User_Password - пароль пользователя БД;\n
#  2. Подключение к MQTT-брокеру и подписка на топик для получения данных.\n
#  3. При получении сообщения запись его параметров в БД. Данный этап проходит циклично.\n

import re
import paho.mqtt.client as cl
import json
import datetime
from datetime import datetime, timedelta
import time
from sqlalchemy import Column, BigInteger, String, DateTime, ForeignKey, create_engine, and_
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy_utils.functions import database_exists
from sqlalchemy.orm import relationship
from sqlalchemy.orm import sessionmaker

# Объявление класса Base, от которого будут наследоваться классы таблиц
Base = declarative_base()

## Класс Host.
#  \brief Класс, предназначенный для создания таблицы "hosts" в БД.
#	
#  Данный класс наследуется от встроенного класса Base, предоставляемого пакетом SQLAlchemy.
#  Предназаначен для хранения различных параметров таблицы 'hosts' и обработки операций с этой таблицей. 
#  По сути содержит в себе список узлов сети и их параметров. Связан с таблицей элементов данных ("items").
class Host(Base):
	__tablename__ = 'hosts'

	## Id узла сети (первичный ключ)
	hostId = Column(BigInteger, primary_key=True)
	## Имя узла сети
	name = Column(String(100))
	## Последнее использование узла сети ("1111-11-11 11:11:11" - если используется сейчас)
	useTime = Column(DateTime)
	item = relationship("Item", backref='hosts', passive_deletes=True)

## Класс Item.
#  \brief Класс, предназначенный для создания таблицы "items" в БД.
#	
#  Данный класс наследуется от встроенного класса Base, предоставляемого пакетом SQLAlchemy.
#  Предназаначен для хранения различных параметров таблицы 'items' и обработки операций с этой таблицей. 
#  По сути содержит в себе список элементов данных и их параметров. Связан с таблицами "datahistory", "hosts", "triggerhistory".
class Item(Base):
	__tablename__ = 'items'

	## Id элемента данных (первичный ключ)
	itemId = Column(BigInteger, primary_key=True)
	## Имя элемента данных
	name = Column(String(100))
	## Формат приходящих данных
	dataType = Column(String(100))
	## Последнее использование узла сети ("1111-11-11 11:11:11" - если используется сейчас)
	useTime = Column(DateTime)
	## Id узла сети (внешний ключ)
	hostId = Column(BigInteger, ForeignKey('hosts.hostId', ondelete='CASCADE'))
	dataHistory = relationship("DataHistory", backref='items', passive_deletes=True)
	trigHistory = relationship("TriggerHistory", backref='items', passive_deletes=True)

## Класс DataHistory.
#  \brief Класс, предназначенный для создания таблицы "datahistory" в БД.
#	
#  Данный класс наследуется от встроенного класса Base, предоставляемого пакетом SQLAlchemy.
#  Предназаначен для хранения различных параметров таблицы 'datahistory' и обработки операций с этой таблицей. 
#  По сути содержит в себе историю поллученных данных с элементов данных. Связан с таблицей "items".
class DataHistory(Base):
	__tablename__ = 'datahistory'

	## Id записи (первичный ключ)
	id = Column(BigInteger, primary_key=True)
	## Временная метка записи
	time = Column(DateTime)
	## Последнее полученное значение
	value = Column(String(100))
	## Id элемента данных (внешний ключ)
	itemId = Column(BigInteger, ForeignKey('items.itemId', ondelete='CASCADE'))

## Класс TriggerHistory.
#  \brief Класс, предназначенный для создания таблицы "triggerhistory" в БД.
#	
#  Данный класс наследуется от встроенного класса Base, предоставляемого пакетом SQLAlchemy.
#  Предназаначен для хранения различных параметров таблицы 'triggerhistory' и обработки операций с этой таблицей. 
#  По сути содержит в себе историю состояний триггеров. Связан с таблицей "items".
class TriggerHistory(Base):
	__tablename__ = 'triggerhistory'

	## Id записи (первичный ключ)
	id = Column(BigInteger, primary_key=True)
	## Временная метка записи
	time = Column(DateTime)
	## Последнее полученное значение
	value = Column(String(100))
	## Верхний порог триггера;
	upper = Column(String(100))
	## Нижний порог триггера;
	lower = Column(String(100))
	## Значение, при равенстве с которыми срабатывает триггер
	equal = Column(String(100))
	## Id элемента данных (внешний ключ)
	itemId = Column(BigInteger, ForeignKey('items.itemId', ondelete='CASCADE'))

## Метод работы с БД.
#  \brief Метод для создания таблиц в БД, а также записи полученных данных в эти таблицы.
#  
#  \param data - тело сообщения, mesTime - время отправки сообщения, userData - различные пользовательские параметры.
#  
#  В самом начале исполнения проверяет, существует ли БД, к которой пользователь хотел бы подключиться и, если она существует, то 
#  подключается к БД MySQL по заданным параметрам.Затем проверяет, есть ли необходимые для записи талицы в рассматриваемой БД. 
#  Если их нет, то самостоятельно создает их. Далее создается объект Session, предназначенный для создания сесси БД. Как только сессия создана,
#  создаются 2 SQL-запроса: первый возвращает все записи из таблицы "hosts", а второй возвращает все записи из таблицы "items". После этого
#  по данным из запроса формируются словари, в которых содержатся параметры возвращенных записей. После этого идет проверка полученного сообщения.
#  В этот момент идет проверка наличия в полученном сообщении узлов сети или элементов данных, отсутствующих в соответствующих таблицах.
#  Если такие элементы отсутствуют в БД, то они записываются в специальный список, а позже заносятся в БД. Если узел сети или элемент данных
#  присутствуют в БД, то идет проверка на то, не изменились ли каки-либо данные. Если выявлется, что какой-либо из параметров в сообщении не соответствует
#  БД, то идет обновление значения в соответствии с полученным сообщением. Если узел сети или элемент данных приходят с сообщением, то в колонки useTime
#  соответствующих таблиц заполняются значением "1111-11-11 11:11:11", которое обозначает, что данный элемент используется на данный момент.
#  Если же в сообщении отсутствует какой-либо элемент, который есть в таблицах "hosts" или "items", то в колонке useTime проставляется последнее 
#  время его использования. Таким образом БД содержит информацию об элементах, которые использовались ранее, но не используются сейчас. Добавление 
#  информации в таблицы "datahistory" и "triggerhistory" происходит по приходу сообщения без предварительных проверок. Также данный метод производит
#  удаление старых записей во всех таблицах по временным отметкам, содержащихся в таблицах. Срок, по истечении которого запись считается устаревшей
#  задется пользователем (см. функцию Init()).
def WriteData(data, mesTime, userData):

	# Адрес подключения к БД MySQL
	connectAdress = "mysql+pymysql://" + userData["dbULogin"] + ":" + userData["dbUPswd"] + "@localhost/" + userData["dbName"]
	# Создание объекта MySQL engine
	engine = create_engine(connectAdress,echo = False)

	# Если БД существует
	if database_exists(engine.url):

		# Если в БД нет какой-либо таблицы
		if not engine.dialect.has_table(engine, "hosts") or not engine.dialect.has_table(engine, "items") \
			or not engine.dialect.has_table(engine, "triggers") or not engine.dialect.has_table(engine, "datahistory") \
			or not engine.dialect.has_table(engine, "triggerhistory"):
			# Создание таблиц, описанных в классах
			Base.metadata.create_all(engine)

		# Создание объекта MySQL Session
		Session = sessionmaker(bind=engine)
		session = Session()

		hIds = {}
		# Создание SQL-запроса с целью получения всех записей таблицы "hosts"
		query = session.query(Host)
		result = query.all()
		# Создание словаря с данными всех записей таблицы "hosts"
		for i in result:
			hostData = {"name":  i.name, "time": i.useTime}
			hIds.update({i.hostId: hostData})

		iIds = {}
		# Создание SQL-запроса с целью получения всех записей таблицы "items"
		query = session.query(Item)
		result = query.all()
		# Создание словаря с данными всех записей таблицы "items"
		for i in result:
			itemData = {"name":  i.name, "time": i.useTime, "dataType": i.dataType}
			iIds.update({i.itemId: itemData})

		newHosts = []
		newItems = []
		newHostIds = []
		newItemIds = []
		dHistList = []
		tHistList = []

		for i in data:
			# Сохранение Id узлов сети для последующих проверок
			newHostIds.append(int(i["hostid"]))

			# Если Id узла сети нет в таблице "hosts"
			if int(i["hostid"]) not in hIds.keys():
				# Составление списка новых узлов сети
				hostBuf = Host(hostId=int(i["hostid"]), name=i["hostname"], useTime="1111-11-11 11:11:11")
				newHosts.append(hostBuf)

			# Если Id узла сети есть в таблице "hosts"
			else:
				# Если какие-либо данные не соответствуют
				if str(hIds[int(i["hostid"])]["time"]) != "1111-11-11 11:11:11" or hIds[int(i["hostid"])]["name"] != i["hostname"]:
					# Обновление данных
					h = session.query(Host).filter(Host.hostId == int(i["hostid"])).first()
					h.name = i["hostname"]
					h.useTime = "1111-11-11 11:11:11"
					session.commit()

			for j in i["items"]:
				# Сохранение данных для таблицы "datahistory"
				dHistBuf = DataHistory(time=mesTime, value=j["value"], itemId=int(j["itemid"]))
				dHistList.append(dHistBuf)
				# Сохранение Id элементов данных для последующих проверок
				newItemIds.append(int(j["itemid"]))

				# Если Id элемента данных нет в таблице "items"
				if int(j["itemid"]) not in iIds.keys():
					# Составление списка новых элементов данных
					itemBuf = Item(itemId=int(j["itemid"]), name=j["itemname"], dataType=j["value_type"],
									hostId=i["hostid"], useTime="1111-11-11 11:11:11")
					newItems.append(itemBuf)

				# Если Id элемента данных есть в таблице "items"
				else:
					# Если какие-либо данные не соответствуют
					if str(iIds[int(j["itemid"])]["time"]) != "1111-11-11 11:11:11" or \
						iIds[int(j["itemid"])]["name"] != j["itemname"] or \
						iIds[int(j["itemid"])]["dataType"] != j["value_type"]:

						# Обновление данных
						itm = session.query(Item).filter(Item.itemId == int(j["itemid"])).first()
						itm.name = j["itemname"]
						itm.useTime = "1111-11-11 11:11:11"
						itm.dataType = j["value_type"]
						session.commit()

				# Если у элемента данных в сообщении есть триггер
				if "trigger" in j:
					# Сохранение данных для таблицы "triggerhistory"
					trigger = j["trigger"]
					tHistBuf = TriggerHistory(time=mesTime, value=trigger["value"], itemId=int(j["itemid"]))

					if "lower" in trigger:
						tHistBuf.lower = trigger["lower"]
					if "upper" in trigger:
						tHistBuf.upper = trigger["upper"]
					if "equal" in trigger:
						tHistBuf.equal = trigger["equal"]

					tHistList.append(tHistBuf)

		#Добавление новых узлов сети
		session.add_all(newHosts)
		session.commit()

		#Добавление новых элементов данных
		session.add_all(newItems)
		session.commit()

		#Добавление записей в историю полученных данных
		session.add_all(dHistList)
		session.commit()

		#Добавление записей в историю триггеров
		session.add_all(tHistList)
		session.commit()

		#Все устаревшие узлы сети		
		query = session.query(Host).filter(and_(Host.hostId.notin_(newHostIds), Host.useTime == \
				"1111-11-11 11:11:11")).update({Host.useTime:mesTime}, synchronize_session = False)
		session.commit()

		#Все устаревшие элементы данных		
		query = session.query(Item).filter(and_(Item.itemId.notin_(newItemIds), Item.useTime == \
				"1111-11-11 11:11:11")).update({Item.useTime:mesTime}, synchronize_session = False)
		session.commit()

		# Преобразование строки в datetime
		normTime = mesTime.split(".")
		newMTime = datetime.strptime(normTime[0], "%Y-%m-%d %H:%M:%S" )
		# Установка даты дедлайна для данных
		deadLine = newMTime - timedelta(days=int(userData["dbTime"]))

		#Удаление всех старых узлов сети
		query = session.query(Host.useTime).filter(and_(Host.useTime <= deadLine, Host.useTime != \
				"1111-11-11 11:11:11"))
		query.delete(synchronize_session=False)
		session.commit()

		#Удаление всех старых элементов данных
		query = session.query(Item.useTime).filter(and_(Item.useTime <= deadLine, Item.useTime != \
				"1111-11-11 11:11:11"))
		query.delete(synchronize_session=False)
		session.commit()

		#Удаление старых записей из истории данных
		query = session.query(DataHistory.time).filter(DataHistory.time <= deadLine)
		query.delete(synchronize_session=False)
		session.commit()

		#Удаление старых записей из истории триггеров
		query = session.query(TriggerHistory.time).filter(TriggerHistory.time <= deadLine)
		query.delete(synchronize_session=False)
		session.commit()

## Метод инициализации.
#  \brief Метод для инициализации пользовательских параметров.
#	
#  \return Сообщение об ошибке или data (brocker, mqttClient, mqttPswd, mqttPort, mqttTopic, dbTime, dbName, dbULogin, dbUPswd).
#  
#  Читает все данные из файла /DBwriter/dbwriter.conf, затем обрабатывает их и записывает в переменные. Если в файле содержится какая-то ошибка, то
#  пользователю будет выведено соответствующее сообщение. Если же все данные считались корректно, то метод возвращает эти данные.
def Init():

	##  Адрес config-файла
	config = open("/DBwriter/dbwriter.conf", "r")
	##  Адрес MQTT-брокера
	brocker = ""
	##  Логин пользователя MQTT
	mqttClient = ""
	##  Пароль пользователя MQTT
	mqttPswd = ""
	##  MQTT порт
	mqttPort = ""
	##  Топик получения данных
	mqttTopic = ""
	##  Время хранения данных в БД в днях
	dbTime = ""
	##  Имя БД
	dbName = ""
	##  Логин пользователя БД
	dbULogin = ""
	##  Пароль пользователя БД
	dbUPswd = ""
	
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
			elif normData[0] == "MQTT_Topic":
				mqttTopic = normData[1]	
			elif normData[0] == "DB_Time":
				dbTime = normData[1]	
			elif normData[0] == "DB_Name":
				dbName = normData[1]	
			elif normData[0] == "DB_User_Name":
				dbULogin = normData[1]
			elif normData[0] == "DB_User_Password":
				dbUPswd = normData[1]	
	config.close()

	#  Вывод сообщения об ошибке или возвращение параметров
	if not brocker or not mqttClient or not mqttPswd or not mqttPort or not mqttTopic or not dbTime or not dbName or not dbULogin or not dbUPswd:
		print("Error: empty parameters in collector.conf")
	else:
		mqttPort = int(mqttPort)
		data = {"host": brocker, "client": mqttClient, "password": mqttPswd, "port": mqttPort, 
				"topic": mqttTopic, "dbTime": dbTime, "dbName": dbName, "dbULogin": dbULogin, 
				"dbUPswd": dbUPswd}
		return data

## Метод подписки на топик.
#  \brief Метод для подписки на топик для получения сообщений.
#  
#  \param client, userdata, flags, rc (См. документацию Paho-MQTT).
#  
#  Привязывается к callback функции, вызывается при успешном подключении к брокеру MQTT. После подключения подписывает на нужный топик.
def on_connect(client, userdata, flags, rc):

    res = client.subscribe(userdata["topic"])

## Метод получения сообщения.
#  \brief Метод для получения и обработки полученных сообщений.
#  
#  \param client, userdata, msg (См. документацию Paho-MQTT).
#  
#  Привязывается к callback функции, вызывается при получении сообщения. Если поступило какое-либо сообшение, то оно парсится и передается
#  на анализ в метод WriteData().
def on_message(client, userdata, msg):

    message = json.loads(msg.payload)
    data = message["result"]
    mesTime = message["time"]
    WriteData(data, mesTime, userdata)

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