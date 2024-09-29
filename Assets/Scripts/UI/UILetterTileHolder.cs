using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TilePlacementInfo = System.Collections.Generic.List<System.ValueTuple<UnityEngine.Vector2, UnityEngine.GameObject>>;

public class UILetterTileHolder : MonoBehaviour
{
    [SerializeField] RectTransform _holderRect;
    [SerializeField] GameObject _tilePrefab;

    Dictionary<int, GameObject> _playersTileParents = null;

    // position and if that position is taken 
    Dictionary<int, TilePlacementInfo> _tilePlacementPlayerMap = null;

    private bool _initialized = false;

    public void Init()
    {
        if (_initialized)
        {
            return;
        }

        _playersTileParents = new Dictionary<int, GameObject>();
        _tilePlacementPlayerMap = new Dictionary<int, TilePlacementInfo>();

        // hardcoding 2 players for right now
        CreateTilePositions(1);
        CreateTilePositions(2);

        // TEMP testing
        _playersTileParents[1].SetActive(true);
        _playersTileParents[1].SetActive(false);

        _initialized = true;
    }

    private void OnEnable()
    {
        GameEventHandler.Instance.Subscribe<PlayerLetterAssignedEvent>(OnPlayerLetterAssigned);
        GameEventHandler.Instance.Subscribe<SendTileToHolderEvent>(OnTileReturnRequest);
    }

    private void OnDisable()
    {
        GameEventHandler.Instance.Unsubscribe<PlayerLetterAssignedEvent>(OnPlayerLetterAssigned);
        GameEventHandler.Instance.Unsubscribe<SendTileToHolderEvent>(OnTileReturnRequest);
    }

    void CreateTilePositions(int playerIndex)
    {
        GameObject playerTileParent = new GameObject("PlayerTiles" + playerIndex.ToString());
        playerTileParent.transform.SetParent(gameObject.transform);
        playerTileParent.transform.localPosition = Vector3.zero;
        playerTileParent.transform.localScale = Vector3.one;
        playerTileParent.AddComponent<RectTransform>();

        _playersTileParents[playerIndex] = playerTileParent;

        // these positions are where UI tiles will be positioned when not on the board
        Vector3[] corners = new Vector3[4];
        _holderRect.GetLocalCorners(corners);

        int numTiles = GameSettingsConfigManager.GameSettings._maxPlayerLetters;

        float width = corners[2].x - corners[0].x;
        float spacingX = width / (numTiles + 1);
        float yOffset = 50.0f;

        float centerY = (corners[0].y + corners[1].y) / 2f;

        _tilePlacementPlayerMap[playerIndex] = new TilePlacementInfo();
        for (int i = 1; i <= numTiles; i++)
        {
            float x = corners[0].x + spacingX * i;
            Vector2 position = new Vector2(x, centerY + yOffset);

            // add an empty gameobject for a tile, later we will add and remove tiles (GameObjects)
            _tilePlacementPlayerMap[playerIndex].Add(new ValueTuple<Vector2, GameObject>(position, null));
        }
    }

    private GameObject CreateTile(SingleLetterInfo letterInfo, int playerIndex)
    {
        GameObject newInstance = Instantiate(_tilePrefab, _playersTileParents[playerIndex].transform);

        UILetterTile ltComponent = newInstance.GetComponent<UILetterTile>();

        ltComponent.Populate(letterInfo, playerIndex);

        newInstance.SetActive(false);

        return newInstance;
    }

    private void OnPlayerLetterAssigned(PlayerLetterAssignedEvent evt)
    {
        if (!HasAvailableSlotForTile(evt.PlayerState.PlayerIndex))
        {
            Debug.LogError("Assigning a letter to a player with no available slots");
            return;
        }

        GameObject tileVisual = CreateTile(evt.LetterInfo, evt.PlayerState.PlayerIndex); // TODO: ideally we would pool these

        AssignTileToNextAvailableSlot(evt.PlayerState.PlayerIndex, tileVisual);
    }

    private void OnTileReturnRequest(SendTileToHolderEvent evt)
    {
        var tilePlacements = _tilePlacementPlayerMap[evt.PlayerIndex];

        foreach (var tuple in tilePlacements)
        {
            if (tuple.Item2 == evt.Tile.gameObject)
            {
                evt.Tile.transform.localPosition = tuple.Item1;
                return;
            }
        }
    }

    bool HasAvailableSlotForTile(int playerIndex)
    {
        TilePlacementInfo tpi = _tilePlacementPlayerMap[playerIndex];

        foreach (var tuple in tpi)
        {
            if (tuple.Item2 == null)
            {
                return true;
            }
        }

        return false;
    }

    void AssignTileToNextAvailableSlot(int playerIndex, GameObject letterTile)
    {
        TilePlacementInfo tpi = _tilePlacementPlayerMap[playerIndex];

        for (int i = 0; i < tpi.Count; i++)
        {
            if (tpi[i].Item2 == null)
            {
                var tuple = tpi[i];
                tuple.Item2 = letterTile;
                tpi[i] = tuple;

                letterTile.SetActive(true);
                letterTile.transform.localPosition = tuple.Item1;
                return;
            }
        }

        Debug.LogError("Could not find an empty Tile Slot in AssignTileToNextAvailableSlot");
    }
}
