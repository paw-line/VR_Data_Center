#!/usr/bin/env python
from daemons.prefab import run
import paho.mqtt.publish as mqtt
import Analyzer
import logging
import os
import sys
import time

class AnalyzeDaemon(run.RunDaemon):

    def run(self):

        if Analyzer.Init():
            userData = Analyzer.Init()
            Analyzer.MQTTConnection(userData)

if __name__ == '__main__':

    if len(sys.argv) == 2:
        action = sys.argv[1]
        logfile = os.path.join(os.getcwd(), "analyzer.log")
        pidfile = os.path.join(os.getcwd(), "analyzer.pid")

        logging.basicConfig(filename=logfile, level=logging.DEBUG)
        d = AnalyzeDaemon(pidfile=pidfile)

        if action == "start":

            d.start()

        elif action == "stop":

            d.stop()

        elif action == "restart":

            d.restart()
    else:
        print("Please, use start/stop/restart commands")