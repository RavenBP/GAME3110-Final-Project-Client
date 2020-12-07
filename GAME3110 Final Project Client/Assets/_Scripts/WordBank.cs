using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordPack
{
    public string question;
    public string answer;
}

// Temporary Words Database
public class WordBank
{
    public List<WordPack> Words = new List<WordPack>(12);

    public void _InstantiateWorld()
    {
        for (int i = 0; i < Words.Capacity; i++)
        {
            Words.Add(new WordPack());
        }

        Words[0].question = "Canada's city";
        Words[0].answer = "Toronto";

        Words[1].question = "Currency";
        Words[1].answer = "Money";

        Words[2].question = "A continent";
        Words[2].answer = "America";

        Words[3].question = "Sweet spice";
        Words[3].answer = "Sugar";

        Words[4].question = "Popular material";
        Words[4].answer = "Wood";

        Words[5].question = "Money container";
        Words[5].answer = "Wallet";

        Words[6].question = "Robin's Sidekick";
        Words[6].answer = "Batman";

        Words[7].question = "Small rodent";
        Words[7].answer = "Mouse";

        Words[8].question = "Where you live";
        Words[8].answer = "House";

        Words[9].question = "A theory";
        Words[9].answer = "Big Bang";

        Words[10].question = "A movie";
        Words[10].answer = "The Lord Of The Rings";

        Words[11].question = "A book series";
        Words[11].answer = "Harry Potter";
    }
}
