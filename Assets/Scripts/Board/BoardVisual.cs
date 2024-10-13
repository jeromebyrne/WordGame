using UnityEngine;
using System.Collections.Generic;

public class BoardVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _boardSprite;

    [SerializeField] private GameObject _letterTilePrefab;

    private List<SpriteRenderer> _letterTilesSprites = new List<SpriteRenderer>(); 

    private Vector2Int _cachedGridDimensions;

    public List<SpriteRenderer> GetTilesSpriteRenderers() { return _letterTilesSprites; }

    public Vector3 GetWorldPositionForGridIndex(Vector2Int gridIndex)
    {
        // Get the board's bounds
        Bounds boardBounds = _boardSprite.bounds;

        // Calculate half the board's width and height
        float boardHalfWidth = boardBounds.size.x * 0.5f;
        float boardHalfHeight = boardBounds.size.y * 0.5f;

        // Calculate the world position based on grid index and slot dimensions
        float worldX = (gridIndex.x * SlotWidth) - boardHalfWidth + SlotWidth * 0.5f /*+ boardBounds.min.x*/;
        float worldY = (gridIndex.y * SlotHeight) - boardHalfHeight + SlotHeight * 0.5f /*+ boardBounds.min.y*/;

        // Return the calculated world position
        return new Vector3(worldX, worldY, -2.0f); // TODO: cache these positions
    }

    public Bounds VisualBounds
    {
        get { return _boardSprite.bounds; }
    }

    public float SlotWidth
    {
        get
        {
            return _boardSprite.bounds.size.x / _cachedGridDimensions.x;
        }
    }

    public float SlotHeight
    {
        get
        {
            return _boardSprite.bounds.size.y / _cachedGridDimensions.y;
        }
    }

    public void SetGridDimensions(Vector2Int dimensions)
    {
        Debug.Assert(_boardSprite != null, "_boardSprite is null. Returning.");

        _cachedGridDimensions = dimensions;

        if (_boardSprite == null)
        {
            return;
        }

        _boardSprite.size = dimensions;
    }

    /*
    private void AddLetterTiles(Vector2Int dimensions)
    {
        // add a tile for every slot, we can turn them on and off
        // assumes SetGridDimensions has been called
        float boardHalfWidth = _boardSprite.bounds.size.x * 0.5f;
        float boardHalfHeight = _boardSprite.bounds.size.x * 0.5f;

        for (int i = 0; i < dimensions.x; i++)
        {
            _letterTiles.Add(new List<WorldLetterTileVisual>());

            for (int j = 0; j < dimensions.y; ++j)
            {
                var tile = CreateLetterTile(new Vector3((j * SlotWidth) - boardHalfWidth + SlotWidth * 0.5f,
                                            (i * SlotHeight) - boardHalfHeight + SlotHeight * 0.5f,
                                            -1.0f),
                                            j,
                                            i);

                _letterTiles[i].Add(tile);

                _letterTilesSprites.Add(tile.GetComponent<SpriteRenderer>());
            }
        }
    }
    */

    public WorldLetterTileVisual CreateLetterTile(LetterDataObj letterData, int playerIndex)
    {
        GameObject newInstance = Instantiate(_letterTilePrefab, Vector3.zero, Quaternion.identity, _boardSprite.gameObject.transform);

        WorldLetterTileVisual tileComponent = newInstance.GetComponent<WorldLetterTileVisual>();

        tileComponent.Populate(letterData, playerIndex);

        _letterTilesSprites.Add(newInstance.GetComponent<SpriteRenderer>());

        return tileComponent;
    }

    public bool IsWorldPositionIntersectingBoard(Vector3 worldPosition)
    {
        Bounds spriteBounds = _boardSprite.bounds;

        return spriteBounds.Contains(worldPosition);
    }

    public Vector2Int GetNearestSlotIndex(Vector3 worldPosition)
    {
        int rows = GameSettingsConfigManager.GameSettings._boardDimensions.x;
        int columns = GameSettingsConfigManager.GameSettings._boardDimensions.y;

        Bounds spriteBounds = _boardSprite.bounds;

        Vector3 localPosition = worldPosition - spriteBounds.min;

        int column = Mathf.FloorToInt(localPosition.x / SlotWidth);
        int row = Mathf.FloorToInt(localPosition.y / SlotHeight);

        if (column < 0 || column >= columns || row < 0 || row >= rows)
        {
            Debug.Log("Position is outside the sprite bounds.");
        }

        return new Vector2Int(column, row); // TODO: don't do new
    }
}
