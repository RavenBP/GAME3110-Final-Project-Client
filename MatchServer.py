import socket
import time
from _thread import *
import threading
from datetime import datetime
import json

players = {}
players_lock = threading.Lock()

################################################ Checking Players' Connections

# For waiting for players to enter room
# and to check for connection drops
def ConnectionLoop(sock, playersInMatch):
	while True:
		data, addr = sock.recvfrom(1024)
		data = json.loads(data)

		# Who sent it
		userid = data['uid']

		if 'command' in data:
			if data['command'] == 'connect':
	
				if ConfirmPlayerHasConnected(userid, playersInMatch):
					CreatePlayerGameData(addr, userid)

			elif data['command'] == 'heartbeat':
				players[userid]['lastHeartBeat'] = datetime.now();

			else:
				print(data)

def ConfirmPlayerHasConnected(userid, playersInMatch):

	# Check if the player belongs in this match
	if userid in playersInMatch.keys():
		# Check if player is registered in the match
		if userid not in players:
			return True

	return False

def CreatePlayerGameData(addr, userid):
	players_lock.acquire()
	gameData = {}

	gameData['userid'] = userid # For client reference
	gameData['addr'] = addr
	gameData['score'] = 0
	gameData['turnOrder'] = len(players) # Turn order will be time players join match
	gameData['lastHeartBeat'] = datetime.now()

	players[userid] = gameData
	players_lock.release()

	print("Players in match: ")
	print(players)

################################################ Server Messaging

def ServerGameStateRelay(sock):
	while True:

		for player in players.values():
			playerData = player.copy()
			playerData.pop('lastHeartBeat'); # datetime is not serializable, so removing it from copy

			gameStateMsg = {'players': []}
			gameStateMsg['players'].append(playerData)
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