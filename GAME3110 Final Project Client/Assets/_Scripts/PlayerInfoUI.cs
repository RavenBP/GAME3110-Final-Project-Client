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

    // Start is called before the first frame update
    void Start()
    {
        usernameText.text = PlayerInfo.username;
        winsText.text = PlayerInfo.numWins.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
