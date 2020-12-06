using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

public class NetworkMatchLoop : MonoBehaviour
{
    [Serializable]
    public class Message
    {
        public string command; // What you want the server to do
        public string uid; // Who is sending the message, identified by user id
    }

    [Serializable]
    public class Player : Message
    {
        public int orderid; // Turn order
        public string state; // Another turn or lose turn
        public string letterGuess;
        public string solveGuess;
        public string userid;
        public int score;
    }

    [Serializable]
    public class GameState : Message
    {
        public Player[] players;
    }

    public UdpClient udp;
    public string uid; // TODO: REMOVE AFTER INTEGRATION

    public Message latestMessage;
    public GameState gameState;

    public PlayerBehaviour player;
    public UI ui;

    // TODO: REMOVE AFTER INTEGRATION
    private void Start()
    {
        StartMatchConnection(12345);
    }

    // Start connection to match socket
    public void StartMatchConnection(int matchPort)
    {
        udp = new UdpClient();

        //udp.Connect("3.130.200.122", matchPort);
        udp.Connect("localhost", matchPort);

        Message connectionMsg = new Message();
        connectionMsg.command = "connect";
        connectionMsg.uid = uid;

        SendMessage(connectionMsg);

        udp.BeginReceive(new AsyncCallback(OnReceived), udp);

        InvokeRepeating("HeartBeat", 1, 0.03f); // Every 0.03 seconds run heartbeat
    }

    void OnReceived(IAsyncResult result)
    {
        // this is what had been passed into BeginReceive as the second parameter:
        UdpClient socket = result.AsyncState as UdpClient;

        // points towards whoever had sent the message:
        IPEndPoint source = new IPEndPoint(0, 0);

        // get the actual message and fill out the source:
        byte[] message = socket.EndReceive(result, ref source);

        // do what you'd like with `message` here:
        string returnData = Encoding.ASCII.GetString(message); // Json string; m (json dump) that was sent by server
        Debug.Log("Got this: " + returnData);

        latestMessage = JsonUtility.FromJson<Message>(returnData);
        try
        {
            switch (latestMessage.command)
            {
                case "update":
                    gameState = JsonUtility.FromJson<GameState>(returnData);
                    Debug.Log(gameState.players[0].score);
                    break;

                default:
                    Debug.Log("Error");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }

        // schedule the next receive operation once reading is done:
        socket.BeginReceive(new AsyncCallback(OnReceived), socket);
    }

    void SendMessage(Message message)
    {
        Byte[] sendBytes = Encoding.ASCII.GetBytes(JsonUtility.ToJson(message));
        udp.Send(sendBytes, sendBytes.Length);
    }

    void HeartBeat()
    {
        Message heartMsg = new Message();
        heartMsg.command = "heartbeat";
        heartMsg.uid = uid;

        SendMessage(heartMsg);
    }

    void SendGameUpdate()
    {
        Player gameMsg = new Player();

        gameMsg.command = "gameUpdate";
        gameMsg.score = player.cumulativeScore + player.roundScore; // Players score according to the UI label
        gameMsg.orderid = player.id; // ID inside the game, not profile id
        gameMsg.state = GameManager.Instance.state; 
        gameMsg.userid = uid; // User ID, the client that sent this message
        gameMsg.letterGuess = ui.guessChar.ToString(); // Player's letter guess
        gameMsg.solveGuess = ui.guessSolve; // Player's solve guess

        SendMessage(gameMsg);
    }

    // Update is called once per frame
    void Update()
    {
        HeartBeat();
        SendGameUpdate();
    }
}
