using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private List<PlayerState> _playerStates = new List<PlayerState>();
    private LetterBag _letterBag = new LetterBag();
    [SerializeField] private UILetterTileHolder _playerTileHolder;
    [SerializeField] private GameBoard _gameBoard;
    [SerializeField] private WorldTileDragHandler _worldTileDragHandler;
    [SerializeField] private BoardVisual _boardVisual;
    [SerializeField] private List<Color> _playerColors;

    private PlayerState _currentPlayerState;
    private const float kMaxTurnTimeSeconds = 90.0f;
    private const float kTurnCountdownTime = 30.0f;
    private float _currentTurnTimeSeconds = 0.0f;
    private bool _hasTriggeredCountdown = false;

    bool _isGameOver = false;

    private void OnEnable()
    {
        GameEventHandler.Instance.Subscribe<UIPlayButtonPressedEvent>(OnAttemptPlayTurn);
        GameEventHandler.Instance.Subscribe<PassTurnEvent>(OnPassTurnEvent);
        GameEventHandler.Instance.Subscribe<GameOverEvent>(OnGameOverEvent);
        GameEventHandler.Instance.Subscribe<RestartGameEvent>(OnRestartGameEvent);
    }

    private void OnDisable()
    {
        GameEventHandler.Instance.Unsubscribe<UIPlayButtonPressedEvent>(OnAttemptPlayTurn);
        GameEventHandler.Instance.Unsubscribe<PassTurnEvent>(OnPassTurnEvent);
        GameEventHandler.Instance.Unsubscribe<GameOverEvent>(OnGameOverEvent);
        GameEventHandler.Instance.Unsubscribe<RestartGameEvent>(OnRestartGameEvent);
    }

    void Start()
    {
        // add players
        _playerStates.Add(new PlayerState(1));
        _playerStates.Add(new PlayerState(2));

        _letterBag.AddAllLetters();

        _playerTileHolder.Init();
        AssignInitialLettersToPlayers();

        var boardDimensions = PlayerSettings.GetBoardDimensions();
        _boardVisual.SetGridDimensions(boardDimensions);

        _gameBoard.Init();

        _worldTileDragHandler.Init();

        _currentPlayerState = _playerStates.First();

        // send a color set event for elements that are colored based on current player
        // Note: need to send this before ConfirmSwitchPlayerEvent
        foreach (PlayerState playerState in _playerStates)
        {
            GameEventHandler.Instance.TriggerEvent(PlayerColorSetEvent.Get(playerState.PlayerIndex, _playerColors[playerState.PlayerIndex - 1]));
        }

        GameEventHandler.Instance.TriggerEvent(ConfirmSwitchPlayerEvent.Get(-1, 1));

        // play background music
        GameEventHandler.Instance.TriggerEvent(PlayAudioEvent.Get("Audio/past_sadness", 1.0f, true, true));
    }

    void AssignInitialLettersToPlayers()
    {
        int numLettersPerPlayer = GameSettingsConfigManager.GameSettings._maxPlayerLetters;
        int totalLetters = numLettersPerPlayer * _playerStates.Count;

        for (int i = 0; i < totalLetters; i++)
        {
            // Assign letter to the player in round-robin fashion
            PlayerState currentPlayerState = _playerStates[i % _playerStates.Count];
            AssignRandomLetterToPlayer(currentPlayerState);
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

    private void OnPassTurnEvent(PassTurnEvent evt)
    {
        PassTurn();
    }

    private void PassTurn()
    {
        // skip the turn (no points)
        GameEventHandler.Instance.TriggerEvent(ReturnAllUncommittedTilesToHolderEvent.Get(_currentPlayerState.PlayerIndex));

        GameEventHandler.Instance.TriggerEvent(PlayAudioEvent.Get("Audio/pass", 1.0f, false, false));

        _currentPlayerState.ConsecutivePasses += 1;

        bool isGameOver = true;
        foreach (PlayerState playerState in _playerStates)
        {
            if (playerState.ConsecutivePasses < 2)
            {
                isGameOver = false;
                break;
            }
        }

        if (isGameOver)
        {
            GameEventHandler.Instance.TriggerEvent(GameOverEvent.Get());
            return;
        }

        SwitchToNextPlayerTurn();
    }

    private void OnAttemptPlayTurn(UIPlayButtonPressedEvent evt)
    {
        if (_isGameOver)
        {
            return;
        }

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

        var contigTuple = BoardDataHelper.AreTilesContiguous(uncommittedTiles, boardState, out contiguousTiles);
        if (!contigTuple.Item1)
        {
            GameEventHandler.Instance.TriggerEvent(DisplayMessageBubbleEvent.Get(contigTuple.Item2));
            PlayErrorAudio();
            return;
        }

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
            // TODO: I don't think I need this check anymore
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

            if (tup.Item2 > _currentPlayerState.HighestWordScore)
            {
                _currentPlayerState.HighestWordScore = tup.Item2;
                _currentPlayerState.HighestScoringWord = tup.Item1;
            }

            _currentPlayerState.AddScore(tup.Item2);
        }

        // remove the letters from the players state
        foreach (LetterDataObj l in lettersToCommit)
        {
            _currentPlayerState.RemoveLetter(l);
        }

        // reset consecutive passes
        _currentPlayerState.ConsecutivePasses = 0;

        SwitchToNextPlayerTurn();

        // play tiles committed successfully sfx
        GameEventHandler.Instance.TriggerEvent(PlayAudioEvent.Get("Audio/tiles_committed", 1.0f, false, false));
    }

    private void SwitchToNextPlayerTurn()
    {
        int playerCount = _playerStates.Count;

        int nextPlayerIndex = _currentPlayerState.PlayerIndex == playerCount ? 1 : _currentPlayerState.PlayerIndex + 1;

        PlayerState nextPlayerState = _playerStates[nextPlayerIndex -1];
        GameEventHandler.Instance.TriggerEvent(ConfirmSwitchPlayerEvent.Get(_currentPlayerState.PlayerIndex, nextPlayerState.PlayerIndex));

        AssignFreshLettersToPlayer(nextPlayerState);

        // stop the countdown if it's active
        _currentTurnTimeSeconds = 0.0f;
        _hasTriggeredCountdown = false;
        GameEventHandler.Instance.TriggerEvent(StopTurnCountdownTimerEvent.Get());
        GameEventHandler.Instance.TriggerEvent(StopAudioEvent.Get("Audio/clock"));

        if (nextPlayerState.CurrentLetterCount < 1)
        {
            // a player has no tiles left, this means game over
            GameEventHandler.Instance.TriggerEvent(GameOverEvent.Get());
            return;
        }

        _currentPlayerState = nextPlayerState;
    }

    private void PlayErrorAudio()
    {
        GameEventHandler.Instance.TriggerEvent(PlayAudioEvent.Get("Audio/error", 1.0f, false, false));
    }

    private void Update()
    {
        if (_isGameOver)
        {
            return;
        }

        if (!_hasTriggeredCountdown && _currentTurnTimeSeconds > (kMaxTurnTimeSeconds - kTurnCountdownTime))
        {
            GameEventHandler.Instance.TriggerEvent(StartTurnCountdownTimerEvent.Get(kTurnCountdownTime));
            _hasTriggeredCountdown = true;

            GameEventHandler.Instance.TriggerEvent(PlayAudioEvent.Get("Audio/clock", 1.0f, true, false));
        }

        _currentTurnTimeSeconds += Time.deltaTime;

        if (_currentTurnTimeSeconds > kMaxTurnTimeSeconds)
        {
            GameEventHandler.Instance.TriggerEvent(PassTurnEvent.Get());
        }
    }

    private void GameOver()
    {
        _isGameOver = true;

        SceneManager.LoadSceneAsync("Assets/Scenes/GameOverScene.unity", LoadSceneMode.Additive).completed += OnGameOverSceneLoaded;
    }

    private void OnGameOverSceneLoaded(AsyncOperation obj)
    {
        if (obj.isDone)
        {
            Debug.Log("Game Over scene loaded successfully!");
        }
        else
        {
            Debug.LogError("Failed to load scene.");
            return;
        }

        // Access the loaded scene by name
        Scene gameOverScene = SceneManager.GetSceneByName("GameOverScene"); // Replace with your actual scene name
        if (gameOverScene.isLoaded)
        {
            GameObject[] rootObjects = gameOverScene.GetRootGameObjects();

            // Find the GameOver component in the scene
            GameOver gameOverComponent = rootObjects
                .SelectMany(root => root.GetComponentsInChildren<GameOver>())
                .FirstOrDefault();

            if (gameOverComponent != null)
            {
                gameOverComponent.Populate(_playerStates, _playerColors);
            }
            else
            {
                Debug.LogError("GameOver component not found in the loaded scene.");
            }
        }
        else
        {
            Debug.LogError("GameOver scene is not loaded.");
        }
    }

    private void OnGameOverEvent(GameOverEvent evt)
    {
        if (_isGameOver)
        {
            return;
        }

        GameOver();
    }

    private void OnRestartGameEvent(RestartGameEvent evt)
    {
        if (!_isGameOver)
        {
            // we shouldn't be trying to restart a game if the current game is not over
            return;
        }

        // TODO: do we want a callback?
        SceneManager.LoadSceneAsync("Assets/Scenes/GameStart.unity", LoadSceneMode.Single);
    }
}
