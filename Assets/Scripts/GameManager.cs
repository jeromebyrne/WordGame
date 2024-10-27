using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private PlayerState _playerOneState = new PlayerState(1);
    private PlayerState _playerTwoState = new PlayerState(2);
    private LetterBag _letterBag = new LetterBag();
    [SerializeField] private UILetterTileHolder _playerTileHolder;
    [SerializeField] private GameBoard _gameBoard;
    [SerializeField] private WorldTileDragHandler _worldTileDragHandler;
    [SerializeField] private BoardVisual _boardVisual;

    private PlayerState _currentPlayerState;

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

        _currentPlayerState = _playerOneState;
        GameEventHandler.Instance.TriggerEvent(ConfirmSwitchPlayerEvent.Get(-1, 1));

        // play background music
        GameEventHandler.Instance.TriggerEvent(PlayAudioEvent.Get("Audio/past_sadness", 1.0f, true, true));
    }

    void AssignInitialLettersToPlayers()
    {
        // assumption is 2 players per game
        int numLettersPerPlayer = GameSettingsConfigManager.GameSettings._maxPlayerLetters;
        for (int i = 0; i < numLettersPerPlayer * 2; i++)
        {
            if (i % 2 == 0)
            {
                AssignRandomLetterToPlayer(_playerOneState);
            }
            else
            {
                AssignRandomLetterToPlayer(_playerTwoState);
            }
        }
    }

    void AssignRandomLetterToPlayer(PlayerState playerState)
    {
        if (!_letterBag.HasLetterToPick())
        {
            return;
        }

        LetterDataObj randomLetter = _letterBag.PickRandomLetter();
        playerState.AssignLetter(randomLetter);

        var evt = PlayerLetterAssignedEvent.Get(playerState, randomLetter);
        GameEventHandler.Instance.TriggerEvent(evt);
    }

    void AssignFreshLettersToPlayer(PlayerState playerState)
    {
        if (!_letterBag.HasLetterToPick())
        {
            Debug.Log("The letter bag is exhausted");
            return;
        }

        int numLettersPerPlayer = GameSettingsConfigManager.GameSettings._maxPlayerLetters;
        for (int i = playerState.CurrentLetterCount; i < numLettersPerPlayer; i++)
        {
            AssignRandomLetterToPlayer(playerState);
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

        if (firstTurn)
        {
            // the player needs to occupy the center slot on the first turn
            if (!boardState.IsCenterTileOccupied())
            {
                Debug.Log("The center tile must be occupied on the first turn");
                return;
            }
        }
        else
        {
            // make sure newly placed tiles touch previously placed tiles
            bool connecting = BoardDataHelper.ArePlacedTilesConnectingWithCommittedTile(boardState, uncommittedTiles);
            if (!connecting)
            {
                Debug.Log("Placed tiles are not connecting with previously placed tiles");
                return;
            }
        }

        List<(string, int, List<BoardSlotIndex>)> wordAndScoreTupleList = BoardDataHelper.GetWordsAndScoresFromTiles(boardState, contiguousTiles);

        int wordCount = wordAndScoreTupleList.Count;

        foreach (var tup in wordAndScoreTupleList)
        {
            if (!WordConfigManager.IsValidWord(tup.Item1))
            {
                Debug.Log(tup.Item1 + " is NOT a valid word!");
                return;
            }
        }

        if (wordCount > 0)
        {
            // here we want to check that the words returned have a common BoardSlotIndex
            // Check if the words share a common BoardSlotIndex
            bool wordsShareCommonTile = BoardDataHelper.DoWordsShareCommonTile(wordAndScoreTupleList);
            if (!wordsShareCommonTile)
            {
                Debug.Log("Multi-Words do not share a common tile!");
                return;
            }
        }

        List<LetterDataObj> lettersToCommit = new List<LetterDataObj>(); // we use these to remove form the player state below

        foreach (BoardSlotIndex i in uncommittedTiles)
        {
            BoardSlotState slotState = boardState.GetSlotState(i);
            lettersToCommit.Add(slotState.OccupiedLetter);
        }

        _gameBoard.CommitTiles(uncommittedTiles, _currentPlayerState.PlayerIndex);

        foreach (var tup in wordAndScoreTupleList)
        {
            Debug.Log("Score for " + tup.Item1 + " is: " + tup.Item2.ToString());

            _currentPlayerState.AddScore(tup.Item2);
        }

        // remove the letters from the players state
        foreach (LetterDataObj l in lettersToCommit)
        {
            _currentPlayerState.RemoveLetter(l);
        }

        PlayerState nextPlayerState = _currentPlayerState.PlayerIndex == 1 ? _playerTwoState : _playerOneState;
        GameEventHandler.Instance.TriggerEvent(ConfirmSwitchPlayerEvent.Get(_currentPlayerState.PlayerIndex, nextPlayerState.PlayerIndex));

        AssignFreshLettersToPlayer(nextPlayerState);

        _currentPlayerState = nextPlayerState;
    }
}
