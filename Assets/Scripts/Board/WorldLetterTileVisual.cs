using UnityEngine;
using TMPro;

public class WorldLetterTileVisual : MonoBehaviour
{
    [SerializeField] private TMP_Text _letterText = null;
    [SerializeField] private TMP_Text _scoreText = null;
    [SerializeField] private SpriteRenderer _spriteRenderer = null;

    public BoardSlotIndex GridIndex { get; private set; }
    public int PlayerIndex { get; set; }
    public bool IsLocked { get; private set; }

    public LetterDataObj LetterData { get; private set; }

    public SpriteRenderer SpriteRenderer { get { return _spriteRenderer; } }

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

        foreach (var index in evt.CommittedTiles)
        {
            if (index == GridIndex)
            {
                IsLocked = true;
                return;
            }
        }
    }
}
