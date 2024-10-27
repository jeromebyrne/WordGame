using System.Collections.Generic;
using UnityEngine;

public class LetterBag
{
    List<LetterDataObj> _letters = new List<LetterDataObj>();

    private static readonly System.Random _random = new System.Random();

    public void AddAllLetters()
    {
        var letterConfig = WordConfigManager.LetterConfig;

        var letterList = letterConfig.GetAllLetters();

        foreach (var letter in letterList)
        {
            for (int i = 0; i < letter._wordBagCount; i++)
            {
                _letters.Add(new LetterDataObj(letter));
            }
        }

        Debug.Log("LetterBag count is: " + _letters.Count.ToString());
    }

    public void ClearAllLetters()
    {
        _letters = new List<LetterDataObj>();
    }

    public bool HasLetterToPick()
    {
        return _letters.Count > 0;
    }

    public LetterDataObj PickRandomLetter()
    {
        // TODO got a crash out of range here.
        int randomIndex = _random.Next(_letters.Count);

        LetterDataObj letterInfo = _letters[randomIndex];

        // Remove so it doesn't get picked again
        _letters.RemoveAt(randomIndex);

        Debug.Log("LetterBag count is: " + _letters.Count.ToString());

        return letterInfo;
    }

}
