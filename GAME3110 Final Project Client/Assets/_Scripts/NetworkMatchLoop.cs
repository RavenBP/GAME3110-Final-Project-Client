using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        public int state; // Another turn or lose turn
        public string letterGuess;
        public string solveGuess;
        public int spinPoints;
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
    UdpClient socket;

    public string uid;

    public Message latestMessage;

    public UI ui;
    public GamePhase currentPhase = GamePhase.SELECT;
    public int matchPort;

    private Queue<Player> playersToAdd; // Temporary variable to add to game

    private void Start()
    {
        uid = PlayerInfo.username;
        //StartMatchConnection("localhost", matchPort);
    }

    // Start connection to match socket
    public void StartMatchConnection(string ip, int matchPort)
    {
        playersToAdd = new Queue<Player>();

        udp = new UdpClient();

        //udp.Connect("3.130.200.122", matchPort);
        udp.Connect(ip, matchPort);

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
        socket = result.AsyncState as UdpClient;

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
                    GameManager.Instance.connected = true;
                    GameState gameState = JsonUtility.FromJson<GameState>(returnData);
                    GameManager.Instance.wordIndex = gameState.wordIndex;
                    GameManager.Instance.hasRoundEnded = false;
                    ui.guessChar = '\0';
                    ui.guessSolve = "";
                    GameManager.Instance.currentPlayer = gameState.currentPlayer;

                    SendGameUpdate(); // Start game confirmation plus message refresh

                    break;

                case "newPlayer":
                    playersToAdd.Enqueue(JsonUtility.FromJson<Player>(returnData));
                    break;

                case "update":
                    Player playerUpdate = JsonUtility.FromJson<Player>(returnData);
                    ProcessUpdateMessage(playerUpdate);

                    break;

                case "switchTurn":
                    gameState = JsonUtility.FromJson<GameState>(returnData);
                    GameManager.Instance.gamePhaseManager.SetPhase(GamePhase.SELECT);
                    GameManager.Instance.currentPlayer = gameState.currentPlayer;
                    GameManager.Instance.TakeTurn();

                    break;

                case "playerDropped":
                    playerUpdate = JsonUtility.FromJson<Player>(returnData);

                    GameManager.Instance.playerToRemove = playerUpdate.orderid;
                    GameManager.Instance.removeAPlayer = true;
                    break;

                case "matchOver":
                    GameManager.Instance.gameOver = true;
                    
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

    public void CloseConnection()
    {
        socket.Close();
    }

    void ProcessUpdateMessage(Player playerUpdate)
    {
        // Make sure not my own update
        if (playerUpdate.uid != uid && GameManager.Instance.players.Count > playerUpdate.orderid)
        {
            PlayerBehaviour player = GameManager.Instance.players[playerUpdate.orderid];

            player.roundScore = playerUpdate.roundScore;
            player.cumulativeScore = playerUpdate.cumulativeScore;

            if (playerUpdate.letterGuess.Length > 0 && GameManager.Instance.currentPlayer == player.id)
            {
                GameManager.Instance.otherPlayerGuess = playerUpdate.letterGuess[0];
                GameManager.Instance.otherPlayerGuessing = true;
            }
            else if (playerUpdate.solveGuess.Length > 0 && GameManager.Instance.currentPlayer == player.id)
            {
                GameManager.Instance.otherPlayerSolution = playerUpdate.solveGuess;
                GameManager.Instance.otherPlayerSolving = true;
            }

            // Only if other player has turn
            if (GameManager.Instance.currentPlayer == playerUpdate.orderid)
            {
                //GameManager.Instance.roulette.spinning = false;
                GameManager.Instance.gamePhaseManager.SetPhase((GamePhase)playerUpdate.state);

                // Only in guess phase
                if (GameManager.Instance.gamePhaseManager.CheckPhase(GamePhase.GUESS))
                {
                    GameManager.Instance.roulette.spinning = false;
                    GameManager.Instance.spinResult = playerUpdate.spinPoints;
                }
            }
        }

    }

    bool SendMessage(Message message)
    {
        try
        {
            Byte[] sendBytes = Encoding.ASCII.GetBytes(JsonUtility.ToJson(message));
            udp.Send(sendBytes, sendBytes.Length);
        }
        catch { }
        return true;
    }

    void HeartBeat()
    {
        Message heartMsg = new Message();
        heartMsg.command = "heartbeat";
        heartMsg.uid = uid;

        SendMessage(heartMsg);
    }

    public bool SendGameUpdate(bool emergency = false)
    {
        if ((GameManager.Instance.clientPlayer == null) || (!GameManager.Instance.CheckHasTurn() && !emergency))
        {
            return true;
        }

        Player gameMsg = new Player();

        gameMsg.uid = uid;
        gameMsg.command = "gameUpdate";

        gameMsg.roundScore = GameManager.Instance.clientPlayer.roundScore;
        gameMsg.cumulativeScore = GameManager.Instance.clientPlayer.cumulativeScore; // Players score according to the UI label
        gameMsg.orderid = GameManager.Instance.clientPlayer.id; // ID inside the game, not profile id

        print(GameManager.Instance.clientPlayer.cumulativeScore);

        gameMsg.letterGuess = ui.guessChar.ToString(); // Player's letter guess
        gameMsg.solveGuess = ui.guessSolve; // Player's solve guess

        Debug.Log("SENDING MSG WITH GUESS: " + gameMsg.letterGuess);

        gameMsg.spinPoints = GameManager.Instance.spinResult;

        gameMsg.state = (int)GameManager.Instance.gamePhaseManager.phase;

        return SendMessage(gameMsg);
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

    public void SendQuitMessage()
    {
        GameState gameStateMsg = new GameState();
        gameStateMsg.command = "quit";
        gameStateMsg.uid = uid;
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
    }
}
