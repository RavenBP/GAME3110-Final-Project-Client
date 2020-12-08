using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

public class NetworkMatchLoop : MonoBehaviour
{
    private static NetworkMatchLoop instance;

    public static NetworkMatchLoop Instance { get { return instance; } }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

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
        public GameState state; // Another turn or lose turn
        public string letterGuess;
        public string solveGuess;
        public int roundScore;
        public int cumulativeScore;
    }

    [Serializable]
    public class GameState : Message
    {
        public int wordIndex;
        public int currentPlayer;
    }

    public UdpClient udp;
    public string uid; // TODO: REMOVE AFTER INTEGRATION

    public Message latestMessage;

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
                case "startGame":
                    GameState gameState = JsonUtility.FromJson<GameState>(returnData);
                    GameManager.Instance.wordIndex = gameState.wordIndex;

                    break;

                case "newPlayer":
                    playersToAdd.Enqueue(JsonUtility.FromJson<Player>(returnData));
                    break;

                case "update":
                    Player playerUpdate = JsonUtility.FromJson<Player>(returnData);

                    // Make sure not my own update
                    if (playerUpdate.uid != uid && GameManager.Instance.players.ContainsKey(playerUpdate.orderid))
                    {
                        PlayerBehaviour player = GameManager.Instance.players[playerUpdate.orderid];

                        player.roundScore = playerUpdate.roundScore;
                        player.cumulativeScore = playerUpdate.cumulativeScore;
                        
                        if (playerUpdate.letterGuess.Length > 0 && playerUpdate.letterGuess[0] != GameManager.Instance.otherPlayerGuess)
                        {
                            GameManager.Instance.otherPlayerGuess = playerUpdate.letterGuess[0];
                            GameManager.Instance.otherPlayerGuessing = true;
                        }
                        else if (playerUpdate.solveGuess.Length > 0 && playerUpdate.solveGuess != GameManager.Instance.otherPlayerSolution)
                        {
                            GameManager.Instance.otherPlayerSolution = playerUpdate.solveGuess;
                            GameManager.Instance.otherPlayerSolving = true;
                        }
                    }

                    break;

                case "switchTurn":
                    gameState = JsonUtility.FromJson<GameState>(returnData);
                    GameManager.Instance.currentPlayer = gameState.currentPlayer;
                    GameManager.Instance.TakeTurn();

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
        gameMsg.letterGuess = ui.guessChar.ToString(); // Player's letter guess
        gameMsg.solveGuess = ui.guessSolve; // Player's solve guess

        SendMessage(gameMsg);
    }

    public void SendLoseTurnMessage(int currentPlayer)
    {
        GameState gameStateMsg = new GameState();

        gameStateMsg.command = "loseTurn";
        gameStateMsg.currentPlayer = currentPlayer;

        SendMessage(gameStateMsg);
    }

    public void SendRoundEndMessage()
    {
        GameState gameStateMsg = new GameState();

        gameStateMsg.command = "roundEnd";

        SendMessage(gameStateMsg);
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
