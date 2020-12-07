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
        public int roundScore;
        public int cumulativeScore;
        public int currentPlayer;
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

    public UI ui;

    private Queue<Player> playersToAdd; // Temporary variable to add to game

    // TODO: REMOVE AFTER INTEGRATION
    private void Start()
    {
        StartMatchConnection(12345);
    }

    // Start connection to match socket
    public void StartMatchConnection(int matchPort)
    {
        playersToAdd = new Queue<Player>();

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
                case "newPlayer":
                    playersToAdd.Enqueue(JsonUtility.FromJson<Player>(returnData));
                    //Debug.Log("New Player: " + playerToAdd.uid);
                    break;

                case "update":
                    Player playerUpdate = JsonUtility.FromJson<Player>(returnData);

                    // Make sure not my own update
                    if (playerUpdate.uid != uid && GameManager.Instance.players.ContainsKey(playerUpdate.orderid))
                    {
                        PlayerBehaviour player = GameManager.Instance.players[playerUpdate.orderid];

                        player.roundScore = playerUpdate.roundScore;
                        player.cumulativeScore = playerUpdate.cumulativeScore;
                    }

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
        if (GameManager.Instance.clientPlayer == null)
        {
            return;
        }

        Player gameMsg = new Player();

        gameMsg.uid = uid;
        gameMsg.command = "gameUpdate";

        gameMsg.roundScore = GameManager.Instance.clientPlayer.roundScore;
        gameMsg.cumulativeScore = GameManager.Instance.clientPlayer.cumulativeScore; // Players score according to the UI label
        gameMsg.orderid = GameManager.Instance.clientPlayer.id; // ID inside the game, not profile id
        gameMsg.state = GameManager.Instance.state; 
        gameMsg.letterGuess = ui.guessChar.ToString(); // Player's letter guess
        gameMsg.solveGuess = ui.guessSolve; // Player's solve guess
        gameMsg.currentPlayer = GameManager.Instance.currentPlayer; // Who has turn

        SendMessage(gameMsg);
    }

    // Adding this because I can't instantiate Prefabs outside of the main thread (i.e. Update(), Awake(), Start())
    void AddPlayer()
    {
        if (playersToAdd.Count > 0)
        {
            GameManager.Instance.AddPlayerToGame(playersToAdd.Dequeue(), uid);
        }
    }

    // Update is called once per frame
    void Update()
    {
        AddPlayer();
        HeartBeat();
        SendGameUpdate();
    }
}
