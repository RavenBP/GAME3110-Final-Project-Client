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

    public Display display;

    public GamePhases gamePhaseManager;
    public Roulette roulette;

    public string state;

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

        // If the new player is this player
        if (player.uid == uid)
        {
            clientPlayer = newPlayer.GetComponent<PlayerBehaviour>();
            clientPlayer.id = player.orderid;

            if (player.orderid != 0)
            {
                ui.DisableInput();
            }
        }
        players.Add(player.orderid, newPlayer.GetComponent<PlayerBehaviour>());
        playerDebug.Add(newPlayer.GetComponent<PlayerBehaviour>());
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

        ui.EnableInput();
    }
}
