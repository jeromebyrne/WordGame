using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class LetterBag
{
    List<SingleLetterInfo> _letters = new List<SingleLetterInfo>();

    private static readonly System.Random _random = new System.Random();

    public void AddAllLetters()
    {
        var letterConfig = WordConfigManager.LetterConfig;

        var letterList = letterConfig.GetAllLetters();

        foreach (var letter in letterList)
        {
            for (int i = 0; i < letter._wordBagCount; i++)
            {
                _letters.Add(letter);
            }
        }

        Debug.Log("LetterBag count is: " + _letters.Count.ToString());
    }

    public void ClearAllLetters()
    {
        _letters = new List<SingleLetterInfo>();
    }

    public bool HasLetterToPick()
    {
        return _letters.Count > 0;
    }

    public SingleLetterInfo PickRandomLetter()
    {
        int randomIndex = _random.Next(_letters.Count);

        SingleLetterInfo letterInfo = _letters[randomIndex];

        // Remove so it doesn't get picked again
        _letters.RemoveAt(randomIndex);

        return letterInfo;
    }

}
