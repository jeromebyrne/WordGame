using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    private PlayerState _playerOneState = new PlayerState(1);
    private PlayerState _playerTwoState = new PlayerState(2);
    private LetterBag _letterBag = new LetterBag();
    [SerializeField] private UILetterTileHolder _playerTileHolder;
    [SerializeField] private GameBoard _gameBoard;
    [SerializeField] private WorldTileDragHandler _worldTileDragHandler;
    [SerializeField] private BoardVisual _boardVisual;

    private int _currentPlayerIndex = 1;

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

        var boardDimensions = GameSettingsConfigManager.GameSettings._boardDimensions;
        _boardVisual.SetGridDimensions(boardDimensions);

        _gameBoard.Init();

        _worldTileDragHandler.Init();

        _currentPlayerIndex = 1;
        GameEventHandler.Instance.TriggerEvent(ConfirmSwitchPlayerEvent.Get(-1, 1));
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
                LetterDataObj randomLetter = _letterBag.PickRandomLetter();
                _playerOneState.AssignLetter(randomLetter);

                var evt = PlayerLetterAssignedEvent.Get(_playerOneState, randomLetter);
                GameEventHandler.Instance.TriggerEvent(evt);
            }
            else
            {
                // player 2
                LetterDataObj randomLetter = _letterBag.PickRandomLetter();
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

        List<BoardSlotIndex> contiguousTiles = new List<BoardSlotIndex>();

        if (!BoardDataHelper.AreTilesContiguous(uncommittedTiles, boardState, out contiguousTiles))
        {
            Debug.Log("Placed tiles are not contiguous!");
            return;
        }

        Debug.Log("Placed tiles are contiguous!");

        // if there are 0 committed tiles it means it's the first turn
        bool firstTurn = boardState.GetCommittedTileCount() == 0;

        if (!firstTurn)
        {
            bool connecting = BoardDataHelper.ArePlacedTilesConnectingWithCommittedTile(boardState, uncommittedTiles);
            if (!connecting)
            {
                Debug.Log("Placed tiles are not connecting with previously placed tiles");
                return;
            }
        }
        
        var wordAndScoreTuple = BoardDataHelper.GetWordAndScoreFromTiles(boardState, contiguousTiles);

        if (!WordConfigManager.IsValidWord(wordAndScoreTuple.word))
        {
            Debug.Log(wordAndScoreTuple.word + " is NOT a valid word!");
            return;
        }

        Debug.Log(wordAndScoreTuple.word + " is a valid word!");

        Debug.Log("Score for " + wordAndScoreTuple.word + " is: " + wordAndScoreTuple.score.ToString());

        _gameBoard.CommitTiles(uncommittedTiles);

        int nextPlayerIndex = _currentPlayerIndex == 1 ? 2 : 1;
        GameEventHandler.Instance.TriggerEvent(ConfirmSwitchPlayerEvent.Get(_currentPlayerIndex, nextPlayerIndex));

        _currentPlayerIndex = nextPlayerIndex;
    }
}
