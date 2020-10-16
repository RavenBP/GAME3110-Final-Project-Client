using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class UI : MonoBehaviour
{
    public TMP_InputField tmpInputField;
    public GameObject errorText; // NOTE: Would be better to specify that this is a TMP Text object...

    public static List<string> usernames = new List<string>() {"test", "test2"}; // TODO: List of account objects will likely need to be obtained here
    public Display display;

    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(tmpInputField.gameObject, null);
        tmpInputField.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SubmitLetter();
        }
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

    public void SubmitLetter()
    {
        if (tmpInputField.text != "")
        {
            EventSystem.current.SetSelectedGameObject(tmpInputField.gameObject, null);
            tmpInputField.OnPointerClick(new PointerEventData(EventSystem.current));

            // Clears the guess
            display.MakeGuess((tmpInputField.text.ToCharArray())[0]);
            tmpInputField.text = "";
            tmpInputField.Select();
        }
        Debug.Log("Player entered the letter: " + Guess.currentGuess);
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
