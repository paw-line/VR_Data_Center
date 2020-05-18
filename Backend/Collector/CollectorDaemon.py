#!/usr/bin/env python
from daemons.prefab import run
import paho.mqtt.publish as mqtt
import Collector
import logging
import os
import sys
import time

class ColDaemon(run.RunDaemon):

    def run(self):

        if Collector.Init():
            server, login, password, brocker, mqttClient, mqttPswd, mqttPort, mqttTopic = Collector.Init()
            mqttPort = int(mqttPort)
            while True:
                msg = Collector.GetData(Collector.GetTriggers(), server, login, password)
                mqtt.single(mqttTopic, msg, hostname=brocker, port=mqttPort, auth={'username': mqttClient, 'password': mqttPswd})   #Отправка данных на MQTT брокер
                time.sleep(1)

if __name__ == '__main__':

    if len(sys.argv) == 2:
        action = sys.argv[1]
        logfile = os.path.join(os.getcwd(), "collector.log")
        pidfile = os.path.join(os.getcwd(), "collector.pid")

        logging.basicConfig(filename=logfile, level=logging.INFO)
        d = ColDaemon(pidfile=pidfile)

        if action == "start":

            d.start()

        elif action == "stop":

            d.stop()

        elif action == "restart":

            d.restart()
    else:
        print("Please, use start/stop/restart commands")