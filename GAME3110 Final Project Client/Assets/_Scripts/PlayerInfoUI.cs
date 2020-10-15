using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInfoUI : MonoBehaviour
{
    [SerializeField]
    TMP_Text usernameText;

    [SerializeField]
    TMP_Text winsText;

    [SerializeField]
    TMP_Text levelText;

    // Start is called before the first frame update
    void Start()
    {
        if (usernameText)
        {
            usernameText.text = PlayerInfo.username;
        }

        if (levelText)
        {
            levelText.text = PlayerInfo.level.ToString();
        }

        if (winsText)
        {
            winsText.text = PlayerInfo.numWins.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
