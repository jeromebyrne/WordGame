using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private PlayerState _playerOneState = new PlayerState(1);
    private PlayerState _playerTwoState = new PlayerState(2);
    private LetterBag _letterBag = new LetterBag();

    private const int kMaxLettersPerPlayer = 7;

    void Start()
    {
        _letterBag.AddAllLetters();

        AssignInitialLettersToPlayers();
    }

    void Update()
    {
    }

    void AssignInitialLettersToPlayers()
    {
        // assumption is 2 players per game
        for (int i = 0; i < kMaxLettersPerPlayer * 2; i++)
        {
            if (i % 2 == 0)
            {
                // player 1
                SingleLetterInfo randomLetter = _letterBag.PickRandomLetter();
                _playerOneState.AssignLetter(randomLetter);

            }
            else
            {
                // player 2
                SingleLetterInfo randomLetter = _letterBag.PickRandomLetter();
                _playerTwoState.AssignLetter(randomLetter);
            }
        }

        // post for player 1
        var evt = PlayerLettersAssigned.Get(_playerOneState);
        GameEventHandler.Instance.TriggerEvent(evt);

        // post for player 2
        evt = PlayerLettersAssigned.Get(_playerTwoState);
        GameEventHandler.Instance.TriggerEvent(evt);
    }
}
