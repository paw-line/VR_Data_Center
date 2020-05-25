#!/usr/bin/env python

## \file AnalyzerDaemon.py
#  \brief Создает процесс - демон, использующий функции Analyzer.py.
#  \authors Стрельцов Г. А.
#  \version 1.0
#  \date 15.05.20
#
#  Данный скрипт предназначен для создания процесса - демона, который будет выполнять функции скрипта Analyzer.py.
#  На вход получет пользовательские параметры из вводимой команты: \n
#  - "start" - запуск демона; \n
#  - "stop" - остановка демона; \n
#  - "restart" - перезапуск демона. \n
#  Если не было введено никаких параметров, пользователь будет уведовлен соответствующим сообщением.
#  Поддерживает логгирование. Запись логов происзодит в файл analyzer.log. Также создает pid-файл analyzer.pid.

from daemons.prefab import run
import paho.mqtt.publish as mqtt
import Analyzer
import logging
import os
import sys
import time

## Класс демонизации AnalyzeDaemon.
#  \brief Содержит в себе основное тело выполняемой программы.
#   
#  Предназанчен для создания процесса - демона. Наследуется от RunDaemon модуля daemons.
#  Содержит в себе основной алгоритм выполения программы в методе run().
class AnalyzeDaemon(run.RunDaemon):

    ## Метод run.
    #  \brief Содержит в себе основное тело выполняемой программы.
    def run(self):

        #  Если инициализация прошла успешно
        if Analyzer.Init():
            #  Чтение параметров
            userData = Analyzer.Init()
            # Подключение по MQTT
            Analyzer.MQTTConnection(userData)

#  Основная программа
if __name__ == '__main__':

    #  Если в команде было несколько аргументов
    if len(sys.argv) == 2:
        action = sys.argv[1]
        #  Создание логов
        logfile = os.path.join(os.getcwd(), "analyzer.log")
        #  Создание pid-файла
        pidfile = os.path.join(os.getcwd(), "analyzer.pid")

        #  Подключение логов
        logging.basicConfig(filename=logfile, level=logging.DEBUG)
        d = AnalyzeDaemon(pidfile=pidfile)

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