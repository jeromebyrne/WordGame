using UnityEngine;
using System.Collections.Generic;

public class BoardVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _boardSprite;

    [SerializeField] private GameObject _letterTilePrefab;

    private List<List<WorldLetterTileVisual>> _letterTiles = new List<List<WorldLetterTileVisual>>();

    private Vector2Int _cachedGridDimensions;

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

        AddLetterTiles(dimensions);
    }

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
                                            new SingleLetterInfo(),
                                            j,
                                            i);

                _letterTiles[i].Add(tile);
            }
        }
    }

    WorldLetterTileVisual CreateLetterTile(Vector3 position, SingleLetterInfo letterInfo, int gridIndexX, int gridIndexY)
    {
        GameObject newInstance = Instantiate(_letterTilePrefab, position, Quaternion.identity, _boardSprite.gameObject.transform);

        WorldLetterTileVisual tileComponent = newInstance.GetComponent<WorldLetterTileVisual>();

        tileComponent.Populate(gridIndexX, gridIndexY, letterInfo._letter, letterInfo._points);

        newInstance.SetActive(false);

        return tileComponent;
    }

    public void EnableTile(int x, int y, SingleLetterInfo letterInfo)
    {
        var obj = _letterTiles[x][y].gameObject;
        obj.SetActive(true);

        WorldLetterTileVisual tileComponent = obj.GetComponent<WorldLetterTileVisual>();
        tileComponent.UpdateVisual(letterInfo._letter, letterInfo._points);
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
        else
        {
            int index = row * columns + column;
            Debug.Log($"World position corresponds to tile index: {index} (row: {row}, column: {column})");
        }

        return new Vector2Int(row, column); // TODO: don't do new
    }
}
