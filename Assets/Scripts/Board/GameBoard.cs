using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
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
        UnityEngine.Debug.Assert(_draggedUILetterTile == evt.LetterTile, "End drag on a UI letter tile that wasn't being tracked");

        // TODO:
        // if the UI letter tile is within the boards world space then place it on the nearest tile
        // otherwise send it back to the tile holder

        if (IsUILetterTileIntersecting(evt.LetterTile, _boardVisual.BoardSpriteRenderer))
        {
            UnityEngine.Debug.Log("Tile is intersecting board");
        }
        else
        {
            UnityEngine. Debug.Log("Tile is NOT intersecting board");

            // send this tile back to its holder
        }

        _draggedUILetterTile = null;
    }

    private bool IsUILetterTileIntersecting(UILetterTile tile, SpriteRenderer boardSpriteRenderer)
    {
        // Check if the tile's RectTransform is null
        var rectTransform = tile.RectTransform;
        if (rectTransform == null)
        {
            return false;
        }

        // Convert UI element corners to world space
        Vector3[] uiCorners = new Vector3[4];
        rectTransform.GetWorldCorners(uiCorners);

        // Convert each UI corner from screen space to world space coordinates
        for (int i = 0; i < uiCorners.Length; i++)
        {
            uiCorners[i] = Camera.main.ScreenToWorldPoint(uiCorners[i]);
        }

        // Get the world space bounds of the SpriteRenderer
        Bounds spriteBounds = boardSpriteRenderer.bounds;

        // Adjust the z-coordinate of uiCorners to match the sprite's z-coordinate
        for (int i = 0; i < uiCorners.Length; i++)
        {
            uiCorners[i].z = spriteBounds.center.z; // Set z value to the sprite's z position
        }

        // Check if any of the adjusted UI corners are within the sprite's world bounds
        for (int i = 0; i < uiCorners.Length; i++)
        {
            if (spriteBounds.Contains(uiCorners[i]))
            {
                return true; // Return true if any corner is inside the sprite's bounds
            }
        }

        return false; // No intersection detected
    }
}
