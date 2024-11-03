using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TilePlacementInfo = System.Collections.Generic.List<System.ValueTuple<UnityEngine.Vector2, UnityEngine.GameObject>>;

public class UILetterTileHolder : MonoBehaviour
{
    [SerializeField] RectTransform _holderRect = null;
    [SerializeField] GameObject _tilePrefab = null;
    [SerializeField] Image _playerColorElement = null;

    Dictionary<int, GameObject> _playersTileParents = null;

    // position and if that position is taken 
    Dictionary<int, TilePlacementInfo> _tilePlacementPlayerMap = null;

    Dictionary<int, Color> _playerColors = new Dictionary<int, Color>();

    private bool _initialized = false;

    public void Init()
    {
        Debug.Log($"Instance ID in Init: {GetInstanceID()}");

        if (_initialized)
        {
            return;
        }

        _playersTileParents = new Dictionary<int, GameObject>();
        _tilePlacementPlayerMap = new Dictionary<int, TilePlacementInfo>();

        // hardcoding 2 players for right now
        // TODO: listen for players added events from GameManager
        CreateTilePositions(1);
        CreateTilePositions(2);

        _playersTileParents[1].SetActive(false);
        _playersTileParents[2].SetActive(false);

        _initialized = true;

        // Verify initialization
        Debug.Log($"_playersTileParents[1]: {_playersTileParents[1]}");
        Debug.Log($"_playersTileParents[2]: {_playersTileParents[2]}");
    }

    private void OnEnable()
    {
        GameEventHandler.Instance.Subscribe<PlayerLetterAssignedEvent>(OnPlayerLetterAssigned);
        GameEventHandler.Instance.Subscribe<ReturnTileToHolderEvent>(OnTileReturnRequest);
        GameEventHandler.Instance.Subscribe<UITilePlacedonBoardEvent>(OnTilePlaced);
        GameEventHandler.Instance.Subscribe<ConfirmSwitchPlayerEvent>(OnPlayerSwitch);
        GameEventHandler.Instance.Subscribe<TilesCommittedEvent>(OnTilesCommitted);
        GameEventHandler.Instance.Subscribe<PlayerColorSetEvent>(OnPlayerColorSetEvent);
    }

    private void OnDisable()
    {
        GameEventHandler.Instance.Unsubscribe<PlayerLetterAssignedEvent>(OnPlayerLetterAssigned);
        GameEventHandler.Instance.Unsubscribe<ReturnTileToHolderEvent>(OnTileReturnRequest);
        GameEventHandler.Instance.Unsubscribe<UITilePlacedonBoardEvent>(OnTilePlaced);
        GameEventHandler.Instance.Unsubscribe<ConfirmSwitchPlayerEvent>(OnPlayerSwitch);
        GameEventHandler.Instance.Unsubscribe<TilesCommittedEvent>(OnTilesCommitted);
        GameEventHandler.Instance.Unsubscribe<PlayerColorSetEvent>(OnPlayerColorSetEvent);
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

    private GameObject CreateTile(LetterDataObj letterInfo, int playerIndex)
    {
        Debug.Log($"Instance ID in CreateTile: {GetInstanceID()}");

        GameObject newInstance = Instantiate(_tilePrefab, _playersTileParents[playerIndex].transform);

        UILetterTile ltComponent = newInstance.GetComponent<UILetterTile>();

        ltComponent.Populate(letterInfo, playerIndex);

        newInstance.SetActive(false);

        return newInstance;
    }

    private void DestroyTile(int playerIndex, GameObject gameObject)
    {
        // find any tiles in the holder that has the unique letter id and destroy it and free up a spot for new tiles
        TilePlacementInfo placementInfo = _tilePlacementPlayerMap[playerIndex];
        
        for (int i = 0; i < placementInfo.Count; i++)
        {
            if (placementInfo[i].Item2 == gameObject)
            {
                Destroy(gameObject);
                placementInfo[i] = ( placementInfo[i].Item1, null ); // null out the gameObject
                Debug.Log("Destroyed UITile in holder");
                return;
            }
        }
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

    private void OnTileReturnRequest(ReturnTileToHolderEvent evt)
    {
        TilePlacementInfo tilePlacements = _tilePlacementPlayerMap[evt.PlayerIndex];

        foreach (var tuple in tilePlacements)
        {
            UILetterTile letterTileComponent = tuple.Item2.GetComponent<UILetterTile>();
            if (letterTileComponent.LetterInfo.UniqueId == evt.LetterId)
            {
                letterTileComponent.gameObject.SetActive(true);
                letterTileComponent.transform.localPosition = tuple.Item1;
                return;
            }
        }
    }

    private void OnTilePlaced(UITilePlacedonBoardEvent evt)
    {
        // For now just disable the tile
        // when we commit the tile later we can null out the GameObject

        evt.Tile.gameObject.SetActive(false);
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

    void OnPlayerSwitch(ConfirmSwitchPlayerEvent switchEvent)
    {
        // TODO: Do any cleanup for the previous player

        if (_playersTileParents.ContainsKey(switchEvent.EndPlayerIndex))
        {
            _playersTileParents[switchEvent.EndPlayerIndex].SetActive(false);
        }

        if (_playersTileParents.ContainsKey(switchEvent.NextPlayerIndex))
        {
            _playersTileParents[switchEvent.NextPlayerIndex].SetActive(true);
        }

        _playerColorElement.color = _playerColors[switchEvent.NextPlayerIndex];
    }

    void OnTilesCommitted(TilesCommittedEvent evt)
    {
        if (!_tilePlacementPlayerMap.ContainsKey(evt.PlayerIndex))
        {
            Debug.Log("PlayerId is invalid in UILetterTileHolder.OnTilesCommitted");
            return;
        }

        TilePlacementInfo tilePlacementTupleList = _tilePlacementPlayerMap[evt.PlayerIndex];

        List<GameObject> killList = new List<GameObject>();

        foreach (var tuple in tilePlacementTupleList)
        {
            GameObject uiTileObject = tuple.Item2;

            if (uiTileObject == null)
            {
                continue;
            }

            UILetterTile tileComponent = uiTileObject.GetComponent<UILetterTile>();

            uint letterId = tileComponent.LetterInfo.UniqueId;

            foreach (uint id in evt.CommittedTileLetterIds)
            {
                if (id == letterId)
                {
                    killList.Add(uiTileObject);
                    break;
                }
            }
        }

        for (int i = 0; i < killList.Count; i++)
        {
            DestroyTile(evt.PlayerIndex, killList[i]);
        }
    }

    void OnPlayerColorSetEvent(PlayerColorSetEvent evt)
    {
        _playerColors.Add(evt.PlayerIndex, evt.PlayerColor);
    }
}
