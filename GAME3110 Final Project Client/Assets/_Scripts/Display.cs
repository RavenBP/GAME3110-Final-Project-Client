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

    // Panels
    public List<List<GameObject>> panelLayout; // How to display to players
    public List<GameObject> solutionPanels; // How the game sees the panels

    [SerializeField]
    private GameObject letterPrefab;

    public Guess guess;
    string[] wordList;

    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (int index in charToIndexDict[char.ToUpper(guess.guess)])
        {
            solutionPanels[index].GetComponentInChildren<TextMeshProUGUI>().enabled = true;
        }
    }

    private void Setup()
    {
        // Setting up arrays
        originalPhrase = originalPhrase.ToUpper();
        alphabet = alphabet.ToUpper();

        solution = originalPhrase.ToCharArray();
        guesses = new char[solution.Length];
        solutionPanels = new List<GameObject>();
        panelLayout = new List<List<GameObject>>();
        charToIndexDict = new Dictionary<char, List<int>>();

        InitPanelLayout();
        PresentPanel();

        foreach (GameObject panel in solutionPanels)
        {
            Debug.Log("After " + panel.GetComponentInChildren<TextMeshProUGUI>().text);
        }

        // Init dictionary
        foreach (char letter in alphabet)
        {
            charToIndexDict[letter] = new List<int>();
        }

        wordList = originalPhrase.Split();
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

    void InitPanelLayout()
    {
        wordList = originalPhrase.Split();
        List<List<GameObject>> wordPanels = new List<List<GameObject>>();

        // Init Panels
        // Create panel references from game logic to onscreen display
        foreach (string word in wordList)
        {
            List<GameObject> wordPanel = new List<GameObject>(); // A list of panels representing the word

            foreach (char letter in word)
            {
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

        //foreach (string word in wordList)
        //{
        //    Debug.Log(word);
        //}

        //foreach (List<GameObject> wordPanel in wordPanels)
        //{
        //    foreach (GameObject panel in wordPanel)
        //    {
        //        Debug.Log(panel.GetComponent<TextMeshProUGUI>().text);
        //    }
        //}

        //foreach (List<GameObject> wordPanel in wordPanels)
        //{
            foreach (GameObject panel in solutionPanels)
            {
                Debug.Log("Before " + panel.GetComponentInChildren<TextMeshProUGUI>().text);
            }
        //}

        // Format layout for player by adding padding
        int rowCount = 1;
        List<GameObject> panelRow = new List<GameObject>(); // A row of panels in the layout

        for (int i = 0; i < wordPanels.Count; i++)
        {
            // Check if row can fit more words
            if (panelRow.Count + wordPanels[i].Count < 15)
            {
                panelRow.AddRange(wordPanels[i]);
            }
            // Row filled - pad the rest of the row
            else if (panelRow.Count + wordPanels[i].Count >= 15)
            {
                PadOutRow(panelRow);

                // Create new row
                //List<GameObject> copy = new List<GameObject>(panelRow);
                panelLayout.Add(panelRow);

                rowCount++;
                panelRow = new List<GameObject>(); // A row of panels in the layout
                i--; // Go back a step
            }
        }

        PadOutRow(panelRow);

        // Add last word
        panelLayout.Add(panelRow);
        rowCount++;

        //foreach (List<GameObject> row in panelLayout)
        //{
        //    foreach (GameObject panel in row)
        //    {
        //        Debug.Log(panel.GetComponent<TextMeshProUGUI>().text);
        //    }
        //}

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
