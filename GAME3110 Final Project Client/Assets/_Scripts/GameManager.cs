using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
    public List<PlayerBehaviour> players;
    public PlayerBehaviour clientPlayer;
    public int currentPlayer = 0;
    public UI ui;

    public GamePhases gamePhaseManager;
    public Roulette roulette;

    public string state;
    public int wordIndex = -1;
    public int prevWordIndex = -1;

    public Display display;
    public bool gameStart = false;
    public bool otherPlayerGuessing = false;
    public bool otherPlayerSolving = false;
    public bool clientHasTurn = false;
    public char otherPlayerGuess;
    public string otherPlayerSolution;
    public bool removeAPlayer = false;
    public int playerToRemove = -1;
    public bool gameOver = false;
    public bool connected = false;

    public int spinResult;
    public bool hasRoundEnded = false; // Safety at the end of each round to make sure everyone is safe to proceed

    public GameObject resultsScreen;
    public TMP_Text resultsText;

    // Start is called before the first frame update
    void Start()
    {
        ui.AddLoseTurnListener(GiveTurn);
        display.onPuzzleSolved.AddListener(FinishRound);
        players = new List<PlayerBehaviour>();
    }

    public void AddPlayerToGame(NetworkMatchLoop.Player player, string uid)
    {
        // We already added the player
        if (players.Count > player.orderid)
        {
            return;
        }

        GameObject newPlayer = Instantiate(playerPrefab);
        PlayerBehaviour newPlayerBehaviour = newPlayer.GetComponent<PlayerBehaviour>();

        newPlayerBehaviour.id = player.orderid;
        newPlayerBehaviour.username = player.uid;

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

        for (int i = 0; i <= player.orderid; i++)
        {
            if (i >= players.Count)
            {
                players.Add(null);
            }
            
            if (i == player.orderid)
            {
                players[i] = newPlayerBehaviour;
            }
        }
    }

    public bool CheckHasTurn()
    {
        try
        {
            return clientPlayer.id == currentPlayer;
        }
        catch
        {
            SceneManager.LoadScene("MainMenuScene");
            return false;
        }
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

        //NetworkMatchLoop.Instance.SendGameUpdate(true); // This is not immediately sent on bankrupt
        NetworkMatchLoop.Instance.SendLoseTurnMessage(currentPlayer);
    }

    public void TakeTurn()
    {
        if (CheckHasTurn())
        {
            roulette.spinning = false;
            clientHasTurn = true;
            //NetworkMatchLoop.Instance.SendGameUpdate(true);
        }
        else
        {
            ui.DisableInput();
        }
    }

    public void RemovePlayer(int orderid)
    {
        if (orderid >= 0 && orderid < players.Count)
        {
            players[orderid].RemovePlayer();
            players.RemoveAt(orderid);

            // Update client id
            for (int i = 0; i < players.Count; i++)
            {
                players[i].id = i;
            }

            TakeTurn();
        }
    }

    void FinishRound()
    {
        if (gameStart)
        {
            // Need new word from server
            NetworkMatchLoop.Instance.SendRoundEndMessage();
            gameStart = false; // Game paused
            wordIndex = -1;
            prevWordIndex = wordIndex;
        }
    }

    IEnumerator CloseMatch()
    {
        List<int> scores = new List<int>();

        foreach (PlayerBehaviour player in players)
        {
            scores.Add(player.cumulativeScore);
        }

        scores.Sort((x, y) => y.CompareTo(x));

        int rank = 0;
        for (int i = 0; i < scores.Count; i++)
        {
            if (clientPlayer.cumulativeScore == scores[i])
            {
                rank = i + 1;
                break;
            }
        }

        if (players.Count > 1)
        {
            resultsScreen.SetActive(true);
            resultsText.text = "You are #" + rank;
        }

        foreach (int score in scores)
        {
            Debug.Log(score);
        }

        Debug.Log("You are #" + rank);

        display.gameObject.SetActive(false);  

        NetworkMatchLoop.Instance.SendGameUpdate(true);
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("MainMenuScene");
        NetworkMatchLoop.Instance.CloseConnection();
    }

    // Handle remote gameplay
    private void Update()
    {
        if (gameStart == false && wordIndex != prevWordIndex && display.wordBank != null && connected)
        {
            prevWordIndex = wordIndex;
            gameStart = true;
            display.Setup(wordIndex);
            display.StartNextRound();

            // No one should be guessing
            otherPlayerGuessing = false;
            otherPlayerGuessing = false;

            TakeTurn();
        }

        // Remote guess
        if (otherPlayerGuessing)
        {
            PlayerBehaviour player = players[currentPlayer];
            GameManager.Instance.display.MakeGuess(otherPlayerGuess, ref player, 0);
            otherPlayerGuessing = false;
            ui.guessChar = '\0';
        }

        // Remote solve
        if (otherPlayerSolving)
        {
            PlayerBehaviour player = players[currentPlayer];
            GameManager.Instance.display.Solve(otherPlayerSolution, ref player);
            otherPlayerGuessing = false;
            ui.guessSolve = "";
        }

        if (clientHasTurn)
        {
            ui.EnableInput();
            clientHasTurn = false;
        }

        if (gamePhaseManager.phase == GamePhase.SPIN && clientPlayer.id != currentPlayer && !roulette.spinning)
        {
            roulette.spinning = true;
            roulette.ExternalSpin();
        }

        // Only in guess phase
        if (GameManager.Instance.gamePhaseManager.CheckPhase(GamePhase.GUESS) && clientPlayer.id != currentPlayer)
        {
            switch (spinResult)
            {
                case 0:
                    GameManager.Instance.roulette.display.text = "LOSE A TURN";
                    break;
                case -1:
                    GameManager.Instance.roulette.display.text = "BANKRUPT";
                    break;
                default:
                    GameManager.Instance.roulette.display.text = spinResult.ToString();
                    break;
            }
        }

        if (removeAPlayer)
        {
            RemovePlayer(playerToRemove);
            playerToRemove = -1;
            removeAPlayer = false;
        }

        if (gameOver)
        {
            StartCoroutine(CloseMatch());
        }
    }
}
