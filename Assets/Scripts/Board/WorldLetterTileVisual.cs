using UnityEngine;
using TMPro;

public class WorldLetterTileVisual : MonoBehaviour
{
    [SerializeField] private TMP_Text _letterText = null;
    [SerializeField] private SpriteRenderer _spriteRenderer = null;

    public Vector2Int GridIndex { get; private set; }

    public void Populate(int gridIndexX, int gridIndexY, string letterDisplay)
    {
        GridIndex = new Vector2Int(gridIndexX, gridIndexY);
        _letterText.text = letterDisplay;
    }

    public Bounds VisualBounds
    {
        get
        {
            return _spriteRenderer.bounds;
        }
    }
}
