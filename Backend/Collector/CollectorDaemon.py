#!/usr/bin/env python

## \file CollectorDaemon.py
#  \brief Создает процесс - демон, использующий функции Collector.py.
#  \authors Стрельцов Г. А.
#  \version 1.0
#  \date 15.05.20
#
#  Данный скрипт предназначен для создания процесса - демона, который будет выполнять функции скрипта Collector.py.
#  На вход получет пользовательские параметры из вводимой команты: \n
#  - "start" - запуск демона; \n
#  - "stop" - остановка демона; \n
#  - "restart" - перезапуск демона. \n
#  Если не было введено никаких параметров, пользователь будет уведовлен соответствующим сообщением.
#  Поддерживает логгирование. Запись логов происзодит в файл collector.log. Также создает pid-файл collector.pid.

from daemons.prefab import run
import paho.mqtt.publish as mqtt
import Collector
import logging
import os
import sys
import time

## Класс демонизации ColDaemon.
#  \brief Содержит в себе основное тело выполняемой программы.
#   
#  Предназанчен для создания процесса - демона. Наследуется от RunDaemon модуля daemons.
#  Содержит в себе основной алгоритм выполения программы в методе run().
class ColDaemon(run.RunDaemon):

    ## Метод run.
    #  \brief Содержит в себе основное тело выполняемой программы.
    def run(self):

        #  Если инициализация прошла успешно
        if Collector.Init():
            #  Чтение параметров
            server, login, password, brocker, mqttClient, mqttPswd, mqttPort, mqttTopic = Collector.Init()
            mqttPort = int(mqttPort)
            while True:
                #  Получение сформированного сообщения
                msg = Collector.GetData(Collector.GetTriggers(), server, login, password)
                #  Отправка сообщения по указанному топику
                mqtt.single(mqttTopic, msg, hostname=brocker, port=mqttPort, auth={'username': mqttClient, 'password': mqttPswd})   #Отправка данных на MQTT брокер
                time.sleep(1)

#  Основная программа
if __name__ == '__main__':

    #  Если в команде было несколько аргументов
    if len(sys.argv) == 2:
        action = sys.argv[1]
        #  Создание логов
        logfile = os.path.join(os.getcwd(), "collector.log")
        #  Создание pid-файла
        pidfile = os.path.join(os.getcwd(), "collector.pid")

        #  Подключение логов
        logging.basicConfig(filename=logfile, level=logging.INFO)
        d = ColDaemon(pidfile=pidfile)

        #  Выполнение команд, в зависимости от параметра
        if action == "start":

            d.start()

        elif action == "stop":

            d.stop()

        elif action == "restart":

            d.restart()
    else:
        #  Вывод ошибки количества параметров
        print("Please, use start/stop/restart commands")