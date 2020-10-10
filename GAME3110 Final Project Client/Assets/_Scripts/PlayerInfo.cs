using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    string username;
    //string password; // Maybe it might be better to just have this stored on the server somewhere?
    int numWins; // Number of wins that can be presented to the player in their account info, this could also be used to calculate their level
    int playerLevel; // This could increase every 100xp for example
    int xpRequired = 100; // Amount of xp needed to level up

    // Start is called before the first frame update
    void Start()
    {
        
    }

    string GetUsername()
    {
        return username;
    }

    int GetNumWins()
    {
        return numWins;
    }

    int GetXPRequired()
    {
        return xpRequired;
    }

    int GetPlayerLevel()
    {
        return playerLevel;
    }
}
