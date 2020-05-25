## \file data_generator.py
#  \brief Генерирует различные данные с помощью бета-распределения.
#  \authors Стрельцов Г. А.
#  \version 1.0
#  \date 15.05.20
#
#  В качестве входных параметров данного скрипта используются нормальное значение генерируемого параметра, 3 вида отклонений по модулю, 
#  включая максимальное, а также путь к файлу, куда будут записаны сгенерированные значения. Вся программа состоит из цикла, в начале которого
#  с помощью модуля random выбирается действие (увеличить значение параметра или уменьшить). После этого с помощью функции бета-распределения модуля
#  random вычисляется вероятность увеличения или уменьшения на конкретную величину, которая тоже генерируется с помощью модуля random. В результате
#  программа генерирует набор различных значений, которые могут колебаться в пределах заданных величин, что позволяет эмулировать примерные показания
#  для различных датчиков.

import random
import math

#Нормальное значение
norm = 20
alpha = [5, 45, 80, 100000]
beta = [5, 45, 80, 100000]
data = norm
#Отклонения по модулю
deviation = [1, 3, 5]
dataList = ""
#Путь к файлу записи
path = "/home/data/DC_sensors/RoomT9/RoomT_data.txt"

for i in range(1000):
	action = random.randint(0, 100)
	add = random.random()
	# ADDITION
	# Norm > 5
	if (action >= 50 and data > norm + deviation[2] and add > 0.8):
		a = random.betavariate(alpha[0], beta[3])
		b = random.random()
		if (b < a):
			data = data + add
	elif (action >= 50 and data > norm + deviation[2] and 0.8 > add > 0.2):
		a = random.betavariate(alpha[0], beta[3])
		b = random.random()
		if (b < a):
			data = data + add
	elif (action >= 50 and data > norm + deviation[2] and add < 0.2):
		a = random.betavariate(alpha[0], beta[3])
		b = random.random()
		if (b < a):
			data = data + add
	# 3 < Norm < 5
	elif (action >= 50 and norm + deviation[2] > data > norm + deviation[1]  and add > 0.8):
		a = random.betavariate(alpha[0], beta[3])
		b = random.random()
		if (b < a):
			data = data + add
	elif (action >= 50 and norm + deviation[2] > data > norm + deviation[1] and 0.8 > add > 0.2):
		a = random.betavariate(alpha[1], beta[3])
		b = random.random()
		if (b < a):
			data = data + add
	elif (action >= 50 and norm + deviation[2] > data > norm + deviation[1] and add < 0.2):
		a = random.betavariate(alpha[1], beta[3])
		b = random.random()
		if (b < a):
			data = data + add
	# 1 < Norm < 3
	elif (action >= 50 and norm + deviation[1] > data > norm + deviation[0]  and add > 0.8):
		a = random.betavariate(alpha[2], beta[2])
		b = random.random()
		if (b < a):
			data = data + add
	elif (action >= 50 and norm + deviation[1] > data > norm + deviation[0] and 0.8 > add > 0.2):
		a = random.betavariate(alpha[2], beta[1])
		b = random.random()
		if (b < a):
			data = data + add
	elif (action >= 50 and norm + deviation[1] > data > norm + deviation[0] and add < 0.2):
		a = random.betavariate(alpha[2], beta[0])
		b = random.random()
		if (b < a):
			data = data + add
	# -1 < Norm < 1
	elif (action >= 50 and norm + deviation[0] > data > norm - deviation[0]  and add > 0.8):
		a = random.betavariate(alpha[2], beta[1])
		b = random.random()
		if (b < a):
			data = data + add
	elif (action >= 50 and norm + deviation[0] > data > norm - deviation[0] and 0.8 > add > 0.2):
		a = random.betavariate(alpha[2], beta[1])
		b = random.random()
		if (b < a):
			data = data + add
	elif (action >= 50 and norm + deviation[0] > data > norm - deviation[0] and add < 0.2):
		a = random.betavariate(alpha[2], beta[0])
		b = random.random()
		if (b < a):
			data = data + add
	# -3 < Norm < -1
	elif (action >= 50 and norm - deviation[0] > data > norm - deviation[1]  and add > 0.8):
		a = random.betavariate(alpha[2], beta[0])
		b = random.random()
		if (b < a):
			data = data + add
	elif (action >= 50 and norm + deviation[0] > data > norm - deviation[0] and 0.8 > add > 0.2):
		a = random.betavariate(alpha[2], beta[0])
		b = random.random()
		if (b < a):
			data = data + add
	elif (action >= 50 and norm + deviation[0] > data > norm - deviation[0] and add < 0.2):
		a = random.betavariate(alpha[2], beta[0])
		b = random.random()
		if (b < a):
			data = data + add
	# Norm < -3
	elif (action >= 50 and norm - deviation[1] > data and add > 0.8):
		a = random.betavariate(alpha[3], beta[0])
		b = random.random()
		if (b < a):
			data = data + add
	elif (action >= 50 and norm - deviation[1] > data and 0.8 > add > 0.2):
		a = random.betavariate(alpha[3], beta[0])
		b = random.random()
		if (b < a):
			data = data + add
	elif (action >= 50 and norm - deviation[1] > data and add < 0.2):
		a = random.betavariate(alpha[3], beta[0])
		b = random.random()
		if (b < a):
			data = data + add
	# SUBSTRACTION
	# Norm < -5
	if (action < 50 and data < norm - deviation[2] and add > 0.8):
		a = random.betavariate(alpha[0], beta[3])
		b = random.random()
		if (b < a):
			data = data - add
	elif (action < 50 and data < norm - deviation[2] and 0.8 > add > 0.2):
		a = random.betavariate(alpha[0], beta[3])
		b = random.random()
		if (b < a):
			data = data - add
	elif (action < 50 and data < norm - deviation[2] and add < 0.2):
		a = random.betavariate(alpha[0], beta[3])
		b = random.random()
		if (b < a):
			data = data - add
	# -5 < Norm < -3
	elif (action < 50 and norm - deviation[1] > data > norm - deviation[2]  and add > 0.8):
		a = random.betavariate(alpha[0], beta[3])
		b = random.random()
		if (b < a):
			data = data - add
	elif (action < 50 and norm - deviation[1] > data > norm - deviation[2] and 0.8 > add > 0.2):
		a = random.betavariate(alpha[1], beta[3])
		b = random.random()
		if (b < a):
			data = data - add
	elif (action < 50 and norm - deviation[1] > data > norm - deviation[2] and add < 0.2):
		a = random.betavariate(alpha[1], beta[3])
		b = random.random()
		if (b < a):
			data = data - add
	# -3 < Norm < -1
	elif (action < 50 and norm - deviation[0] > data > norm - deviation[1]  and add > 0.8):
		a = random.betavariate(alpha[2], beta[2])
		b = random.random()
		if (b < a):
			data = data - add
	elif (action < 50 and norm - deviation[0] > data > norm - deviation[1] and 0.8 > add > 0.2):
		a = random.betavariate(alpha[2], beta[1])
		b = random.random()
		if (b < a):
			data = data - add
	elif (action < 50 and norm - deviation[0] > data > norm - deviation[1] and add < 0.2):
		a = random.betavariate(alpha[2], beta[0])
		b = random.random()
		if (b < a):
			data = data - add
	# -1 < Norm < 1
	elif (action < 50 and norm + deviation[0] > data > norm - deviation[0]  and add > 0.8):
		a = random.betavariate(alpha[2], beta[1])
		b = random.random()
		if (b < a):
			data = data - add
	elif (action < 50 and norm + deviation[0] > data > norm - deviation[0] and 0.8 > add > 0.2):
		a = random.betavariate(alpha[2], beta[1])
		b = random.random()
		if (b < a):
			data = data - add
	elif (action < 50 and norm + deviation[0] > data > norm - deviation[0] and add < 0.2):
		a = random.betavariate(alpha[2], beta[0])
		b = random.random()
		if (b < a):
			data = data - add
	# 1 < Norm < 3
	elif (action < 50 and norm + deviation[1] > data > norm + deviation[0]  and add > 0.8):
		a = random.betavariate(alpha[2], beta[0])
		b = random.random()
		if (b < a):
			data = data - add
	elif (action < 50 and norm + deviation[1] > data > norm + deviation[0] and 0.8 > add > 0.2):
		a = random.betavariate(alpha[2], beta[0])
		b = random.random()
		if (b < a):
			data = data - add
	elif (action < 50 and norm + deviation[1] > data > norm + deviation[0] and add < 0.2):
		a = random.betavariate(alpha[2], beta[0])
		b = random.random()
		if (b < a):
			data = data - add
	# Norm > 3
	elif (action >= 50 and data > norm + deviation[1] and add > 0.8):
		a = random.betavariate(alpha[3], beta[0])
		b = random.random()
		if (b < a):
			data = data - add
	elif (action >= 50 and data > norm + deviation[1] and 0.8 > add > 0.2):
		a = random.betavariate(alpha[3], beta[0])
		b = random.random()
		if (b < a):
			data = data - add
	elif (action >= 50 and data > norm + deviation[1] and add < 0.2):
		a = random.betavariate(alpha[3], beta[0])
		b = random.random()
		if (b < a):
			data = data - add
	strData = str(data)
	dataList = dataList + strData + ' '

f = open(path, 'w')
f.write(dataList)
f.close()