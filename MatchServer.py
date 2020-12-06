import socket
import time
from _thread import *
import threading
from datetime import datetime
import json

players = {}

# For waiting for players to enter room
# and to check for connection drops
def ConnectionLoop(sock, playersInMatch):
	while True:
		data, addr = sock.recvfrom(1024)
		data = json.loads(data)

		if 'command' in data:
			if data['command'] == 'connect':
				ConfirmPlayerHasConnected(data, playersInMatch)

			if data['command'] == 'heartbeat':
				print(data)

def ConfirmPlayerHasConnected(data, playersInMatch):
	userid = data['uid']

	# Check if the player belongs in this match
	if userid in playersInMatch:
		# Check if player is registered in the match
		if userid not in players:
			# Set the player game data here
			CreatePlayerGameData(userid)
			print("Players in match: ")
			print(players)

def CreatePlayerGameData(userid):
	gameData = {}
	players[userid] = gameData
	gameData['score'] = 0

def StartMatchLoop(sock):
	print("Match started")

	start_new_thread(ConnectionLoop,(sock,{'0':{}},))

def main():
	sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
	sock.bind(('', 12345))

	StartMatchLoop(sock)

	while True:
		time.sleep(1/30)

if __name__ == '__main__':
   main()