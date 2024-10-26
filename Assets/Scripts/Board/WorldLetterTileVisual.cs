using UnityEngine;
using TMPro;

public class WorldLetterTileVisual : MonoBehaviour
{
    [SerializeField] private TMP_Text _letterText = null;
    [SerializeField] private TMP_Text _scoreText = null;
    [SerializeField] private SpriteRenderer _spriteRenderer = null;

    private Color _initColor;

    public BoardSlotIndex GridIndex { get; private set; }
    public int PlayerIndex { get; set; }
    public bool IsLocked { get; private set; }

    public LetterDataObj LetterData { get; private set; }

    public SpriteRenderer SpriteRenderer { get { return _spriteRenderer; } }

    private void Start()
    {
        // here the tile have only been placed,
        // discolor it so it stands out against already committed tiles
        _spriteRenderer.color = Color.gray;
        _initColor = _scoreText.color;
        _scoreText.color = Color.white;
        _letterText.color = Color.white;
    }

    private void OnEnable()
    {
        GameEventHandler.Instance.Subscribe<TilesCommittedEvent>(OnTilesCommitted);
    }

    private void OnDisable()
    {
        GameEventHandler.Instance.Unsubscribe<TilesCommittedEvent>(OnTilesCommitted);
    }

    public void SetGridIndex(BoardSlotIndex gridIndex)
    {
        GridIndex = gridIndex;
    }

    public void Populate(LetterDataObj letterData, int playerIndex)
    {
        _letterText.text = letterData.Character.ToString().ToUpper();
        _scoreText.text = letterData.Score.ToString();
        PlayerIndex = playerIndex;

        LetterData = letterData;
    }

    private void OnTilesCommitted(TilesCommittedEvent evt)
    {
        if (IsLocked)
        {
            // we don't care because we have already been locked
            return;
        }

        foreach (var index in evt.CommittedTileIndices)
        {
            if (index == GridIndex)
            {
                IsLocked = true;
                // TODO: fix this, these can be null somehow
                if (_spriteRenderer) _spriteRenderer.color = Color.white;
                if (_letterText) _letterText.color = _initColor;
                if (_scoreText) _scoreText.color = _initColor;

                return;
            }
        }
    }
}
