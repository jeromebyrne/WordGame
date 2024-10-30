using System.Collections.Generic;
using UnityEditor.VersionControl;
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
    private const float kMaxTurnTimeSeconds = 120.0f;
    private const float kTurnCountdownTime = 30.0f;
    private float _currentTurnTimeSeconds = 0.0f;
    private bool _hasTriggeredCountdown = false;

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

        GameEventHandler.Instance.TriggerEvent(PlayerLetterAssignedEvent.Get(playerState, randomLetter));
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

        if (uncommittedTiles.Count < 1)
        {
            string message = "Drag letter tiles from the bottom of the screen onto the game board";
            GameEventHandler.Instance.TriggerEvent(DisplayMessageBubbleEvent.Get(message));
            PlayErrorAudio();
            return;
        }

        List<BoardSlotIndex> contiguousTiles = new List<BoardSlotIndex>();

        if (!BoardDataHelper.AreTilesContiguous(uncommittedTiles, boardState, out contiguousTiles))
        {
            string message = "Letter tiles must be placed contiguously, vertically or horizontally";
            GameEventHandler.Instance.TriggerEvent(DisplayMessageBubbleEvent.Get(message));
            PlayErrorAudio();
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
                string message = "You must place a letter tile on the center red square on the first move";
                GameEventHandler.Instance.TriggerEvent(DisplayMessageBubbleEvent.Get(message));
                PlayErrorAudio();
                return;
            }

            if (uncommittedTiles.Count < 2)
            {
                string message = "Words must consist of at least 2 letters";
                GameEventHandler.Instance.TriggerEvent(DisplayMessageBubbleEvent.Get(message));
                PlayErrorAudio();
                return;
            }
        }
        else
        {
            // make sure newly placed tiles touch previously placed tiles
            bool connecting = BoardDataHelper.ArePlacedTilesConnectingWithCommittedTile(boardState, uncommittedTiles);
            if (!connecting)
            {
                string message = "Placed letter tiles are not connecting with previously placed tiles";
                GameEventHandler.Instance.TriggerEvent(DisplayMessageBubbleEvent.Get(message));
                PlayErrorAudio();
                return;
            }
        }

        List<(string, int, List<BoardSlotIndex>)> wordAndScoreTupleList = BoardDataHelper.GetWordsAndScoresFromTiles(boardState, contiguousTiles);

        int wordCount = wordAndScoreTupleList.Count;

        foreach (var tup in wordAndScoreTupleList)
        {
            if (!WordConfigManager.IsValidWord(tup.Item1))
            {
                string message = "\"" + tup.Item1.ToUpper() + "\"" + " is not a supported word";
                GameEventHandler.Instance.TriggerEvent(DisplayMessageBubbleEvent.Get(message));
                PlayErrorAudio();
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
                string message = "Multi-Words do not share a common letter tile";
                GameEventHandler.Instance.TriggerEvent(DisplayMessageBubbleEvent.Get(message));
                PlayErrorAudio();
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

        SwitchToNextPlayerTurn();

        // play tiles committed successfully sfx
        GameEventHandler.Instance.TriggerEvent(PlayAudioEvent.Get("Audio/tiles_committed", 1.0f, false, false));
    }

    private void SwitchToNextPlayerTurn()
    {
        PlayerState nextPlayerState = _currentPlayerState.PlayerIndex == 1 ? _playerTwoState : _playerOneState;
        GameEventHandler.Instance.TriggerEvent(ConfirmSwitchPlayerEvent.Get(_currentPlayerState.PlayerIndex, nextPlayerState.PlayerIndex));

        AssignFreshLettersToPlayer(nextPlayerState);

        _currentPlayerState = nextPlayerState;

        // stop the countdown if it's active
        _currentTurnTimeSeconds = 0.0f;
        _hasTriggeredCountdown = false;
        GameEventHandler.Instance.TriggerEvent(StopTurnCountdownTimerEvent.Get());
        GameEventHandler.Instance.TriggerEvent(StopAudioEvent.Get("Audio/clock"));
    }

    private void PlayErrorAudio()
    {
        GameEventHandler.Instance.TriggerEvent(PlayAudioEvent.Get("Audio/error", 1.0f, false, false));
    }

    private void Update()
    {
        if (!_hasTriggeredCountdown && _currentTurnTimeSeconds > (kMaxTurnTimeSeconds - kTurnCountdownTime))
        {
            GameEventHandler.Instance.TriggerEvent(StartTurnCountdownTimerEvent.Get(kTurnCountdownTime));
            _hasTriggeredCountdown = true;

            GameEventHandler.Instance.TriggerEvent(PlayAudioEvent.Get("Audio/clock", 1.0f, true, false));
        }

        _currentTurnTimeSeconds += Time.deltaTime;

        if (_currentTurnTimeSeconds > kMaxTurnTimeSeconds)
        {
            // skip the turn (no points)
            GameEventHandler.Instance.TriggerEvent(ReturnAllUncommittedTilesToHolderEvent.Get(_currentPlayerState.PlayerIndex));

            SwitchToNextPlayerTurn();
        }
    }
}
