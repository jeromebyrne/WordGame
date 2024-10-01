using UnityEngine;
using TMPro;

public class WorldLetterTileVisual : MonoBehaviour
{
    [SerializeField] private TMP_Text _letterText = null;
    [SerializeField] private TMP_Text _scoreText = null;
    [SerializeField] private SpriteRenderer _spriteRenderer = null;

    public Vector2Int GridIndex { get; private set; }

    public void Populate(int gridIndexX, int gridIndexY, char letter, int score)
    {
        GridIndex = new Vector2Int(gridIndexX, gridIndexY);
        _letterText.text = letter.ToString();
        _scoreText.text = score.ToString();
    }

    public void UpdateVisual(char letter, int score)
    {
        _letterText.text = letter.ToString().ToUpper();
        _scoreText.text = score.ToString();
    }

    public Bounds VisualBounds
    {
        get
        {
            return _spriteRenderer.bounds;
        }
    }
}
