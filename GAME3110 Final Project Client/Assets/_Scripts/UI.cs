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
using UnityEngine.Networking;

public class UI : MonoBehaviour
{
    public GameObject errorText; // NOTE: Would be better to specify that this is a TMP Text object...

    public static List<string> usernames = new List<string>() {"test", "test2"}; // TODO: List of account objects will likely need to be obtained here
    public TMP_InputField tmpInputField1;
    public TMP_InputField tmpInputField2;
    public TMP_InputField tmpSolveField;

    public Display display;
    public Button submitButton;
    public GameObject scoreLabel;
    public GameObject solve;
    public GameObject guess;

    public char guessChar;
    public string guessSolve;

    public List<Button> interactableButtons;

    ///////////////////////////////////// LoginScene /////////////////////////////////////

    private void Start()
    {
        // Players are instantiated dynamically, so by default it is null in the inspector
        submitButton.GetComponent<Button>().onClick.AddListener(SubmitLetter);
    }

    public void Login()
    {
        Debug.Log("Logging in...");
        StartCoroutine(AccountLogin(tmpInputField1.text, tmpInputField2.text));
    }

    IEnumerator AccountLogin(string username, string password)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get("https://ren12vw886.execute-api.us-east-1.amazonaws.com/default/AccountLogin?username=" + username + "&password=" + password);

        yield return webRequest.SendWebRequest();

        // Because of JSON serialization, " is being added to the beginning/end of the string we are retrieving...
        string testString = webRequest.downloadHandler.text;
        testString = testString.Trim('"'); 

        if (webRequest.isNetworkError == false)
        {
            if (testString == "LOGGED IN")
            {
                PlayerInfo.username = username; // NOTE: It might be better to set the username to the username found in the database...
                SceneManager.LoadScene("MainMenuScene");
                Debug.Log("The player logged in");
            }
            else if (testString == "INCORRECT PASSWORD")
            {
                Debug.Log("The player entered an incorrect password");
            }
            else if (testString == "ACCOUNT DOES NOT EXIST")
            {
                Debug.Log("That account does not exist");
                errorText.SetActive(true);
            }
        }
        else
        {
            Debug.Log("Network Error");
        }
    }

    public void LoadCreateAccountScene()
    {
        SceneManager.LoadScene("CreateAccountScene");
    }

    public void CreateAccount()
    {
        if (usernames.Contains(tmpInputField1.text)) // Entered username exists within known usernames
        {
            errorText.SetActive(true);
        }
        else if (!usernames.Contains(tmpInputField1.text)) // Create account here
        {
            usernames.Add(tmpInputField1.text);
            PlayerInfo.username = tmpInputField1.text;
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

    public void SubmitLetter()
    {
        PlayerBehaviour player = GameManager.Instance.players[GameManager.Instance.currentPlayer];

        if (GameManager.Instance.gamePhaseManager.CheckPhase(GamePhase.SPIN))
        {
            return;
        }

        if (solve.activeInHierarchy && GameManager.Instance.gamePhaseManager.CheckPhase(GamePhase.SOLVE))
        {
            guessSolve = tmpSolveField.text;
            NetworkMatchLoop.Instance.SendGameUpdate(); // Send after each guess

            if (!display.Solve(tmpSolveField.text, ref player))
            {
                StartCoroutine(LoseTurn());
            }
            else
            {
                // Sets the input field to selected
                EventSystem.current.SetSelectedGameObject(tmpInputField1.gameObject, null);
                tmpInputField1.OnPointerClick(new PointerEventData(EventSystem.current));
            }
            // Show player's current score
            player.DisplayScore();

            tmpSolveField.text = "";
            tmpInputField1.Select();

            solve.SetActive(false);
            guess.SetActive(true);

            GameManager.Instance.gamePhaseManager.SetPhase(GamePhase.SELECT);

            guessSolve = "";
        }
        else if (guess.activeInHierarchy && GameManager.Instance.gamePhaseManager.CheckPhase(GamePhase.GUESS))
        {
            if (tmpInputField1.text != "")
            {
                int scoreVal = GameManager.Instance.roulette.GetSpinResult();

                guessChar = (tmpInputField1.text.ToCharArray())[0];
                NetworkMatchLoop.Instance.SendGameUpdate(); // Send after each guess

                // Clears the guess
                if (!display.MakeGuess(guessChar, ref player, scoreVal))
                {
                    StartCoroutine(LoseTurn());
                }
                else
                {
                    // Sets the input field to selected
                    EventSystem.current.SetSelectedGameObject(tmpInputField1.gameObject, null);
                    tmpInputField1.OnPointerClick(new PointerEventData(EventSystem.current));
                }

                // Show player's current score
                player.DisplayScore();

                tmpInputField1.text = "";
                tmpInputField1.Select();

                GameManager.Instance.gamePhaseManager.SetPhase(GamePhase.SELECT);

                guessChar = '\0';
            }
        }
    }

    public IEnumerator LoseTurn()
    {
        Debug.Log("LOSE TURN");
        DisableInput();

        yield return new WaitForSeconds(0.1f); // Delay for a short bit
        loseTurn.Invoke();
    }

    public void DisableInput()
    {
        tmpInputField1.interactable = false;
        tmpSolveField.interactable = false;
        submitButton.interactable = false;

        foreach (Button button in interactableButtons)
        {
            button.interactable = false;
        }
    }

    public void EnableInput()
    {
        tmpInputField1.interactable = true;
        tmpSolveField.interactable = true;
        submitButton.interactable = true;

        foreach (Button button in interactableButtons)
        {
            button.interactable = true;
        }
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
        if (GameManager.Instance.CheckHasTurn())
        {
            StartCoroutine(LoseTurn());
        }
        NetworkMatchLoop.Instance.SendQuitMessage();
        SceneManager.LoadScene("MainMenuScene");
    }

    ///////////////////////////////////// DEBUG /////////////////////////////////////
}
