using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<PlayerBehaviour> players;
    int currentPlayer = 0;

    // Start is called before the first frame update
    void Start()
    {
        players[currentPlayer].EnablePlayer(); // Turn player on

        for (int i = 0; i < players.Count; i++) 
        {
            players[i].AddLoseTurnListener(GiveTurn);
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

        players[currentPlayer].EnablePlayer();
    }
}
