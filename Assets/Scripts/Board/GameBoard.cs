using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    struct BoardSlotState
    {
        public BoardSlotState(bool occupied, Vector2Int boardIndex)
        {
            IsOccupied = occupied;
            OccupiedLetter = new SingleLetterInfo();
            BoardIndex = boardIndex;
        }

        public bool IsOccupied { get; set; }
        public SingleLetterInfo OccupiedLetter { get; private set; }
        public Vector2Int BoardIndex { get; private set; }
    }

    [SerializeField] BoardVisual _boardVisual = null;

    private List<List<BoardSlotState>> _boardSlotStates = null;

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
        var boardGridDimensions = GameSettingsConfigManager.GameSettings._boardDimensions;
        _boardVisual.SetGridDimensions(boardGridDimensions);

        CreateSlots();
    }

    private void CreateSlots()
    {
        _boardSlotStates = new List<List<BoardSlotState>>();

        Vector2Int boardDImensions = GameSettingsConfigManager.GameSettings._boardDimensions;

        for (int i = 0; i < boardDImensions.x; i++)
        {
            _boardSlotStates.Add(new List<BoardSlotState>());
            for (int j = 0; j < boardDImensions.y; j++)
            {
                // BoardSlotState slotState
                BoardSlotState slotState = new BoardSlotState(false, new Vector2Int(j,i));
                _boardSlotStates[i].Add(slotState);
            }
        }
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

        if (_boardVisual.IsUILetterTileIntersecting(evt.LetterTile))
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

        Bounds spriteBounds = _boardVisual.VisualBounds;

        if (!spriteBounds.Contains(worldPos))
        {
            return;
        }

        Vector2Int slotIndex = _boardVisual.GetNearestSlotIndex(worldPos);

        // TODO: check if it's valid + update data state
        _boardVisual.EnableTile(slotIndex.x, slotIndex.y);

    }
}
