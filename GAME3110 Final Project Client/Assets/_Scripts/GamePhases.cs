using System.Collections;
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

    private void Start()
    {
        phase = GamePhase.SELECT;
    }

    private void Update()
    {
        switch (phase)
        {
            case GamePhase.SPIN:
                tmpInputField.gameObject.SetActive(false);
                tmpSolveField.gameObject.SetActive(false);
                solveButton.SetActive(false);
                guessButton.SetActive(false);
                submitButton.gameObject.SetActive(false);
                spinButton.gameObject.SetActive(true);
                break;
            case GamePhase.SELECT:
                tmpInputField.gameObject.SetActive(false);
                tmpSolveField.gameObject.SetActive(false);
                solveButton.SetActive(true);
                guessButton.SetActive(true);
                submitButton.gameObject.SetActive(false);
                spinButton.gameObject.SetActive(false);
                break;
            case GamePhase.GUESS:
                tmpInputField.gameObject.SetActive(true);
                tmpSolveField.gameObject.SetActive(false);
                solveButton.SetActive(false);
                submitButton.gameObject.SetActive(true);
                spinButton.gameObject.SetActive(false);
                break;
            case GamePhase.SOLVE:
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
        Debug.Log(phase);
    }

    public bool CheckPhase(GamePhase _phase)
    {
        return phase == _phase;
    }
}
