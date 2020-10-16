using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public List<PlayerBehaviour> players;
    int currentPlayer = 0;
    public UI ui;

    // Start is called before the first frame update
    void Start()
    {
        ui.AddLoseTurnListener(GiveTurn);

        for (int i = 0; i < players.Count; i++) 
        {
            players[i].id = i + 1; 
        }
    }

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
