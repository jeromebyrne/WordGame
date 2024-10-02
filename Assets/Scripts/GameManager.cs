using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private PlayerState _playerOneState = new PlayerState(1);
    private PlayerState _playerTwoState = new PlayerState(2);
    private LetterBag _letterBag = new LetterBag();
    [SerializeField] private UILetterTileHolder _playerTileHolder;
    [SerializeField] private GameBoard _gameBoard;

    private void OnEnable()
    {
        GameEventHandler.Instance.Subscribe<UIPlayButtonPressedEvent>(OnAttemptPlayTurn);
    }

    private void OnDisable()
    {
        GameEventHandler.Instance.Unsubscribe<UIPlayButtonPressedEvent>(OnAttemptPlayTurn);
    }

    void Start()
    {
        _letterBag.AddAllLetters();

        _playerTileHolder.Init();
        AssignInitialLettersToPlayers();

        _gameBoard.Init();
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

                var evt = PlayerLetterAssignedEvent.Get(_playerOneState, randomLetter);
                GameEventHandler.Instance.TriggerEvent(evt);

            }
            else
            {
                // player 2
                SingleLetterInfo randomLetter = _letterBag.PickRandomLetter();
                _playerTwoState.AssignLetter(randomLetter);

                var evt = PlayerLetterAssignedEvent.Get(_playerTwoState, randomLetter);
                GameEventHandler.Instance.TriggerEvent(evt);
            }
        }
    }

    private void OnAttemptPlayTurn(UIPlayButtonPressedEvent evt)
    {
        // check if we can play this turn

        var boardState = _gameBoard.GetBoardState();

        var uncommittedTiles = BoardDataHelper.GetUncommittedTiles(boardState);

        List<Vector2Int> contiguousTiles = new List<Vector2Int>();

        if (!BoardDataHelper.AreTilesContiguous(uncommittedTiles, boardState, out contiguousTiles))
        {
            Debug.Log("Tiles are not contiguous!");
            return;
        }

        Debug.Log("Tiles are contiguous!");

        string word = BoardDataHelper.GetWordFromTiles(boardState, contiguousTiles);

        if (!WordConfigManager.IsValidWord(word))
        {
            Debug.Log(word + " is NOT a valid word!");
            return;
        }

        Debug.Log(word + " is a valid word!");

        int score = BoardDataHelper.GetScoreFromTiles(boardState, contiguousTiles);

        Debug.Log("Score for " + word + " is: " + score.ToString());
    }
}
