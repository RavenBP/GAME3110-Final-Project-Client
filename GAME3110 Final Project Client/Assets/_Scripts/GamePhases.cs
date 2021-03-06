﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GamePhase
{
    SPIN,
    GUESS,
    SOLVE,
    SELECT
}

public class GamePhases : MonoBehaviour
{ 
    public GamePhase phase;

    public Button submitButton;
    public Button spinButton;
    public GameObject solveButton;
    public GameObject guessButton;
    public TMP_InputField tmpInputField;
    public TMP_InputField tmpSolveField;
    public GameObject letterValue;

    private void Start()
    {
        phase = GamePhase.SELECT;
    }

    private void Update()
    {
        // Turns on/off UI elements based on player's choices
        switch (phase)
        {
            case GamePhase.SPIN:

                letterValue.SetActive(true);
                tmpInputField.gameObject.SetActive(false);
                tmpSolveField.gameObject.SetActive(false);
                solveButton.SetActive(false);
                guessButton.SetActive(false);
                submitButton.gameObject.SetActive(false);
                spinButton.gameObject.SetActive(true);

                break;
            case GamePhase.SELECT:

                letterValue.SetActive(false);
                tmpInputField.gameObject.SetActive(false);
                tmpSolveField.gameObject.SetActive(false);
                solveButton.SetActive(true);
                guessButton.SetActive(true);
                submitButton.gameObject.SetActive(false);
                spinButton.gameObject.SetActive(false);

                break;
            case GamePhase.GUESS:

                letterValue.SetActive(true);
                tmpInputField.gameObject.SetActive(true);
                tmpSolveField.gameObject.SetActive(false);
                solveButton.SetActive(false);
                submitButton.gameObject.SetActive(true);
                spinButton.gameObject.SetActive(false);

                break;
            case GamePhase.SOLVE:

                letterValue.SetActive(false);
                solveButton.SetActive(false);
                guessButton.SetActive(false);
                tmpInputField.gameObject.SetActive(false);
                tmpSolveField.gameObject.SetActive(true);
                submitButton.gameObject.SetActive(true);
                spinButton.gameObject.SetActive(false);

                break;
        }
    }

    public void SetPhase(GamePhase _phase)
    {
        phase = _phase;

        if (GameManager.Instance.clientPlayer.id == GameManager.Instance.currentPlayer)
        {
            if (!CheckPhase(GamePhase.GUESS))
            {
                GameManager.Instance.ui.guessChar = '\0';
                GameManager.Instance.ui.guessSolve = "";
            }
            NetworkMatchLoop.Instance.SendGameUpdate(); // Send everytime phase changes by client
        }
    }

    public bool CheckPhase(GamePhase _phase)
    {
        return phase == _phase;
    }
}
