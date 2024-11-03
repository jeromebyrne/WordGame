using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerScoreCounter : MonoBehaviour
{
    [SerializeField] private int _playerIndex = -1;
    [SerializeField] TMP_Text _scoreLabel = null;
    [SerializeField] Image _image = null;
    [SerializeField] Image _caratImage = null;

    Color _playerColor;
    private Color _inactiveColor = Color.gray;
    private Vector3 _activeScale = new Vector3(1.0f, 1.0f, 1.0f);

    private void OnEnable()
    {
        GameEventHandler.Instance.Subscribe<PlayerStateUpdatedEvent>(OnScoreUpdated);
        GameEventHandler.Instance.Subscribe<ConfirmSwitchPlayerEvent>(OnPlayerSwitch);
        GameEventHandler.Instance.Subscribe<PlayerColorSetEvent>(OnPlayerColorSetEvent);
    }

    private void OnDisable()
    {
        GameEventHandler.Instance.Unsubscribe<PlayerStateUpdatedEvent>(OnScoreUpdated);
        GameEventHandler.Instance.Unsubscribe<ConfirmSwitchPlayerEvent>(OnPlayerSwitch);
        GameEventHandler.Instance.Unsubscribe<PlayerColorSetEvent>(OnPlayerColorSetEvent);
    }

    void Start()
    {
        _scoreLabel.text = "0";
        _image.color = _inactiveColor;
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
            _caratImage.gameObject.SetActive(true);
        }
        else
        {
            _image.color = _inactiveColor;
            _image.transform.localScale = Vector3.one;
            _caratImage.gameObject.SetActive(false);
        }
    }

    void OnPlayerColorSetEvent(PlayerColorSetEvent evt)
    {
        if (evt.PlayerIndex != _playerIndex)
        {
            return;
        }

        _playerColor = evt.PlayerColor;

        _caratImage.color = _playerColor;
    }
}
