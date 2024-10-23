using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class BoardVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _boardSprite;
    [SerializeField] private GameObject _letterTilePrefab;
    [SerializeField] private GameObject _bonusTilePrefab;

    // the int is the letter id
    private Dictionary<uint, SpriteRenderer> _letterTilesSpritesMap = new Dictionary<uint, SpriteRenderer>(); 

    private Vector2Int _cachedGridDimensions;

    public Dictionary<uint, SpriteRenderer> GetTilesSpriteRenderers() { return _letterTilesSpritesMap; }

    public Vector3 GetWorldPositionForGridIndex(BoardSlotIndex gridIndex)
    {
        // Get the board's bounds
        Bounds boardBounds = _boardSprite.bounds;

        // Calculate half the board's width and height
        float boardHalfWidth = boardBounds.size.x * 0.5f;
        float boardHalfHeight = boardBounds.size.y * 0.5f;

        // Calculate the world position based on grid index and slot dimensions
        float worldX = (gridIndex.Column * SlotWidth) - boardHalfWidth + SlotWidth * 0.5f /*+ boardBounds.min.x*/;
        float worldY = (gridIndex.Row * SlotHeight) - boardHalfHeight + SlotHeight * 0.5f /*+ boardBounds.min.y*/;

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

    public void DestroyLetterTile(uint letterId)
    { 
        if (_letterTilesSpritesMap.ContainsKey(letterId))
        {
            Destroy(_letterTilesSpritesMap[letterId].gameObject);
            _letterTilesSpritesMap.Remove(letterId);
        }
    }

    public WorldLetterTileVisual CreateLetterTile(LetterDataObj letterData, int playerIndex)
    {
        GameObject newInstance = Instantiate(_letterTilePrefab, Vector3.zero, Quaternion.identity, _boardSprite.gameObject.transform);

        WorldLetterTileVisual tileComponent = newInstance.GetComponent<WorldLetterTileVisual>();

        tileComponent.Populate(letterData, playerIndex);

        _letterTilesSpritesMap.Add(letterData.UniqueId, newInstance.GetComponent<SpriteRenderer>());

        return tileComponent;
    }

    public bool IsWorldPositionIntersectingBoard(Vector3 worldPosition)
    {
        Bounds spriteBounds = _boardSprite.bounds;

        return spriteBounds.Contains(worldPosition);
    }

    public BoardSlotIndex GetNearestSlotIndex(Vector3 worldPosition)
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

        BoardSlotIndex index;
        index.Row = row;
        index.Column = column;
        return index;
    }

    public void CreateBonusTileVisuals(IReadOnlyBoardState boardState)
    {
        var slotStates = boardState.GetAllSlotStatesFlattened();

        foreach (var slotState in slotStates)
        {
            if (slotState.BonusType == TileBonusType.kNone)
            {
                continue;
            }

            CreateBonusTile(slotState);
        }
    }

    private void CreateBonusTile(BoardSlotState slotState)
    {
        Vector3 snappedPosition = GetWorldPositionForGridIndex(slotState.BoardIndex);

        GameObject newInstance = Instantiate(_bonusTilePrefab, snappedPosition, Quaternion.identity, _boardSprite.gameObject.transform);

        WorldBonusTile bonusTileComponent = newInstance.GetComponent<WorldBonusTile>();

        bonusTileComponent.Populate(slotState.BonusType);
    }
}
