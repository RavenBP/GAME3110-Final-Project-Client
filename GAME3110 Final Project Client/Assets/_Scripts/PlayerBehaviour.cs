using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class PlayerBehaviour : MonoBehaviour
{
    public int id;
    public string username;
    public int cumulativeScore;
    public int roundScore;
    public bool hasTurn;

    public GameObject scoreLabelPrefab;
    GameObject scoreLabel;
    public UI ui;

    [SerializeField]
    private GameObject scores;
    private GameObject playerInfo;
    public TMP_Text playerUsernameLabel;

    private void Awake()
    {
        ui = GameObject.Find("UI").gameObject.GetComponent<UI>();

        //SetPlayerProfileUI();
    }

    public void SetPlayerProfileUI()
    {
        // Setup Game Objects by finding them in the scene
        scoreLabel = ui.scoreLabel;

        // Instantiate the score label so that new players will have their score go underneath the previous
        playerInfo = Instantiate(scoreLabelPrefab, ui.scoreLabel.transform);

        playerUsernameLabel = playerInfo.transform.Find("Player_Name").gameObject.GetComponent<TMP_Text>();
        playerUsernameLabel.text = username;

        scores = playerInfo.transform.Find("Player_Score").gameObject;

        playerInfo.GetComponent<RectTransform>().localPosition = new Vector3(40, (playerInfo.GetComponent<RectTransform>().localPosition.y - 120 * id));
    }

    public void DisplayScore()
    {
        scores.GetComponent<TextMeshProUGUI>().text = (cumulativeScore + roundScore).ToString();
    }

    public void RemovePlayer()
    {
        Destroy(scores);
        Destroy(playerInfo);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        DisplayScore();

        if (!hasTurn)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            ui.SubmitLetter();
        }
    }
}
