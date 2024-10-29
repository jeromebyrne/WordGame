using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICountdownTimer : MonoBehaviour
{
    [SerializeField] TMP_Text _countdownLabel = null;
    [SerializeField] Image _clockImage = null;

    private float _currentCountdownTime = 0.0f;
    private float _countdownTime = 0.0f;

    private void Start()
    {
        EnableElements(false);
    }

    public void OnEnable()
    {
        GameEventHandler.Instance.Subscribe<StartTurnCountdownTimer>(OnStartTimerEvent);
        GameEventHandler.Instance.Subscribe<StopTurnCountdownTimer>(OnStopTimerEvent);
    }

    public void OnDisable()
    {
        GameEventHandler.Instance.Unsubscribe<StartTurnCountdownTimer>(OnStartTimerEvent);
        GameEventHandler.Instance.Unsubscribe<StopTurnCountdownTimer>(OnStopTimerEvent);
    }

    void OnStartTimerEvent(StartTurnCountdownTimer evt)
    {
        _currentCountdownTime = evt.CountdownTime;
        _countdownTime = evt.CountdownTime;

        _countdownLabel.color = Color.white;
        _clockImage.color = Color.white;

        EnableElements(true);
    }

    void OnStopTimerEvent(StopTurnCountdownTimer evt)
    {
        EnableElements(false);
    }

    private void EnableElements(bool enabled)
    {
        _countdownLabel.gameObject.SetActive(enabled);
        _clockImage.gameObject.SetActive(enabled);
    }

    private void Update()
    {
        if (_countdownLabel.isActiveAndEnabled)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(_currentCountdownTime);

            string formattedTime = string.Format("{0:D2}:{1:D2}",
                                 timeSpan.Minutes,
                                 timeSpan.Seconds);

            _countdownLabel.text = formattedTime;

            if (_currentCountdownTime < _countdownTime * 0.5f)
            {
                _countdownLabel.color = Color.red;
                _clockImage.color = Color.red;
            }
            else
            {
                _countdownLabel.color = Color.white;
                _clockImage.color = Color.white;
            }
        }

        _currentCountdownTime -= Time.deltaTime;
    
    }
}
