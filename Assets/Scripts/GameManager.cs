using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private PlayerState _playerOneState = new PlayerState(1);
    private PlayerState _playerTwoState = new PlayerState(2);
    private LetterBag _letterBag = new LetterBag();
    [SerializeField] private UILetterTileHolder _playerTileHolder;

    void Start()
    {
        _letterBag.AddAllLetters();

        _playerTileHolder.Init();
        AssignInitialLettersToPlayers();
    }

    void Update()
    {
    }

    void AssignInitialLettersToPlayers()
    {
        // assumption is 2 players per game
        int numLettersPerPlayer = GameSettingsConfigManager.GameSettings._maxPlayerLetters;
        for (int i = 0; i < numLettersPerPlayer * 2; i++)
        {
            if (i % 2 == 0)
            {
                // player 1
                SingleLetterInfo randomLetter = _letterBag.PickRandomLetter();
                _playerOneState.AssignLetter(randomLetter);

                var evt = PlayerLetterAssigned.Get(_playerOneState, randomLetter);
                GameEventHandler.Instance.TriggerEvent(evt);

            }
            else
            {
                // player 2
                SingleLetterInfo randomLetter = _letterBag.PickRandomLetter();
                _playerTwoState.AssignLetter(randomLetter);

                var evt = PlayerLetterAssigned.Get(_playerTwoState, randomLetter);
                GameEventHandler.Instance.TriggerEvent(evt);
            }
        }
    }
}
