using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PlayerBehaviour : MonoBehaviour
{
    public int score;
    public bool hasTurn;

    public Display display;
    public TMP_InputField tmpInputField;
    public Button submitButton;

    // Score show
    public TextMeshProUGUI scores;

    private void Start()
    {
        // Sets the input field to selected
        EventSystem.current.SetSelectedGameObject(tmpInputField.gameObject, null);
        tmpInputField.OnPointerClick(new PointerEventData(EventSystem.current));
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
            SubmitLetter();
        }
    }

    public void DisablePlayer()
    {
        hasTurn = false;
        this.enabled = false;
        tmpInputField.interactable = false;
        submitButton.interactable = false;
    }

    public void EnablePlayer()
    {
        hasTurn = true;
        this.enabled = true;
        tmpInputField.interactable = true;
        submitButton.interactable = true;
    }

    public void SubmitLetter()
    {
        if (tmpInputField.text != "")
        {
            // Clears the guess
            if (!display.MakeGuess((tmpInputField.text.ToCharArray())[0], ref score))
            {
                DisablePlayer();
            }
            else
            {
                // Sets the input field to selected
                EventSystem.current.SetSelectedGameObject(tmpInputField.gameObject, null);
                tmpInputField.OnPointerClick(new PointerEventData(EventSystem.current));
            }

            // Show player's current score
            scores.text = score.ToString();

            tmpInputField.text = "";
            tmpInputField.Select();
        }

        //Debug.Log("Player entered the letter: " + Guess.currentGuess);
    }
}
