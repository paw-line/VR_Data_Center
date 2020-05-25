import time
import random
from threading import Thread
import paho.mqtt.publish as mqtt
import json
import os

## \file Server.py
#  \brief Эмулятор серверного оборудования.
#  \authors Стрельцов Г. А.
#  \version 1.0
#  \date 8.04.20
#
#  Данный скрипт предназанчен для эмуляции данных серверного оборудования
#  и передачи их на MQTT брокер Mosquitto. В программе предусмотрена возможность
#  ручного управления состоянием эмулированного серверного оборудования.
#  Скрипт создавался с целями отладки и тестирования взаимодействий между подсистемами.

## Класс Servers
#  \brief Класс для реализации серверного оборудования.
#  \authors Стрельцов Г. А.
#  \version 1.0
#  \date 8.04.20
#
#  Класс Servers содержит в себе различные параметры, схожие с параметрами
#  реального серверного оборудования, и методы необходимые для работы с 
#  параметрами. Также Servers содержит методы для реализации ручного управления
#  серверным оборудованием посредством консольного окна.
class Servers:

	## Температура в серверном помещении (в градусах Цельсия)
	roomTemp = 20 			
	## Температура оборудования (в градусах Цельсия)
	servTemp = 60 			
	## Влажность в серверном помещении (в процентах)
	hum = 80 				
	## Нагрузка на процессор (в процентах)
	cpu = 20 				
	## Заполненность оперативной памяти (в процентах)
	mem = 20 				
	## Заполненность хранилища данных (в процентах)
	storage = 20 			
	## Количество свободных портов
	freePorts = 5 			
	## Общее количество портов
	totalPorts= 5 			

	## Количество вызовов метода StartChrome()
	chrome = 0 				
	## Флаг (было ли включено серверное оборудование или нет)
	IsWorking = True 		

	## "Запуск Chrome"
	#  \brief Метод, создающий искусственную нагрузку на процессор и оперативную память серверного оборудования.
	#
	#  При первоначальном вызове метода для конкретного оборудования постепенно повышает загруженность процессора 
	#  и памяти с шагом в 13 процентов. При повторных вызовах метода проверяется наличие соответствующих ресурсов. 
	#  Если процессор и память перегружены, то пользователю выдается соответствующее уведомление. Если ресурсов 
	#  хватает только на один запуск, то аттрибутам cpu и mem присваивается значение в 99%.
	def StartChrome(self):
		print("Starting Chrome and opening tab")
		if (self.cpu > 80 and self.mem > 50):
			if (self.cpu == 99 and self.mem == 99):
				print("Total overload of cpu and memory, can't open one more tab")
				self.Stats()
			else:
				self.cpu = 99
				self.mem = 99
				self.chrome += 1
				print("Total overload of cpu and memory")
				self.Stats()
		else:
			for i in range(0,5):
				self.cpu += 13
				self.mem += 13
			self.chrome += 1
			self.Stats()

	## "Закрытие Chrome"
	#  \brief Метод, снижающий искусственную нагрузку на процессор и оперативную память серверного оборудования.
	#
	#  Выполняется при условии, что был выполнен метод по нагрузке процессора и оперативной памяти (метод StartChrome()).
	#  Постепенно снижает нагрузку на процессор и оперативную память с шагом в 13%. 
	def CloseChrome(self):
		if self.chrome >= 1:
			print("Closing Chrome Tab")
			if (self.chrome == 1):
				self.cpu = 20
				self.mem = 20
			else:
				for i in range(0,5):
					self.cpu -= 13
					self.mem -= 13
			self.chrome -= 1
			self.Stats()
		else:
			print("Chrome is already closed")
			self.Stats()

	## "Нагреть сервер"
	#  \brief Метод, повышающий температуру серверного оборудования.
	#
	#  Повышает температуру с шагом в 50 градусов Цельсия.
	def HeatServer(self):
		print("Burning the server")
		self.servTemp += 50
		self.Stats()

	## "Охладить сервер"
	#  \brief Метод, понижающий температуру серверного оборудования. Минимально возможное значение температуры: 10 градусов Цельсия.
	#
	#  Постепенно снижает температуру с шагом в 21 градус Цельсия. При достижении 
	#  минимально допустимой температуры пользователю выводится соответствующее сообщение.
	def CoolServer(self):
		if self.servTemp > 10:
			if self.servTemp > 31:
				print("Cooling the server")
				self.servTemp -= 21
				self.Stats()
			else:
				print("Total freeze of the server")			#Максимально возможное охлаждение
				self.servTemp = 10
				self.Stats()
		else:
			print("Total freeze of the server")				#Максимально возможное охлаждение
			self.servTemp = 10
			self.Stats()

	## "Нагреть комнату"
	#  \brief Метод, повышающий температуру серверного помещения.
	#
	#  Повышает температуру с шагом в 19 градусов Цельсия.
	def Burn(self):
		print("Burning the server room")
		self.roomTemp += 19
		self.Stats()

	## "Охладить комнату"
	#  \brief Метод, понижающий температуру серверного помещения. Минимально возможное значение температуры: 5 градусов Цельсия.
	#
	#  Постепенно снижает температуру с шагом в 21 градус Цельсия. При достижении 
	#  минимально допустимой температуры пользователю выводится соответствующее сообщение.
	def Cool(self):
		if self.roomTemp > 5:
			if self.roomTemp > 26:
				print("Cooling the server room")
				self.roomTemp -= 21
				self.Stats()
			else:
				print("Total freeze of the server room")	#Максимально возможное охлаждение 
				self.roomTemp = 6
				self.Stats()
		else:
			print("Total freeze of the server room")		#Максимально возможное охлаждение
			self.Stats()

	## "Скачать файлы"
	#  \brief Метод, заполняющий место в хранилище серверного оборудования.
	#
	#  При каждом вызове заполняет хранилище на 20 процентов. Если хранилище
	#  заполнено, то пользователь получает соответствующее уведомление.
	def DownloadFilms(self):
		if self.storage < 80:
			print("Downloading films")
			self.storage += 20
			self.Stats()
		else: 
			print("Out of memory")							#Полное заполнение хранилища
			self.Stats()

	## "Удалить файлы"
	#  \brief Метод, освобождающий место в хранилище серверного оборудования.
	#
	#  Выполняется при условии, что был выполнен метод, заполняющий место в хранилище серверного оборудования. (метод DownloadFilms()).
	#  При каждом вызове освобождает хранилище на 20 процентов. Если хранилище
	#  пустое, то пользователь получает соответствующее уведомление.
	def DeleteFilms(self):
		if self.storage >= 20:
			print("Deleting Films")
			self.storage -= 20
			self.Stats()
		elif self.storage >0:
			print("Deleting Films")
			self.storage = 0
			self.Stats()
		else: 
			print("No available files")						#Полное освобождение хранилища
			self.Stats()

	## "Разлить воду"
	#  \brief Метод, повышающий влажность в серверном помещении. Максимальная влажность - 100%.
	#
	#  При каждом вызове повышает влажность помещения на 17 процентов.
	def SpillWater(self):
		print("Spilling water")
		if self.hum <= 83:
			self.hum += 17
			self.Stats()
		else:
			self.hum = 100									#Максимальная влажность
			self.Stats()

	## "Высушить"
	#  \brief Метод, понижающий влажность в серверном помещении. Минимальная влажность - 0%.
	#
	#  При каждом вызове снижает влажность помещения на 17 процентов.
	def Dry(self):
		print("Drying server room")
		if self.hum >= 17:
			self.hum -= 17
			self.Stats()
		else:
			self.hum = 0									#Минимальная влажность
			self.Stats()

	## "Подключить устройство"
	#  \brief Метод, заполняющий свободные порты серверного оборудования.
	#
	#  При каждом вызове занимает один порт оборудования. Если все порты заняты, пользователь
	#  получит соответствующее уведомление.
	def ConnectDevice(self):
		if self.freePorts > 0:
			print("Connecting device")
			self.freePorts -= 1
			self.Stats()
		else:
			print("No free ports")
			self.Stats()

	## "Отключить устройство"
	#  \brief Метод, заполняющий свободные порты серверного оборудования. По умолчанию заполняет один порт.
	#
	#  При каждом вызове освобождает один порт оборудования. Если все порты свободны, пользователь
	#  получит соответствующее уведомление.
	def DisconnectDevice(self):
		if self.totalPorts - self.freePorts > 0:
			self.freePorts += 1
			print("Disconnecting device")
			self.Stats()
		else:
			print("No devices connected")
			self.Stats()

	## "Статистика"
	#  \brief Метод, выводящий пользователю все параметры нужного оборудования.
	def Stats(self):
		print("#############################################")
		print("Room temperature: " + str(self.roomTemp))
		print("Server temperature: " + str(self.servTemp))
		print("Humidity: " + str(self.hum))
		print("CPU: " + str(self.cpu))
		print("Memory: " + str(self.mem))
		print("Storage: " + str(self.storage))
		print("Free ports number: " + str(self.freePorts))
		print("Total ports number: " + str(self.totalPorts))
		print("#############################################\n")

	## "Помощь"
	#  \brief Метод, выводящий пользователю все доступные ему команды.
	def Help(self):
		print("Available commands:\nstartc  -  start Chrome\nclosec  -  close Chrome\nloadf  \
			-  download films\ndelf  -  delete films\nspillw  -  spill water in the server room\ndry \
			-  dry the server room\nconnectd -  connect a device\ndisconnectd  -  disconnect a device\nsh \
			-  show server stats\nburnroom  -  burn the fire in the server room\ncoolroom  -  cool the \
			server room\nburnserv  -  burn the server\ncoolserv  -  cool the server\nh  -  instructions\nesc  -  Exit\n")

	## "Ввод"
	#  \brief Метод, являющийся контроллером для управления командами пользователя.
	#
	#  При вызове данного метода он переходит в состояние ожидания ввода команды от пользователя.
	#  В зависимости от введенной команды вызываются соответствующие функции.
	#  Ниже приведено соответствие вводимой команды и описания вызываемой функции в формате "команда - описание":\n
	#  		- "startc" - повышает нагрузку на процессор и оперативную память серверного оборудования;\n
	#  		- "closec" - снижает нагрузку на процессор и оперативную память серверного оборудования;\n
	#  		- "loadf" - заполняет место в хранилище серверного оборудования;\n
	#  		- "delf" - освобождает место в хранилище серверного оборудования;\n
	#  		- "spillw" - повышает влажность в серверном помещении;\n
	#  		- "dry" - понижает влажность в серверном помещении;\n
	#  		- "connectd" - заполняет свободные порты серверного оборудования;\n	
	#  		- "disconnectd" - освобождает свободные порты серверного оборудования;\n	
	#  		- "burnroom" - повышает температуру серверного помещения;\n	
	#  		- "coolroom" - понижает температуру серверного помещения;\n	
	#  		- "burnserv" - повышает температуру серверного оборудования;\n	
	#  		- "coolserv" - охлаждает температуру серверного оборудования;\n	
	#  		- "sh" - выводит список параметров серверного оборудования;\n	
	#  		- "h" - выводит список доступных пользователю комманд;\n	
	#  		- "esc" - выход из программы.\n
	#  При вводе команды, которой нет в списке выше, пользователю выводится соответствующее сообщение.
	def Inp(self):
		key = input()
		if key == "startc":
			self.StartChrome()
		elif key == "closec":
			self.CloseChrome()
		elif key == "loadf":
			self.DownloadFilms()
		elif key == "delf":
			self.DeleteFilms()
		elif key == "spillw":
			self.SpillWater()
		elif key == "dry":
			self.Dry()
		elif key == "connectd":
			self.ConnectDevice()
		elif key == "disconnectd":
			self.DisconnectDevice()
		elif key == "sh":
			self.Stats()
		elif key == "burnroom":
			self.Burn()
		elif key == "coolroom":
			self.Cool()
		elif key == "burnserv":
			self.HeatServer()
		elif key == "coolserv":
			self.CoolServer()
		elif key == "h":
			self.Help()
		elif key == "esc":
			self.IsWorking = False
			print("Stopping the programm")
		else:
			print("Unknown command")

## "Ченджер"
#  \brief Метод, случайно меняющий различные показания серверов с интервалом в 5 секунд, а также пересылающий эти параметры в анализатор посредством MQTT.
def Changer():
	mqtt_port = 1883												#Порт MQTT
	host = '134.209.224.248'										#Хост MQTT брокера
	topic = ['/serv1', '/serv2', '/serv3', '/serv4', '/serv5',		
			'/serv6', '/serv7', '/serv8', '/serv9', '/serv10']		#Список топиков

	CurrentTime = time.time()

	while True:
		if time.time() - CurrentTime > 0.001:
			CurrentTime = time.time()

			t = [random.randint(0, 9), random.randint(0, 9), 
				random.randint(0, 9), random.randint(0, 9), 
				random.randint(0, 9), random.randint(0, 9), 
				random.randint(0, 9), random.randint(0, 9), 
				random.randint(0, 9), random.randint(0, 9)]			#Случайный набор команд

			os.system('cls')
			print("1# Server")
			serv[0].Stats()
			print("2# Server")
			serv[1].Stats()

			for i in range(2,10):									#Случайный выбор команды
				if t[i] == 0:
					print(str(i+1) + "# Server")
					serv[i].StartChrome()
				elif t[i] == 1:
					print(str(i+1) + "# Server")
					serv[i].CloseChrome()
				elif t[i] == 2:
					print(str(i+1) + "# Server")
					serv[i].SpillWater()
				elif t[i] == 3:
					print(str(i+1) + "# Server")
					serv[i].Dry()
				elif t[i] == 4:
					print(str(i+1) + "# Server")
					serv[i].DownloadFilms()
				elif t[i] == 5:
					print(str(i+1) + "# Server")
					serv[i].DeleteFilms()
				elif t[i] == 6:
					print(str(i+1) + "# Server")
					serv[i].Burn()
				elif t[i] == 7:
					print(str(i+1) + "# Server")
					serv[i].Cool()
				elif t[i] == 8:
					print(str(i+1) + "# Server")
					serv[i].HeatServer()
				elif t[i] == 9:
					print(str(i+1) + "# Server")
					serv[i].CoolServer()

			message = [{}, {}, {}, {}, {}, {}, {}, {}, {}, {}]

			for i in range(0,10):

				message[i] = {"roomTemp": serv[i].roomTemp,
				"servTemp": serv[i].servTemp, "hum": serv[i].hum,
				"cpu": serv[i].cpu, "mem": serv[i].mem,
				"storage": serv[i].storage,
				"free ports": serv[i].freePorts,
				"total ports": serv[i].totalPorts}

				message[i] = json.dumps(message[i])
				mqtt.single(topic[i], message[i],
							 hostname=host, port=mqtt_port) 	#Отправка данных на MQTT брокер

#---------------------------------------------------------------------------Main Program----------------------------------------------------------------------------------------------------------
serv = [Servers(),Servers(),Servers(),Servers(),Servers(),
		Servers(),Servers(),Servers(), Servers(), Servers()]	#Создание объектов серверов

thread1 = Thread(target=Changer, daemon=True)					#Создание отдельного потока для фонового изменения данных серверов
thread1.start()
print("Press h for help")
while (serv[0].IsWorking and serv[1].IsWorking and serv[2].IsWorking
		and serv[3].IsWorking and serv[4].IsWorking and serv[5].IsWorking
		and serv[6].IsWorking and serv[7].IsWorking and serv[8].IsWorking 
		and serv[9].IsWorking):
	print("Input Server name")
	inp = input()
	if inp == "1":
		serv[0].Inp()
	elif inp == "2":
		serv[1].Inp()
	elif inp == "3":
		serv[2].Inp()
	elif inp == "4":
		serv[3].Inp()
	elif inp == "5":
		serv[4].Inp()
	elif inp == "6":
		serv[5].Inp()
	elif inp == "7":
		serv[6].Inp()
	elif inp == "8":
		serv[7].Inp()
	elif inp == "9":
		serv[8].Inp()
	elif inp == "10":
		serv[9].Inp()
	else:
		print("Wrong Server name")