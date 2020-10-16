using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PlayerBehaviour : MonoBehaviour
{
    public int score;
    public bool hasTurn;

    public Display display;
    public TMP_InputField tmpInputField;

    // Score show
    public TextMeshProUGUI scores;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SubmitLetter();
        }
    }

    public void SubmitLetter()
    {
        if (tmpInputField.text != "")
        {
            // Sets the input field to selected
            EventSystem.current.SetSelectedGameObject(tmpInputField.gameObject, null);
            tmpInputField.OnPointerClick(new PointerEventData(EventSystem.current));

            // Clears the guess
            display.MakeGuess((tmpInputField.text.ToCharArray())[0]);
            tmpInputField.text = "";
            tmpInputField.Select();
        }
        Debug.Log("Player entered the letter: " + Guess.currentGuess);
    }
}
