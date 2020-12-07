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

public class UI : MonoBehaviour
{
    public GameObject errorText; // NOTE: Would be better to specify that this is a TMP Text object...

    public static List<string> usernames = new List<string>() {"test", "test2"}; // TODO: List of account objects will likely need to be obtained here
    public TMP_InputField tmpInputField;
    public TMP_InputField tmpSolveField;

    public Display display;
    public Button submitButton;
    public GameObject scoreLabel;
    public GameObject solve;
    public GameObject guess;

    public char guessChar;
    public string guessSolve;

    ///////////////////////////////////// LoginScene /////////////////////////////////////

    public void Login()
    {
        if (usernames.Contains(tmpInputField.text)) // Entered username exists within known usernames
        {
            PlayerInfo.username = tmpInputField.text;
            SceneManager.LoadScene("MainMenuScene");
        }
        else
        {
            errorText.SetActive(true);
            Debug.Log("Username does not exist");
        }
    }

    public void LoadCreateAccountScene()
    {
        SceneManager.LoadScene("CreateAccountScene");
    }

    public void CreateAccount()
    {
        if (usernames.Contains(tmpInputField.text)) // Entered username exists within known usernames
        {
            errorText.SetActive(true);
        }
        else if (!usernames.Contains(tmpInputField.text)) // Create account here
        {
            usernames.Add(tmpInputField.text);
            PlayerInfo.username = tmpInputField.text;
            SceneManager.LoadScene("MainMenuScene");

            //for (int i = 0; i < usernames.Count; i++)
            //{
            //    Debug.Log("The " + i + " element in the list contains: " + usernames[i]);
            //}
        }
    }

    ///////////////////////////////////// LoginScene /////////////////////////////////////

    ///////////////////////////////////// GameScene /////////////////////////////////////

    UnityEvent loseTurn = new UnityEvent();

    public void AddLoseTurnListener(UnityAction action)
    {
        loseTurn.AddListener(action);
    }

    public void SubmitLetter(PlayerBehaviour player)
    {
        if (GameManager.Instance.gamePhaseManager.CheckPhase(GamePhase.SPIN))
        {
            return;
        }

        if (solve.activeInHierarchy && GameManager.Instance.gamePhaseManager.CheckPhase(GamePhase.SOLVE))
        {
            guessSolve = tmpSolveField.text;

            if (!display.Solve(tmpSolveField.text, ref player))
            {
                LoseTurn();
            }
            else
            {
                // Sets the input field to selected
                EventSystem.current.SetSelectedGameObject(tmpInputField.gameObject, null);
                tmpInputField.OnPointerClick(new PointerEventData(EventSystem.current));
            }
            // Show player's current score
            player.scores.GetComponent<TextMeshProUGUI>().text = (player.cumulativeScore + player.roundScore).ToString();

            tmpSolveField.text = "";
            tmpInputField.Select();

            solve.SetActive(false);
            guess.SetActive(true);

            GameManager.Instance.gamePhaseManager.SetPhase(GamePhase.SELECT);
        }
        else if (guess.activeInHierarchy && GameManager.Instance.gamePhaseManager.CheckPhase(GamePhase.GUESS))
        {
            if (tmpInputField.text != "")
            {
                int scoreVal = GameManager.Instance.roulette.GetSpinResult();

                guessChar = (tmpInputField.text.ToCharArray())[0];

                // Clears the guess
                if (!display.MakeGuess(guessChar, ref player, scoreVal))
                {
                    LoseTurn();
                }
                else
                {
                    // Sets the input field to selected
                    EventSystem.current.SetSelectedGameObject(tmpInputField.gameObject, null);
                    tmpInputField.OnPointerClick(new PointerEventData(EventSystem.current));
                }

                // Show player's current score
                player.scores.GetComponent<TextMeshProUGUI>().text = (player.cumulativeScore + player.roundScore).ToString();

                tmpInputField.text = "";
                tmpInputField.Select();

                GameManager.Instance.gamePhaseManager.SetPhase(GamePhase.SELECT);
                //Debug.Log("Player entered the letter: " + Guess.currentGuess);
            }
        }
    }

    public void LoseTurn()
    {
        Debug.Log("LOSE TURN");
        DisableInput();
        loseTurn.Invoke();
    }

    public void DisableInput()
    {
        tmpInputField.interactable = false;
        submitButton.interactable = false;
    }

    public void EnableInput()
    {
        tmpInputField.interactable = true;
        submitButton.interactable = true;
    }

    public void ViewAccount()
    {
        SceneManager.LoadScene("AccountScene");
    }

    public void Logout()
    {
        // Reset all user variables here

        SceneManager.LoadScene("LoginScene");
    }

    ///////////////////////////////////// GameScene /////////////////////////////////////

    ///////////////////////////////////// DEBUG /////////////////////////////////////

    public void LoadLoginScene()
    {
        SceneManager.LoadScene("LoginScene");
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    ///////////////////////////////////// DEBUG /////////////////////////////////////
}
