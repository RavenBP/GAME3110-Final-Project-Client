﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PlayerInfo : MonoBehaviour
{
    public static string username;
    //string password; // Maybe it might be better to just have this stored on the server somewhere?
    public static int numWins = 0; // Number of wins that can be presented to the player in their account info, this could also be used to calculate their level
    public static int level = 1; // This could increase every 100xp for example
    public static int exp = 100; // Amount of xp needed to level up

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
        return exp;
    }

    int GetLevel()
    {
        return level;
    }
}
