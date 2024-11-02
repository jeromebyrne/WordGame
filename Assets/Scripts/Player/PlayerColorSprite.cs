using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorSprite : MonoBehaviour
{
    [SerializeField] SpriteRenderer _spriteRenderer = null;

    Dictionary<int, Color> _playerColors = new Dictionary<int, Color>();

    private void OnEnable()
    {
        GameEventHandler.Instance.Subscribe<ConfirmSwitchPlayerEvent>(OnPlayerSwitch);
        GameEventHandler.Instance.Subscribe<PlayerColorSetEvent>(OnPlayerColorSet);
    }

    private void OnDisable()
    {
        GameEventHandler.Instance.Unsubscribe<ConfirmSwitchPlayerEvent>(OnPlayerSwitch);
        GameEventHandler.Instance.Unsubscribe<PlayerColorSetEvent>(OnPlayerColorSet);
    }

    private void OnPlayerSwitch(ConfirmSwitchPlayerEvent evt)
    {
        _spriteRenderer.color = _playerColors[evt.NextPlayerIndex];
    }

    private void OnPlayerColorSet(PlayerColorSetEvent evt)
    {
        _playerColors.Add(evt.PlayerIndex, evt.PlayerColor);
    }
}
