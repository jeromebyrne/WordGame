using UnityEngine;

public class BoardVisual : MonoBehaviour
{
    [SerializeField] SpriteRenderer _boardSprite;

    public SpriteRenderer BoardSpriteRenderer { get { return _boardSprite; } }

    public void SetGridDimensions(Vector2Int dimensions)
    {
        Debug.Assert(_boardSprite != null, "_boardSprite is null. Returning.");

        if (_boardSprite == null)
        {
            return;
        }

        _boardSprite.size = dimensions;
    }
}
