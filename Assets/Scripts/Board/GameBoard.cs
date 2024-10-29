using System.Collections.Generic;
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
        GameEventHandler.Instance.Subscribe<WorldTileStartDragEvent>(OnWorldTileStartDrag);
        GameEventHandler.Instance.Subscribe<WorldTileEndDragEvent>(OnWorldTileEndDrag);
        GameEventHandler.Instance.Subscribe<ReturnAllUncommittedTilesToHolder>(OnReturnAllUncommittedTiles);
        
    }

    private void OnDisable()
    {
        GameEventHandler.Instance.Unsubscribe<UILetterTileStartDragEvent>(OnUITileStartDrag);
        GameEventHandler.Instance.Unsubscribe<UILetterTileEndDragEvent>(OnUITileEndDrag);
        GameEventHandler.Instance.Unsubscribe<WorldTileStartDragEvent>(OnWorldTileStartDrag);
        GameEventHandler.Instance.Unsubscribe<WorldTileEndDragEvent>(OnWorldTileEndDrag);
        GameEventHandler.Instance.Unsubscribe<ReturnAllUncommittedTilesToHolder>(OnReturnAllUncommittedTiles);
    }

    public void Init()
    {
        _boardState = new BoardState(GameSettingsConfigManager.GameSettings._boardDimensions);
        _boardVisual.CreateBonusTileVisuals(_boardState);
    }

    private void OnUITileStartDrag(UILetterTileStartDragEvent evt)
    {
        if (_draggedUILetterTile != null && evt.LetterTile != _draggedUILetterTile)
        {
            GameEventHandler.Instance.TriggerEvent(ReturnTileToHolderEvent.Get(evt.LetterTile.PlayerIndex, evt.LetterTile.LetterInfo.UniqueId));
        }

        _draggedUILetterTile = evt.LetterTile;

        // GameEventHandler.Instance.TriggerEvent(PlayAudioEvent.Get("Audio/select", 0.1f, false, false));
    }

    private void OnUITileEndDrag(UILetterTileEndDragEvent evt)
    {
        UnityEngine.Debug.Assert(_draggedUILetterTile == evt.LetterTile, "End drag on a UI letter tile that wasn't being tracked");

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(evt.LetterTile.RectTransform.position);

        if (_boardVisual.IsWorldPositionIntersectingBoard(worldPos))
        {
            UnityEngine.Debug.Log("Tile is intersecting board");

            PlaceUITile(evt.LetterTile.PlayerIndex, evt.LetterTile, worldPos);
        }
        else
        {
            UnityEngine. Debug.Log("Tile is NOT intersecting board");
            GameEventHandler.Instance.TriggerEvent(ReturnTileToHolderEvent.Get(evt.LetterTile.PlayerIndex, evt.LetterTile.LetterInfo.UniqueId));
            GameEventHandler.Instance.TriggerEvent(PlayAudioEvent.Get("Audio/fly", 1.0f, false, false));
        }

        _draggedUILetterTile = null;
    }

    private void OnWorldTileStartDrag(WorldTileStartDragEvent evt)
    {
        var worldTile = evt.LetterTile;

        // Update the board data
        BoardSlotState slotState = _boardState.GetSlotState(worldTile.GridIndex);

        if (slotState.IsTileCommitted)
        {
            // We shouldn't be able to move committed tiles
            return;
        }

        slotState.IsOccupied = false;
        _boardState.UpdateSlotState(worldTile.GridIndex, slotState);

        // GameEventHandler.Instance.TriggerEvent(PlayAudioEvent.Get("Audio/select", 0.1f, false, false));
    }

    private void OnWorldTileEndDrag(WorldTileEndDragEvent evt)
    {
        var worldTile = evt.LetterTile;

        Vector2 worldPos = worldTile.transform.position;

        if (!_boardVisual.IsWorldPositionIntersectingBoard(worldPos))
        {
            GameEventHandler.Instance.TriggerEvent(ReturnTileToHolderEvent.Get(evt.LetterTile.PlayerIndex, evt.LetterTile.LetterData.UniqueId));
            _boardVisual.DestroyLetterTile(evt.LetterTile.LetterData.UniqueId);
            GameEventHandler.Instance.TriggerEvent(PlayAudioEvent.Get("Audio/fly", 1.0f, false, false));
            return;
        }

        SnapWorldTile(evt.LetterTile.PlayerIndex, evt.LetterTile, worldPos);

        // play sound effect
        int randomNumber = Random.Range(1, 5);
        GameEventHandler.Instance.TriggerEvent(PlayAudioEvent.Get("Audio/tile_" + randomNumber.ToString(), 1.0f, false, false));
    }

    private void SnapWorldTile(int playerIndex, WorldLetterTileVisual worldTile, Vector2 worldPos)
    {
        BoardSlotIndex nearestSlotIndex = _boardVisual.GetNearestSlotIndex(worldPos);

        var retTuple = BoardDataHelper.FindNextNearestUnoccupiedSlot(nearestSlotIndex, _boardState, worldPos, _boardVisual);

        if (retTuple.Item1 == false)
        {
            // TODO: there was no space on the board somehow...
            // We need to figure out what we should do in this scenario

            // TODO: if the worldTile has a set grid index then mak
            return;
        }

        BoardSlotIndex nearestUnoccupiedIndex = retTuple.Item2;

        worldTile.SetGridIndex(nearestUnoccupiedIndex);

        Vector3 snappedPosition = _boardVisual.GetWorldPositionForGridIndex(nearestUnoccupiedIndex);
        snappedPosition.z = _boardVisual.transform.position.z - 2.0f; // TODO: this is hacky

        worldTile.transform.position = snappedPosition;

        // Update the board data
        BoardSlotState slotState = _boardState.GetSlotState(nearestUnoccupiedIndex);

        slotState.IsOccupied = true;
        slotState.OccupiedLetter = worldTile.LetterData;
        _boardState.UpdateSlotState(nearestUnoccupiedIndex, slotState);
    }

    private void PlaceUITile(int playerIndex, UILetterTile uiTile, Vector2 worldPos)
    {
        if (!_boardVisual.IsWorldPositionIntersectingBoard(worldPos))
        {
            return;
        }

        // update board visual (TODO: should really send an event to do this)
        var worldTile = _boardVisual.CreateLetterTile(uiTile.LetterInfo, playerIndex);

        SnapWorldTile(playerIndex, worldTile, worldPos);

        GameEventHandler.Instance.TriggerEvent(UITilePlacedonBoardEvent.Get(playerIndex, uiTile));

        int randomNumber = Random.Range(1, 5);
        GameEventHandler.Instance.TriggerEvent(PlayAudioEvent.Get("Audio/tile_" + randomNumber.ToString(), 1.0f, false, false));
    }

    public void CommitTiles(List<BoardSlotIndex> tilesToCommit, int playerIndex)
    {
        List<uint> letterIds = new List<uint>();

        foreach (var index in tilesToCommit)
        {
            var slotState = _boardState.GetSlotState(index);
            slotState.IsTileCommitted = true;
            letterIds.Add(slotState.OccupiedLetter.UniqueId);
            _boardState.UpdateSlotState(index, slotState);
        }

        GameEventHandler.Instance.TriggerEvent(TilesCommittedEvent.Get(playerIndex, tilesToCommit, letterIds));
    }

    private void OnReturnAllUncommittedTiles(ReturnAllUncommittedTilesToHolder evt)
    {
        List<BoardSlotIndex> placedIndices = BoardDataHelper.GetUncommittedTiles(_boardState);

        int count = 0;

        foreach (BoardSlotIndex index in placedIndices)
        {
            BoardSlotState slotState = _boardState.GetSlotState(index);

            if (slotState.IsOccupied && !slotState.IsTileCommitted)
            {
                GameEventHandler.Instance.TriggerEvent(ReturnTileToHolderEvent.Get(evt.PlayerIndex, slotState.OccupiedLetter.UniqueId));
                slotState.IsOccupied = false;
                _boardState.UpdateSlotState(index, slotState);
                _boardVisual.DestroyLetterTile(slotState.OccupiedLetter.UniqueId);
                count++;
            }
        }

        if (count > 0)
        {
            GameEventHandler.Instance.TriggerEvent(PlayAudioEvent.Get("Audio/fly", 1.0f, false, false));
        }
    }
}
