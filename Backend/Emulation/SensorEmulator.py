## \file SensorEmulator.py
#  \brief Производит чтение данных из файла с данными и возвращает их.
#  \authors Стрельцов Г. А.
#  \version 1.0
#  \date 15.05.20
#
#  Данный скрипт предназначен для использование его в системе Zabbix. Он производит чтение данных из заранее созданного фала с данными и возвращает
#  их поочередно. Для эмулирования одного датчика необходимо создать отдельный скрипт, скопировать в него данный код и поменять пути к файлам. 
#  В одном файле должны содержаться данные, разделенные пробелом, в другом запись типа "0 0", где первый аргумент отвечает за счетчик для прохождения
#  по циклу данных, а второй представляет из себя добавочное значение для ручного управления значениями данных, посылаемыми в Zabbix.

import re

# Путь к файлу с конфигурацией
f = open("/home/data/DC_sensors/AirFlw/conf.txt", 'r')
nums = re.split(r' ', f.read())
index = int(nums[0])
boost = int(nums[1])
f.close()

# Путь к файлу с данными
f = open("/home/data/DC_sensors/AirFlw/AirFlw_data.txt", 'r')
values = re.split(r' ', f.read())
val = float(values[index])
f.close()

print(val + boost)

num_rows = len(values)
if(index == num_rows-2):
	index = 0
else:
	index += 1

# Путь к файлу с конфигурацией
f = open("/home/data/DC_sensors/AirFlw/conf.txt", 'w')
st = str(index) + ' ' + str(boost)
f.write(st)
f.close()

