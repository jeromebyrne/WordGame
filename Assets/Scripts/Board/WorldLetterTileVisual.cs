using UnityEngine;
using TMPro;

public class WorldLetterTileVisual : MonoBehaviour
{
    [SerializeField] private TMP_Text _letterText = null;
    [SerializeField] private TMP_Text _scoreText = null;
    [SerializeField] private SpriteRenderer _spriteRenderer = null;

    public Vector2Int GridIndex { get; private set; }
    public int PlayerIndex { get; set; }

    public LetterDataObj LetterData { get; private set; }

    public SpriteRenderer SpriteRenderer { get { return _spriteRenderer; } }

    public void SetGridIndex(Vector2Int gridIndex)
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
}
