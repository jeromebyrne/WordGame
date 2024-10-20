using Unity.VisualScripting;
using UnityEngine;

public class WorldTileDragHandler : MonoBehaviour
{
    private Camera _camera;
    private bool _isDragging = false;
    private GameObject _selectedTile = null;
    [SerializeField] BoardVisual _boardVisual = null;

    public void Init()
    {
        _camera = Camera.main;
    }

    private void OnEnable()
    {
        GameEventHandler.Instance.Subscribe<ReturnTileToHolderEvent>(OnTileReturnedToHolder);
    }

    private void OnDisable()
    {
        GameEventHandler.Instance.Unsubscribe<ReturnTileToHolderEvent>(OnTileReturnedToHolder);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectTileTapped();
        }

        if (_isDragging && _selectedTile != null)
        {
            DragTile();
        }

        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;

            if (_selectedTile != null)
            {
                // send a drag end event
                var visualComponent = _selectedTile.GetComponent<WorldLetterTileVisual>();

                if (visualComponent)
                {
                    var postEvt = WorldTileEndDragEvent.Get(visualComponent);
                    GameEventHandler.Instance.TriggerEvent(postEvt);
                }
                
            }

            _selectedTile = null;
        }
    }

    // Detects if a tile was tapped
    void DetectTileTapped()
    {
        if (_selectedTile != null)
        {
            // already dragging
            return;
        }

        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = _camera.ScreenToWorldPoint(mousePos);

        // Find all tiles in the scene (could also optimize with tags or other methods)
        var tileRenderers = _boardVisual.GetTilesSpriteRenderers();

        foreach (var kvp in tileRenderers)
        {
            SpriteRenderer sr = kvp.Value;
            if (!sr.gameObject.activeInHierarchy)
            {
                continue;
            }

            // we want to make sure this is not a committed tile. Committed tiles should not be moveable
            var visualComponent = sr.gameObject.GetComponent<WorldLetterTileVisual>();
            if (visualComponent == null || visualComponent.IsLocked)
            {
                continue;
            }

            if (IsPointWithinSpriteBounds(sr, worldPos))
            {
                _selectedTile = sr.gameObject;
                _isDragging = true;

                var postEvt = WorldTileStartDragEvent.Get(visualComponent);
                GameEventHandler.Instance.TriggerEvent(postEvt);

                break;
            }
        }
    }

    // Checks if the point is within the bounds of the sprite
    bool IsPointWithinSpriteBounds(SpriteRenderer renderer, Vector3 point)
    {
        point.z = renderer.transform.position.z;

        // Get the bounds of the sprite in world space
        Bounds bounds = renderer.bounds;
        return bounds.Contains(point);
    }

    // Drags the tile by updating its position to follow the cursor
    void DragTile()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = _camera.ScreenToWorldPoint(mousePos);
        worldPos.z = _selectedTile.transform.position.z; // Keep the Z position constant
        _selectedTile.transform.position = worldPos;
    }

    void OnTileReturnedToHolder(ReturnTileToHolderEvent evt)
    {
        if (_selectedTile)
        {
            WorldLetterTileVisual tileVisual = _selectedTile.gameObject.GetComponent<WorldLetterTileVisual>();

            if (tileVisual.LetterData.UniqueId == evt.LetterId)
            {
                // this tile will be destroyed
                _selectedTile = null;
                _isDragging = false;
            }
        }
    }
}