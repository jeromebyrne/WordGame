using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMessageBubble : MonoBehaviour
{
    [SerializeField] TMP_Text _messageLabel = null;
    [SerializeField] GameObject _messageRoot = null;

    private void OnEnable()
    {
        GameEventHandler.Instance.Subscribe<DisplayMessageBubbleEvent>(OnDisplayMessage);
        GameEventHandler.Instance.Subscribe<DismissMessageBubbleEvent>(OnDismissMessage);
    }

    private void OnDisable()
    {
        GameEventHandler.Instance.Unsubscribe<DisplayMessageBubbleEvent>(OnDisplayMessage);
        GameEventHandler.Instance.Unsubscribe<DismissMessageBubbleEvent>(OnDismissMessage);
    }

    // Start is called before the first frame update
    void Start()
    {
        _messageRoot.SetActive(false);
    }

    private void OnDisplayMessage(DisplayMessageBubbleEvent evt)
    {
        _messageRoot.SetActive(true);
        _messageLabel.text = evt.Message;

        Debug.Log(evt.Message);
    }

    private void OnDismissMessage(DismissMessageBubbleEvent evt)
    {
        _messageRoot.SetActive(false);
    }

    public void OnDismissButtonPressed()
    {
        GameEventHandler.Instance.TriggerEvent(DismissMessageBubbleEvent.Get());
    }
}
