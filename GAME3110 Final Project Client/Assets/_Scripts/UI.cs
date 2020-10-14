using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    public TMP_InputField tmpInputField;
    public GameObject errorText; // NOTE: Would be better to specify that this is a TMP Text object...
    string username; // TODO: Associate this with a client

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    ///////////////////////////////////// LoginScene /////////////////////////////////////

    public void Login()
    {
        if (username == "test") // TODO: Check to see if a client with this username exists (maybe inside a list?)
        {
            SceneManager.LoadScene("GameScene"); // Load Game Scene
        }
        else // Display error text
        {
            Debug.Log("Username does not exist.");
            errorText.SetActive(true);
        }
    }

    public void EnteredUsername()
    {
        username = tmpInputField.text;
        //Debug.Log("The current value of input field is: " + username);
    }

    public void CreateAccount()
    {
        // Either load a scene or a pop up panel?
    }



    ///////////////////////////////////// LoginScene /////////////////////////////////////

    ///////////////////////////////////// GameScene /////////////////////////////////////

    public void SubmitLetter()
    {
        //Debug.Log("Player entered the letter: " + tmpInputField.text);
        Guess.currentGuess = (tmpInputField.text.ToCharArray())[0];
        Debug.Log("Player entered the letter: " + Guess.currentGuess);
    }

    public void ViewAccount()
    {
        SceneManager.LoadScene("AccountScene");
    }

    public void Logout()
    {
        // NOTE: Will likely need to remove all variables associated with the current user...

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

    public void LoadAccountScene() // Also used inside GameScene
    {
        SceneManager.LoadScene("AccountScene");
    }

    ///////////////////////////////////// DEBUG /////////////////////////////////////
}
