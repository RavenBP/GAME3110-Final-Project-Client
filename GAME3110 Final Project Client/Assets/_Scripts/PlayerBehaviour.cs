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
    public int score;
    public bool hasTurn;

    public GameObject scoreLabelPrefab;
    GameObject scoreLabel;
    public UI ui;

    public GameObject scores;

    private void Start()
    {
        ui = GameObject.Find("UI").gameObject.GetComponent<UI>();

        // Setup Game Objects by finding them in the scene
        scoreLabel = ui.scoreLabel;

        // Instantiate the score label so that new players will have their score go underneath the previous
        scores = Instantiate(scoreLabelPrefab, ui.scoreLabel.transform);
        scores.GetComponent<RectTransform>().position += new Vector3(0, (id - 1) * 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasTurn)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            ui.SubmitLetter(this);
        }
    }
}
