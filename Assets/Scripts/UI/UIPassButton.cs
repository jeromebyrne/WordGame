using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPassButton : MonoBehaviour
{
    [SerializeField] TMP_Text _label = null;
    [SerializeField] Image _buttonImage = null;

    private const float kAutoCollapseTime = 2.0f;
    private float _autoCollapseCountdown = kAutoCollapseTime;

    private Vector2 _collapsedSize;
    private Vector2 _expandedSize;

    private readonly Color kCollapsedButtonColor = Color.grey;
    private readonly Color kExpandedButtonColor = Color.red;
    private readonly Color kCollapsedTextColor = Color.black;
    private readonly Color kExpandedTextColor = Color.white;

    bool _isCollapsed = true;

    private void OnEnable()
    {
        GameEventHandler.Instance.Subscribe<PassTurnEvent>(OnPassTurnEvent);
    }

    private void OnDisable()
    {
        GameEventHandler.Instance.Unsubscribe<PassTurnEvent>(OnPassTurnEvent);
    }

    // Start is called before the first frame update
    void Start()
    {
        _collapsedSize = _buttonImage.rectTransform.sizeDelta;
        _expandedSize = _collapsedSize;
        _expandedSize.x *= 2.0f;

        SetCollapsed(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isCollapsed)
        {
            _autoCollapseCountdown -= Time.deltaTime;

            if (_autoCollapseCountdown <= 0.0f)
            {
                SetCollapsed(true);
            }
        }
    }

    private void SetCollapsed(bool collapsed)
    {
        _buttonImage.rectTransform.sizeDelta = collapsed ? _collapsedSize : _expandedSize;
        _label.text = collapsed ? "..." : "Pass";
        _label.color = collapsed ? kCollapsedTextColor : kExpandedTextColor;
        _buttonImage.color = collapsed ? kCollapsedButtonColor : kExpandedButtonColor;
        _autoCollapseCountdown = kAutoCollapseTime; // always reset the time whatever state we are in
        _isCollapsed = collapsed;
    }

    public void OnButtonPressed()
    {
        if (_isCollapsed)
        {
            // GameEventHandler.Instance.TriggerEvent(PlayAudioEvent.Get("Audio/pass_btn", 1.0f, false, false));
            SetCollapsed(false);
            return;
        }

        GameEventHandler.Instance.TriggerEvent(PassTurnEvent.Get());
    }

    private void OnPassTurnEvent(PassTurnEvent evt)
    {
        SetCollapsed(true);
    }
}
