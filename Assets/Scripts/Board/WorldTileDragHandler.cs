using UnityEngine;

public class WorldTileDragHandler : MonoBehaviour
{
    private Camera _camera;
    private bool _isDragging = false;
    private GameObject _selectedTile = null;
    private SpriteRenderer _selectedTileRenderer = null;
    [SerializeField] BoardVisual _boardVisual = null;

    public void Init()
    {
        _camera = Camera.main;
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
            _selectedTile = null;
            _selectedTileRenderer = null;
        }
    }

    // Detects if a tile was tapped
    void DetectTileTapped()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = _camera.ScreenToWorldPoint(mousePos);

        // Find all tiles in the scene (could also optimize with tags or other methods)
        var tileRenderers = _boardVisual.GetTilesSpriteRenderers();

        foreach (var tr in tileRenderers)
        {
            if (!tr.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (IsPointWithinSpriteBounds(tr, worldPos))
            {
                _selectedTile = tr.gameObject;
                _selectedTileRenderer = tr;
                _isDragging = true;
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
        // _selectedTile.transform.position = worldPos;

        // disable this tile and re-enable the UI element
    }
}