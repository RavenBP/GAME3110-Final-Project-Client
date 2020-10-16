using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Display : MonoBehaviour
{
    public Dictionary<char, List<int>> charToIndexDict; // Maps letters to location in phrase using indices
    private string alphabet = "abcdefghijklmnopqrstuvwxyz";

    public string originalPhrase;
    private char[] solution;
    private char[] guesses;
    // Word hint
    public TextMeshProUGUI question;
    // Missed Letter show
    public TextMeshProUGUI missedLetters;
    // Score show
    public TextMeshProUGUI scores;

    // Panels
    private List<List<GameObject>> panelLayout = new List<List<GameObject>>(); // How to display to players
    private List<GameObject> solutionPanels; // How the game sees the panels

    [SerializeField]
    private GameObject letterPrefab;

    public Guess guess;

    // keep tract player's current score
    private int score = 0;

    // List of all letters, wrong letters and remaining correct letters of current word
    private List<char> allLetters = new List<char>();
    private List<char> wrongLetters = new List<char>();
    private List<char> remainingCorrectLetters = new List<char>();

    // Words data
    public WordBank wordBank = new WordBank();
    // Index of current Word in WordBank array
    private int curIndex = -1;
    // Keep track of remaining words
    private List<int> remainingWordsIndices = new List<int>();

    string[] wordList;

    public bool reveal = false;

    // Start is called before the first frame update
    void Start()
    {
        LoadWords();
        Setup();
        //Debug.Log(wordBank.Words.Length);
    }

    // Update is called once per frame
    void Update()
    {
        if (reveal)
        {
            foreach (GameObject panel in solutionPanels)
            {
                panel.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
            }
        }

        // Start a new round
        if (remainingCorrectLetters.Count <= 0)
        {
            
            Setup();
        }
    }

    public void MakeGuess(char guess)
    {
        // Remove letter from remaining list when guessing correctly
        if (allLetters.Contains(char.ToUpper(guess)))
        {
            if (remainingCorrectLetters.Contains(char.ToUpper(guess)))
            {
                remainingCorrectLetters.Remove(char.ToUpper(guess));
                // Add score when player guesses correctly, add bonus score when player opens whole word
                score += Score.SINGLEPOINT * charToIndexDict[char.ToUpper(guess)].Count; // Multiplier bonus

                if (remainingCorrectLetters.Count <= 0) // All letters have been correctly guessed
                {
                    score += Score.WHOLEWORDPOINT;
                    PlayerInfo.numWins++;
                }
            }
        }
        // Add letter to wrong list when guessing wrong
        else
        {
            if (!wrongLetters.Contains(char.ToUpper(guess)) && guess != ' ')
            {
                wrongLetters.Add(char.ToUpper(guess));
            }
        }

        // Show player's current score
        scores.text = score.ToString();

        // Show list of missed letters on screen
        string wrongString = "";
        for (int i = 0; i < wrongLetters.Count; i++)
        {
            wrongString += wrongLetters[i].ToString();
            wrongString += (i == wrongLetters.Count - 1) ? "" : " , ";
        }
        missedLetters.faceColor = new Color32(251, 241, 24, 255);
        missedLetters.text = wrongString;

        // Test code - need better implementation
        if (charToIndexDict.ContainsKey(char.ToUpper(guess)))
        {
            foreach (int index in charToIndexDict[char.ToUpper(guess)])
            {
                solutionPanels[index].GetComponentInChildren<TextMeshProUGUI>().enabled = true;
            }
        }
    }

    // Reset some variables to initial values to start a new round
    private void _Reset()
    {
        Guess.currentGuess = ' ';
        allLetters = new List<char>();
        remainingCorrectLetters = new List<char>();
        wrongLetters = new List<char>();
        
        foreach (List<GameObject> row in panelLayout)
        {
            foreach (GameObject panel in row)
            {
                panel.SetActive(false);
            }
        }
    }

    private void LoadWords()
    {
        // Create Words data
        wordBank._InstantiateWorld();
        // Initiate list of ramaining words by indices
        for(int i = 0; i < wordBank.Words.Length; i++)
        {
            remainingWordsIndices.Add(i);
        }
    }
    private void Setup()
    {
        _Reset();

        // Choose playing word randomly but should be in remaining words pool
        do
        {
            curIndex = Random.Range(0, wordBank.Words.Length);
        } while (remainingWordsIndices.Contains(curIndex) == false);
        // Remove chosen word from remaining words pool
        remainingWordsIndices.Remove(curIndex);

        // Show word hint on game scene
        question.faceColor = new Color32(253, 26, 26, 255);
        question.text = wordBank.Words[curIndex].question;

        originalPhrase = wordBank.Words[curIndex].answer;

        solution = originalPhrase.ToCharArray();

        // Initializing arrays
        guesses = new char[solution.Length];
        solutionPanels = new List<GameObject>();
        panelLayout = new List<List<GameObject>>();
        charToIndexDict = new Dictionary<char, List<int>>();

        // Convert Lowercase to Uppercase
        originalPhrase = originalPhrase.ToUpper();
        alphabet = alphabet.ToUpper();

        // Initiate remaining and all letters lists
        for(int i = 0; i < originalPhrase.Length; i ++)
        {
            if(remainingCorrectLetters.Contains(originalPhrase[i]) == false)
            {
                remainingCorrectLetters.Add(originalPhrase[i]);
            }

            if (allLetters.Contains(originalPhrase[i]) == false)
            {
                allLetters.Add(originalPhrase[i]);
            }
        }

        InitPanelLayout();
        PresentPanel();

        // Init dictionary
        foreach (char letter in alphabet)
        {
            charToIndexDict[letter] = new List<int>();
        }

        // Solution without whitespaces; to keep indices consistent with
        // panels' indices
        string noSpaces = "";
        foreach (string word in wordList)
        {
            noSpaces += word;
        }
        
        for (int i = 0; i < noSpaces.Length; i++)
        {
            // Find the dictionary entry and add the index
            // The key is the alphabet
            charToIndexDict[noSpaces[i]].Add(i);
        }
    }

    // Init Panels
    void InitPanelLayout()
    {
        wordList = originalPhrase.Split();
        List<List<GameObject>> wordPanels = new List<List<GameObject>>();

        /* Explanation:
         * Instantiate a new letter panel.  Put this panel into two lists.
         * One is backend, for the game logic to figure what panel should be turned on/off.
         * One is frontend, what the user sees, which should be more visually appealing than what the game sees.
         * 
         * This works because the panel is the same panel in both Lists (same reference).
         */

        // Create panel references from game logic to onscreen display
        foreach (string word in wordList)
        {
            List<GameObject> wordPanel = new List<GameObject>(); // A list of panels representing the word

            foreach (char letter in word)
            {
                // Create a panel for the letter
                GameObject panel = Instantiate(letterPrefab);
                panel.GetComponentInChildren<TextMeshProUGUI>().text = letter.ToString();
                panel.GetComponentInChildren<TextMeshProUGUI>().enabled = false;

                // Indices will be one to one with original phrase
                solutionPanels.Add(panel);

                wordPanel.Add(panel);
            }

            // Add a white space to the end of every word
            GameObject blankPanel = Instantiate(letterPrefab);
            blankPanel.GetComponentInChildren<TextMeshProUGUI>().text = " ";
            blankPanel.GetComponent<Image>().color = Color.green;
            wordPanel.Add(blankPanel);

            wordPanels.Add(wordPanel); // List of words in panel form
        }  

        // Format layout for player by adding padding and word wrap
        int rowCount = 1;
        List<GameObject> panelRow = new List<GameObject>(); // A row of panels in the layout

        for (int i = 0; i < wordPanels.Count; i++)
        {
            // Check if row can fit more words
            if (panelRow.Count + wordPanels[i].Count < 15)
            {
                // Fit as many words as possible in one row.
                panelRow.AddRange(wordPanels[i]);
            }
            // Row filled - pad the rest of the row
            else if (panelRow.Count + wordPanels[i].Count >= 15)
            {
                PadOutRow(panelRow);

                // Create new row
                panelLayout.Add(panelRow);

                rowCount++;
                panelRow = new List<GameObject>(); // A row of panels in the layout
                i--; // Go back a step to add the current word to the row
            }
        }

        PadOutRow(panelRow);

        // Add last word
        panelLayout.Add(panelRow);
        rowCount++;

        // Add a blank row above and below the layout to create a border
        for (int i = 0; i < 2; i++)
        {
            panelRow = new List<GameObject>();

            for (int j = 0; j < 15; j++)
            {
                GameObject blankPanel = Instantiate(letterPrefab);
                blankPanel.GetComponentInChildren<TextMeshProUGUI>().text = "";
                blankPanel.GetComponent<Image>().color = Color.green;

                panelRow.Add(blankPanel);
            }

            if (i == 0)
            {
                panelLayout.Add(panelRow);
            }
            else
            {
                panelLayout.Insert(0, panelRow);
            }
        }
    }

    void PadOutRow(List<GameObject> panelRow)
    {
        // These +/-1 is account for the extra white space after the word
        int numPadding = 14 - (panelRow.Count - 1);
        int padLeft = (int)(numPadding * 0.5f) + 1;
        int padRight = numPadding - padLeft;

        for (int j = 0; j < padLeft; j++)
        {
            GameObject blankPanel = Instantiate(letterPrefab);
            blankPanel.GetComponentInChildren<TextMeshProUGUI>().text = "";
            blankPanel.GetComponent<Image>().color = Color.green;

            panelRow.Insert(0, blankPanel);
        }

        for (int k = 0; k < padRight; k++)
        {
            GameObject blankPanel = Instantiate(letterPrefab);
            blankPanel.GetComponentInChildren<TextMeshProUGUI>().text = "";
            blankPanel.GetComponent<Image>().color = Color.green;

            panelRow.Add(blankPanel);
        }
    }

    // The panels are displayed in the order they are parented
    // Parent the panels after setting them up
    void PresentPanel()
    {
        foreach (List<GameObject> row in panelLayout)
        {
            foreach(GameObject panel in row)
            {
                panel.transform.parent = transform;
            }
        }
    }
}
