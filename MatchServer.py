import socket
import time
from _thread import *
import threading
from datetime import datetime
import json

players = {}
players_lock = threading.Lock()

heartbeats = {}

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
				heartbeats[userid] = datetime.now();

			elif data['command'] == 'gameUpdate':
				# Json format: { 'command': '', 'uid': '', 'orderid': 0, 'state': '', 'letterGuess': '', 'solveGuess': '', 'score': 0}
				PlayerGameDataUpdate(data, userid, sock)

def ConfirmPlayerHasConnected(userid, playersInMatch):

	# Check if the player belongs in this match
	if userid in playersInMatch.keys():
		# Check if player is registered in the match
		if userid not in players:
			return True

	return False

############################################### Match Functions

def CreatePlayerGameData(addr, userid):
	players_lock.acquire()
	gameData = {}

	gameData['uid'] = userid # For client reference
	gameData['addr'] = addr

	gameData['score'] = 0
	gameData['orderid'] = len(players) # Turn order will be time players join match
	gameData['state'] = ''
	gameData['letterGuess'] = ''
	gameData['solveGuess'] = ''

	heartbeats[userid] = datetime.now()

	players[userid] = gameData
	players_lock.release()

	print("Players in match: ")
	print(players)

def PlayerGameDataUpdate(data, userid, sock):
	players_lock.acquire()

	players[userid]['score'] = data['score']
	players[userid]['orderid'] = data['orderid']
	players[userid]['state'] = data['state']
	players[userid]['letterGuess'] = data['letterGuess']
	players[userid]['solveGuess'] = data['solveGuess']

	players_lock.release()

	ServerGameStateRelay(sock, userid)

################################################ Server Messaging

def ServerGameStateRelay(sock, userid):
	#while True:

	for player in players.values():
		#if player['uid'] != userid:
		gameStateMsg = {'players': []}
		gameStateMsg['players'].append(player)
		gameStateMsg["command"] = "update"
		gameStateMsg = json.dumps(gameStateMsg)

		address = player['addr']
		sock.sendto(bytes(gameStateMsg, 'utf8'), address)

		print("Sent ")
		print(gameStateMsg)

################################################ Start Match

def StartMatchLoop(sock):
	print("Match started")

	start_new_thread(ConnectionLoop,(sock,{'0':{}},))
	#start_new_thread(ServerGameStateRelay,(sock,))

################################################ Test Code

def main():
	sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
	sock.bind(('', 12345))

	StartMatchLoop(sock)

	while True:
		time.sleep(1/30)

if __name__ == '__main__':
   main()