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

    private void Start()
    {
        // Sets the input field to selected
        EventSystem.current.SetSelectedGameObject(tmpInputField.gameObject, null);
        tmpInputField.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    

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
        if (solve.activeInHierarchy)
        {
            if (!display.Solve(tmpSolveField.text, ref player.score))
            {
                DisableInput();
                loseTurn.Invoke();
            }
            else
            {
                // Sets the input field to selected
                EventSystem.current.SetSelectedGameObject(tmpInputField.gameObject, null);
                tmpInputField.OnPointerClick(new PointerEventData(EventSystem.current));
            }
            // Show player's current score
            player.scores.GetComponent<TextMeshProUGUI>().text = player.score.ToString();

            tmpSolveField.text = "";
            tmpInputField.Select();

            solve.SetActive(false);
            guess.SetActive(true);
            
        }
        else if (guess.activeInHierarchy)
        {
            if (tmpInputField.text != "")
            {
                // Clears the guess
                if (!display.MakeGuess((tmpInputField.text.ToCharArray())[0], ref player.score))
                {
                    DisableInput();
                    loseTurn.Invoke();
                }
                else
                {
                    // Sets the input field to selected
                    EventSystem.current.SetSelectedGameObject(tmpInputField.gameObject, null);
                    tmpInputField.OnPointerClick(new PointerEventData(EventSystem.current));
                }

                // Show player's current score
                player.scores.GetComponent<TextMeshProUGUI>().text = player.score.ToString();

                tmpInputField.text = "";
                tmpInputField.Select();
            }

            //Debug.Log("Player entered the letter: " + Guess.currentGuess);
        }
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
