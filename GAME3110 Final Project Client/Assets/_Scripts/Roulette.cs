using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class Roulette : MonoBehaviour
{
    public TextMeshProUGUI display;
    Array values;
    Score randomValue;
    public UI ui;

    private void Start()
    {
        values = Enum.GetValues(typeof(Score));
    }

    public void StartStop()
    {
        // Do not spin wheel when guessing a letter
        if (GameManager.Instance.gamePhaseManager.CheckPhase(GamePhase.GUESS))
        {
            return;
        }

        spinning = !spinning;

        if (spinning && !alreadySpinning && GameManager.Instance.gamePhaseManager.CheckPhase(GamePhase.SELECT))
        {
            alreadySpinning = true;
            StartCoroutine(Spin());
            GameManager.Instance.gamePhaseManager.SetPhase(GamePhase.SPIN);
        }
    }

    // Called Externally 
    public void ExternalSpin()
    {
        StartCoroutine(Spin());
    }

    bool spinning = false;
    bool alreadySpinning = false;
    public IEnumerator Spin()
    {
        while (spinning)
        {
            // Cycle through Score to simulate a roulette
            randomValue = (Score)values.GetValue(UnityEngine.Random.Range(0, values.Length));

            // Score text
            if ((int)randomValue > 0)
            {
                display.faceColor = new Color32(11, 12, 135, 255);
                display.text = ((int)randomValue).ToString();
            }
            // LOSETURN BANKRUPT Texts
            else
            {
                display.faceColor = new Color32(251,38,11,255);
                display.text = randomValue.ToString().Remove(randomValue.ToString().Length-1);
            }

            yield return new WaitForSeconds(0.1f);
        }

        alreadySpinning = false;
        GameManager.Instance.gamePhaseManager.SetPhase(GamePhase.GUESS);
        GetSpinResult();
    }

    public int GetSpinResult()
    {
        if ((int)randomValue == 0)
        {
            GameManager.Instance.gamePhaseManager.SetPhase(GamePhase.SELECT);
            ui.LoseTurn();
        }
        else if ((int)randomValue == -1)
        {
            Debug.Log("BANKRUPT");
            GameManager.Instance.gamePhaseManager.SetPhase(GamePhase.SELECT);
            GameManager.Instance.clientPlayer.roundScore = 0;
            GameManager.Instance.clientPlayer.DisplayScore();
            ui.LoseTurn();
        }

        return (int)randomValue;
    }
}
