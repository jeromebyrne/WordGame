using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerScoreCounter : MonoBehaviour
{
    [SerializeField] private int _playerIndex = -1;
    [SerializeField] TMP_Text _scoreLabel = null;
    [SerializeField] Color _playerColor;
    [SerializeField] Image _image = null;

    private Color _inactiveColor = Color.gray;
    private Vector3 _activeScale = new Vector3(1.25f, 1.25f, 1.0f);

    private void OnEnable()
    {
        GameEventHandler.Instance.Subscribe<PlayerStateUpdatedEvent>(OnScoreUpdated);
        GameEventHandler.Instance.Subscribe<ConfirmSwitchPlayerEvent>(OnPlayerSwitch);
    }

    private void OnDisable()
    {
        GameEventHandler.Instance.Subscribe<PlayerStateUpdatedEvent>(OnScoreUpdated);
        GameEventHandler.Instance.Subscribe<ConfirmSwitchPlayerEvent>(OnPlayerSwitch);
    }

    void Start()
    {
        _scoreLabel.text = "0";
        _image.color = _inactiveColor;
        _playerColor.a = 1.0f;
    }

    void OnScoreUpdated(PlayerStateUpdatedEvent evt)
    {
        if (evt.PlayerState.PlayerIndex != _playerIndex)
        {
            return;
        }

        _scoreLabel.text = evt.PlayerState.Score.ToString();
    }

    void OnPlayerSwitch(ConfirmSwitchPlayerEvent evt)
    {
        if (evt.NextPlayerIndex == _playerIndex)
        {
            _image.color = _playerColor;
            _image.transform.localScale = _activeScale;
        }
        else
        {
            _image.color = _inactiveColor;
            _image.transform.localScale = Vector3.one;
        }
    }
}
