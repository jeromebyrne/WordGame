using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField] BoardVisual _boardVisual = null;
    private BoardState _boardState = null;

    public IReadOnlyBoardState GetBoardState() // don't cast me to the concrete type...
    { 
        return _boardState;
    }

    private UILetterTile _draggedUILetterTile = null; // only allow dragging 1 tile at a time for simplicity

    private void OnEnable()
    {
        GameEventHandler.Instance.Subscribe<UILetterTileStartDragEvent>(OnUITileStartDrag);
        GameEventHandler.Instance.Subscribe<UILetterTileEndDragEvent>(OnUITileEndDrag);
    }

    private void OnDisable()
    {
        GameEventHandler.Instance.Unsubscribe<UILetterTileStartDragEvent>(OnUITileStartDrag);
        GameEventHandler.Instance.Unsubscribe<UILetterTileEndDragEvent>(OnUITileEndDrag);
    }

    public void Init()
    {
        _boardState = new BoardState(GameSettingsConfigManager.GameSettings._boardDimensions);
    }

    private void OnUITileStartDrag(UILetterTileStartDragEvent evt)
    {
        if (_draggedUILetterTile != null && evt.LetterTile != _draggedUILetterTile)
        {
            var postEvt = SendTileToHolderEvent.Get(evt.LetterTile.PlayerIndex, evt.LetterTile);
            GameEventHandler.Instance.TriggerEvent(postEvt);
        }

        _draggedUILetterTile = evt.LetterTile;
    }

    private void OnUITileEndDrag(UILetterTileEndDragEvent evt)
    {
        UnityEngine.Debug.Assert(_draggedUILetterTile == evt.LetterTile, "End drag on a UI letter tile that wasn't being tracked");

        // TODO:
        // if the UI letter tile is within the boards world space then place it on the nearest tile
        // otherwise send it back to the tile holder

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(evt.LetterTile.RectTransform.position);

        if (_boardVisual.IsWorldPositionIntersectingBoard(worldPos))
        {
            UnityEngine.Debug.Log("Tile is intersecting board");

            PlaceTile(evt.LetterTile.PlayerIndex, evt.LetterTile);
        }
        else
        {
            UnityEngine. Debug.Log("Tile is NOT intersecting board");
            var postEvt = SendTileToHolderEvent.Get(evt.LetterTile.PlayerIndex, evt.LetterTile);
            GameEventHandler.Instance.TriggerEvent(postEvt);
        }

        _draggedUILetterTile = null;
    }


    private void PlaceTile(int playerIndex, UILetterTile uiTile)
    {
        // Get world pos of UITile
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(uiTile.RectTransform.position);

        if (!_boardVisual.IsWorldPositionIntersectingBoard(worldPos))
        {
            return;
        }

        Vector2Int slotIndex = _boardVisual.GetNearestSlotIndex(worldPos);

        BoardSlotState slotState = _boardState.GetSlotState(slotIndex.x, slotIndex.y);
        if (slotState.IsOccupied)
        {
            // if the slow is already occupied then return it to the holder
            var sendBackEvent = SendTileToHolderEvent.Get(playerIndex, uiTile);
            GameEventHandler.Instance.TriggerEvent(sendBackEvent);
            return;
        }

        // update board data
        slotState.IsOccupied = true;
        slotState.OccupiedLetter = uiTile.LetterInfo;
        _boardState.UpdateSlotState(slotIndex.x, slotIndex.y, slotState);

        // update board visual (TODO: should really send an event to do this)
        _boardVisual.EnableTile(slotIndex.x, slotIndex.y, uiTile.LetterInfo);

        var postEvt = UITilePlacedonBoardEvent.Get(playerIndex, uiTile);
        GameEventHandler.Instance.TriggerEvent(postEvt);
    }

    public bool HasSlotAbove(BoardSlotState currentSlot)
    {
        Vector2Int index = currentSlot.BoardIndex;

        if (index.y < (_boardState.Dimensions.y - 1))
        {
            return true;
        }

        return false;
    }

    public bool HasSlotBelow(BoardSlotState currentSlot)
    {
        Vector2Int index = currentSlot.BoardIndex;

        if (index.y > 0)
        {
            return true;
        }

        return false;
    }

    public bool HasSlotLeft(BoardSlotState currentSlot)
    {
        Vector2Int index = currentSlot.BoardIndex;

        if (index.x > 0)
        {
            return true;
        }

        return false;
    }

    public bool HasSlotRight(BoardSlotState currentSlot)
    {
        Vector2Int index = currentSlot.BoardIndex;

        if (index.x < (_boardState.Dimensions.x - 1))
        {
            return true;
        }

        return false;
    }

    public BoardSlotState GetSlotAbove(BoardSlotState currentSlot)
    {
        // assumes there is a slot above
        Vector2Int index = currentSlot.BoardIndex;

        return _boardState.GetSlotState(index.x, index.y + 1);
    }

    public BoardSlotState GetSlotBelow(BoardSlotState currentSlot)
    {
        Vector2Int index = currentSlot.BoardIndex;

        return _boardState.GetSlotState(index.x, index.y - 1);
    }

    public BoardSlotState GetSlotLeft(BoardSlotState currentSlot)
    {
        Vector2Int index = currentSlot.BoardIndex;

        return _boardState.GetSlotState(index.x - 1, index.y);
    }

    public BoardSlotState GetSlotRight(BoardSlotState currentSlot)
    {
        Vector2Int index = currentSlot.BoardIndex;

        return _boardState.GetSlotState(index.x + 1, index.y);
    }
}
