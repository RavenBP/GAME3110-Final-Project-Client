import socket
import time
from _thread import *
import threading
from datetime import datetime
import json

players = {}

################################################ Checking Players' Connections

# For waiting for players to enter room
# and to check for connection drops
def ConnectionLoop(sock, playersInMatch):
	while True:
		data, addr = sock.recvfrom(1024)
		data = json.loads(data)

		if 'command' in data:
			if data['command'] == 'connect':
				userid = data['uid']

				if ConfirmPlayerHasConnected(userid, playersInMatch):
					CreatePlayerGameData(addr, userid)

			if data['command'] == 'heartbeat':
				print(data)

def ConfirmPlayerHasConnected(userid, playersInMatch):

	# Check if the player belongs in this match
	if userid in playersInMatch.keys():
		# Check if player is registered in the match
		if userid not in players:
			return True

	return False

def CreatePlayerGameData(addr, userid):
	gameData = {}
	players[userid] = gameData
	gameData['userid'] = userid # For client reference
	gameData['addr'] = addr
	gameData['score'] = 99

	print("Players in match: ")
	print(players)

################################################ Server Messaging

def ServerGameStateRelay(sock):
	while True:

		for player in players.values():

			gameStateMsg = {'players': []}
			gameStateMsg['players'].append(player)
			gameStateMsg["command"] = "update"
			gameStateMsg = json.dumps(gameStateMsg)

			address = player['addr']
			sock.sendto(bytes(gameStateMsg, 'utf8'), address)

################################################ Start Match

def StartMatchLoop(sock):
	print("Match started")

	start_new_thread(ConnectionLoop,(sock,{'0':{}},))
	start_new_thread(ServerGameStateRelay,(sock,))

################################################ Test Code

def main():
	sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
	sock.bind(('', 12345))

	StartMatchLoop(sock)

	while True:
		time.sleep(1/30)

if __name__ == '__main__':
   main()