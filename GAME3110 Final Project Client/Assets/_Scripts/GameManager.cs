using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance { get { return instance; } }

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

    public GameObject playerPrefab;
    public List<PlayerBehaviour> playerDebug;
    public Dictionary<int, PlayerBehaviour> players;
    public PlayerBehaviour clientPlayer;
    public int currentPlayer = 0;
    public UI ui;

    public GamePhases gamePhaseManager;
    public Roulette roulette;

    public string state;
    public int wordIndex = -1;

    public Display display;
    public bool gameStart = false;
    public bool otherPlayerGuessing = false;
    public char otherPlayerGuess;

    // Start is called before the first frame update
    void Start()
    {
        ui.AddLoseTurnListener(GiveTurn);
        players = new Dictionary<int, PlayerBehaviour>();
        //for (int i = 0; i < players.Count; i++) 
        //{
        //    players[i].id = i + 1; 
        //}
    }

    public void AddPlayerToGame(NetworkMatchLoop.Player player, string uid)
    {
        // We already added the player
        if (players.ContainsKey(player.orderid))
        {
            return;
        }

        GameObject newPlayer = Instantiate(playerPrefab);
        PlayerBehaviour newPlayerBehaviour = newPlayer.GetComponent<PlayerBehaviour>();

        newPlayerBehaviour.id = player.orderid;

        // If the new player is this player
        if (player.uid == uid)
        {
            clientPlayer = newPlayerBehaviour;

            if (player.orderid != 0)
            {
                ui.DisableInput();
            }
        }
        newPlayerBehaviour.SetPlayerProfileUI();

        players.Add(player.orderid, newPlayerBehaviour);
        playerDebug.Add(newPlayer.GetComponent<PlayerBehaviour>());
    }

    public bool CheckHasTurn()
    {
        return clientPlayer.id == currentPlayer;
    }

    // Hand turn to next player in list
    void GiveTurn()
    {
        // Check end of index
        if (currentPlayer < players.Count - 1)
        {
            currentPlayer++;
        }
        else
        {
            currentPlayer = 0;
        }

        ui.DisableInput();
    }

    // Handle remote gameplay
    private void Update()
    {
        if (gameStart == false && wordIndex != -1 && display.wordBank != null)
        {
            gameStart = true;
            display.Setup(wordIndex);
        }

        // Remote guess
        if (otherPlayerGuessing)
        {
            PlayerBehaviour player = players[currentPlayer];
            GameManager.Instance.display.MakeGuess(otherPlayerGuess, ref player, 0);
            otherPlayerGuessing = false;
        }
    }
}
