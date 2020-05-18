from pyzabbix.api import ZabbixAPI
import json
import paho.mqtt.publish as mqtt
import re
import datetime
import time
import os

def Init():

	config = open("/Collector/collector.conf", "r")
	server = ""
	login = ""
	password = ""
	brocker = ""
	mqttClient = ""
	mqttPswd = ""
	mqttPort = ""
	mqttTopic = ""
	
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

	if not server or not login or not password or not brocker or not mqttClient or not mqttPswd or not mqttPort or not mqttTopic:
		print("Error: empty parameters in collector.conf")
	else:
		return server, login, password, brocker, mqttClient, mqttPswd, mqttPort, mqttTopic
	
def GetData(triggers, server, login, password):

	# Create ZabbixAPI class instance
	zapi = ZabbixAPI(url=server, user=login, password=password)

	# Get all monitored hosts
	request1 = zapi.host.get(monitored_hosts=1, output='extend')
	hostIds = [i['hostid'] for i in request1]
	request2 = zapi.do_request('item.get',
	                          {
	                            "hostids": hostIds
	                          })
	hosts = []
	time = str(datetime.datetime.now())
	for i in request1:
		items = []
		for j in request2['result']:
			if j['hostid'] == i['hostid']:
				buf = {"itemid": j['itemid'], "itemname": j['name'], "value_type": j['value_type'], 
						"value": j['lastvalue'], "time": time}
				for k in triggers:
					if k['host'] == i['name'] and k['item'] == j['name']:
						buf.update({"trigger": k})
				items.append(buf)
		hosts.append({"hostname": i['name'], "hostid": i['hostid'], "items": items})

	msg = {"result": hosts}
	msg = json.dumps(msg)

	# Logout from Zabbix
	zapi.user.logout()

	return msg

def DefineExp(exp):

	if exp == "<":
		return "lower"
	elif exp == ">":
		return "upper"
	elif exp == "=":
		return "equal"
	else:
		return "unknown"

def GetTriggers():

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
	return allTriggers



if __name__ == '__main__':
	if Init():
		server, login, password, brocker, mqttClient, mqttPswd, mqttPort, mqttTopic = Init()
		mqttPort = int(mqttPort)
		while True:
			msg = GetData(GetTriggers(), server, login, password)
			mqtt.single(mqttTopic, msg, hostname=brocker, port=mqttPort, auth={'username': mqttClient, 'password': mqttPswd}) 	#Отправка данных на MQTT брокер
			time.sleep(1)
