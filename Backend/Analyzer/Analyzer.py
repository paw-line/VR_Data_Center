import re
import paho.mqtt.client as cl
import json
import paho.mqtt.publish as mqtt

def Init():

	config = open("/Analyzer/analyzer.conf", "r")
	brocker = ""
	mqttClient = ""
	mqttPswd = ""
	mqttPort = ""
	mqttTopicGet = ""
	mqttTopicSend = ""
	
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
			elif normData[0] == "MQTT_Topic_Get":
				mqttTopicGet = normData[1]	
			elif normData[0] == "MQTT_Topic_Send":
				mqttTopicSend = normData[1]	
	config.close()

	if not brocker or not mqttClient or not mqttPswd or not mqttPort or not mqttTopicGet or not mqttTopicSend:
		print("Error: empty parameters in collector.conf")
	else:
		mqttPort = int(mqttPort)
		data = {"host": brocker, "client": mqttClient, "password": mqttPswd, "port": mqttPort, 
				"topicGet": mqttTopicGet, "topicSend": mqttTopicSend}
		return data

def Analizator(data):

	for i in data:
		for j in i["items"]:
			if "trigger" in j:
				trigger = j["trigger"]
				if "lower" in trigger:
					if float(j["value"]) < float(trigger["lower"]):
						trigger.update({"value": 1})
				if "upper" in trigger:
					if float(j["value"]) > float(trigger["upper"]):
						trigger.update({"value": 1})
				if "equal" in trigger:
					if j["value"] == trigger["equal"]:
						trigger.update({"value": 1})

	return data

def Publisher(msg, userdata):

	normMsg = {"result": msg}
	normMsg = json.dumps(normMsg)
	mqtt.single(userdata["topicSend"], normMsg, hostname=userdata["host"], 
				port=userdata["port"], auth={'username': userdata["client"], 
				'password': userdata["password"]})

def on_connect(client, userdata, flags, rc):

    res = client.subscribe(userdata["topicGet"])

def on_message(client, userdata, msg):

    message = json.loads(msg.payload)
    data = message["result"]
    sendMsg = Analizator(data)
    Publisher(sendMsg, userdata)

def MQTTConnection(userData):
	
	client = cl.Client()
	client.username_pw_set(username=userData["client"], password=userData["password"])
	client.user_data_set(userData)
	client.on_connect = on_connect
	client.on_message = on_message
	client.connect(userData["host"], userData["port"])
	client.loop_forever()

if __name__ == '__main__':
	if Init():
		userData = Init()
		MQTTConnection(userData)