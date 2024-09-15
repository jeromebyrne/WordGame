using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField] BoardVisual _boardVisual = null;

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

    void Start()
    {
        // TODO: Move this to its own function and have the GameManager call it so we can control init order
        var boardGridDimensions = GameSettingsConfigManager.GameSettings._boardDimensions;
        _boardVisual.SetGridDimensions(boardGridDimensions);
    }

    void Update()
    {
        
    }

    private void OnUITileStartDrag(UILetterTileStartDragEvent evt)
    {
        if (evt.LetterTile != _draggedUILetterTile)
        {
            // TODO: send to the letter holder
        }

        _draggedUILetterTile = evt.LetterTile;
    }

    private void OnUITileEndDrag(UILetterTileEndDragEvent evt)
    {
        Debug.Assert(_draggedUILetterTile == evt.LetterTile, "End drag on a UI letter tile that wasn't being tracked");

        // TODO:
        // if the UI letter tile is within the boards world space then place it on the nearest tile
        // otherwise send it back to the tile holder

        _draggedUILetterTile = null;
    }
}
