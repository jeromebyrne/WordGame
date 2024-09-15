using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILetterTileHolder : MonoBehaviour
{
    [SerializeField] private List<GameObject> _letterTileParents = new List<GameObject>();

    private void OnEnable()
    {
        GameEventHandler.Instance.Subscribe<PlayerLettersAssigned>(OnPLayerLettersAssigned);
    }

    private void OnDisable()
    {
        GameEventHandler.Instance.Unsubscribe<PlayerLettersAssigned>(OnPLayerLettersAssigned);
    }

    GameObject GetFirstAvailableTileParent()
    {
        foreach (var g in _letterTileParents)
        {
            if (g.transform.childCount > 0)
            {
                return g;
            }
        }

        return null;
    }

    private void OnPLayerLettersAssigned(PlayerLettersAssigned evt)
    {
        // TODO: generate player tiles
    }
}
