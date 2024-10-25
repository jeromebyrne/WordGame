using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILetterTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TMP_Text _letterLabel;
    [SerializeField] private TMP_Text _scoreLabel;
    [SerializeField] RectTransform _rectTransform;
    [SerializeField] Image _image;

    static readonly Vector3 _selectedScale = new Vector3(1.0f, 1.0f, 1.0f);
    static readonly Color _selectedColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
    static readonly Color _unselectedColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

    private Color _textInitColor;

    public RectTransform RectTransform { get { return _rectTransform; } }

    private CanvasGroup _canvasGroup;

    public int PlayerIndex { get; private set; }

    public LetterDataObj LetterInfo { get; private set; }

    private void Start()
    {
        _textInitColor = _letterLabel.color;
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    // TODO: shouldn't pass SingleLetterInfo, just integers
    public void Populate(LetterDataObj letterInfo, int playerIndex)
    {
        _letterLabel.text = letterInfo.Character.ToString().ToUpper();
        _scoreLabel.text = letterInfo.Score.ToString();
        PlayerIndex = playerIndex;
        LetterInfo = letterInfo;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = false;

        // post a drag start event
        var evt = UILetterTileStartDragEvent.Get(this);
        GameEventHandler.Instance.TriggerEvent(evt);

        gameObject.transform.localScale = _selectedScale;
        _image.color = _selectedColor;
        _letterLabel.color = Color.white;
        _scoreLabel.color = Color.white;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Convert the screen point to a local point in the RectTransform
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform.parent as RectTransform,
            eventData.position,
            null,
            out localPoint);

        _rectTransform.anchoredPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;

        // post a drag end event
        var evt = UILetterTileEndDragEvent.Get(this);
        GameEventHandler.Instance.TriggerEvent(evt);

        gameObject.transform.localScale = Vector3.one;
        _image.color = _unselectedColor;
        _letterLabel.color = _textInitColor;
        _scoreLabel.color = _textInitColor;
    }
}
