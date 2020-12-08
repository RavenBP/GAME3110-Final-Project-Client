import random
import socket
import time
from _thread import *
import threading
from datetime import datetime
import json

players = {}
players_lock = threading.Lock()

heartbeats = {}
gameState = {}

################################################ Checking Players' Connections

# For waiting for players to enter room
# and to check for connection drops
def ConnectionLoop(sock, playersInMatch):
	gameState['beginRoundRollcall'] = 0

	while True:
		data, addr = sock.recvfrom(1024)
		data = json.loads(data)

		# Who sent it
		userid = data['uid']

		if 'command' in data:
			if data['command'] == 'connect':
	
				if ConfirmPlayerHasConnected(userid, playersInMatch):
					CreatePlayerGameData(addr, userid, sock)

			elif data['command'] == 'heartbeat':
				heartbeats[userid] = datetime.now();

			elif data['command'] == 'gameUpdate':
				# Json format: { 'command': '', 'uid': '', 'orderid': 0, 'state': '', 'letterGuess': '', 'solveGuess': '', 'roundScore': 0, 'cumulativeScore': 0}
				PlayerGameDataUpdate(data, userid, sock)

			# Puzzle solved, start next round
			elif data['command'] == 'roundEnd':
				print("Round End")
				HandleRoundEnd(sock);

			# All rounds finished, go back to lobby
			elif data['command'] == 'matchEnd':
				print(0)

			# If someone leaves match, still treat it as a dropped player
			elif data['command'] == 'quit':
				print(0)

			# Just pass the message on to everyone else
			elif data['command'] == 'loseTurn':
				print("Lose Turn")
				PassTurn(sock, data)

def ConfirmPlayerHasConnected(userid, playersInMatch):

	# Check if the player belongs in this match
	if userid in playersInMatch.keys():
		# Check if player is registered in the match
		if userid not in players:
			return True

	return False

############################################### Match Functions

def CreatePlayerGameData(addr, userid, sock):
	players_lock.acquire()
	gameData = {}

	gameData['uid'] = userid # For client reference
	gameData['addr'] = addr

	gameData['orderid'] = len(players) # Turn order will be time players join match
	gameData['state'] = ''
	gameData['letterGuess'] = ''
	gameData['solveGuess'] = ''
	gameData['roundScore'] = 0
	gameData['cumulativeScore'] = 0
	gameData['wordIndex'] = gameState['currentWord']
	gameData['currentPlayer'] = gameState['currentPlayer']

	heartbeats[userid] = datetime.now()

	players[userid] = gameData
	players_lock.release()

	# Send new player data to other clients
	for player in players.values():
		newPlayerMsg = {}
		newPlayerMsg = player.copy()
		newPlayerMsg['command'] = 'newPlayer'

		for playerAddress in players.values():
			print("Sent")
			print(newPlayerMsg)
			address = playerAddress['addr']
			msg = json.dumps(newPlayerMsg)
		
			sock.sendto(bytes(msg, 'utf8'), address)

	# Signal Player to begin game
	StartGameSignal(sock, addr)

	print("Players in match: ")
	print(players)

def PlayerGameDataUpdate(data, userid, sock):
	players_lock.acquire()

	players[userid]['orderid'] = data['orderid']
	players[userid]['state'] = data['state']
	players[userid]['letterGuess'] = data['letterGuess']
	players[userid]['solveGuess'] = data['solveGuess']
	players[userid]['roundScore'] = data['roundScore']
	players[userid]['cumulativeScore'] = data['cumulativeScore']

	players_lock.release()

	ServerGameStateRelay(sock, userid)

#Pregame setup, basically who begins game, what is the word
#Should be called for each new player and after each round
def StartGameSignal(sock, addr):
	startMsg = {}
	startMsg['command'] = 'startGame'
	startMsg['wordIndex'] = gameState['currentWord']
	startMsg['currentPlayer'] = gameState['currentPlayer']
	msg = json.dumps(startMsg)
		
	sock.sendto(bytes(msg, 'utf8'), addr)

def PassTurn(sock, passTurnMsg):

	for player in players.values():
		passTurnMsg['command'] = 'switchTurn'
		msg = json.dumps(passTurnMsg)

		addr = player['addr']
		sock.sendto(bytes(msg, 'utf8'), addr)

def HandleRoundEnd(sock):
	# Should only accept one round end message from all clients instead of from each

	# Wait for all players to confirm round ended
	gameState['beginRoundRollcall'] += 1
	if gameState['beginRoundRollcall'] >= len(players):
		gameState['currentWord'] = random.randint(0, gameState['remaingWords']-1)

		for player in players.values():
			gameState['currentPlayer'] = random.randint(0, len(players) - 1)
			gameState['remaingWords'] = gameState['remaingWords'] - 1

			StartGameSignal(sock, player['addr'])

		# Reset Roll
		gameState['beginRoundRollcall'] = 0

################################################ Server Messaging

def ServerGameStateRelay(sock, userid):
	#while True:

	for player in players.values():
		#if player['uid'] != userid:
		#gameStateMsg = {'players': []}
		#gameStateMsg['players'].append(player)
		gameStateMsg = player.copy()
		gameStateMsg["command"] = "update"
		gameStateMsg = json.dumps(gameStateMsg)

		for playerAddress in players.values():
			address = playerAddress['addr']
			sock.sendto(bytes(gameStateMsg, 'utf8'), address)

		#print("Sent ")
		#print(gameStateMsg)

################################################ Start Match

def StartMatchLoop(sock):
	print("Match started")

	gameState['currentPlayer'] = 0
	gameState['remaingWords'] = 12

	# Generate random word
	gameState['currentWord'] = random.randint(0, gameState['remaingWords']-1)
	gameState['remaingWords'] = gameState['remaingWords'] - 1

	start_new_thread(ConnectionLoop,(sock,{'0':{}, '1':{}},))
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