using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILetterTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
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

    private bool _isDragging = false;

    public int PlayerIndex { get; private set; }

    public LetterDataObj LetterInfo { get; private set; }

    private void Start()
    {
        _textInitColor = _letterLabel.color;
    }

    private void OnEnable()
    {
        GameEventHandler.Instance.Subscribe<ReturnAllUncommittedTilesToHolderEvent>(OnReturnAllUncommittedTiles);
    }

    private void OnDisable()
    {
        GameEventHandler.Instance.Unsubscribe<ReturnAllUncommittedTilesToHolderEvent>(OnReturnAllUncommittedTiles);
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

    public void OnPointerDown(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = false;
        _isDragging = true;

        ShowSelectedVisuals(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;
        _isDragging = false;

        ShowSelectedVisuals(false);
    }

    void ShowSelectedVisuals(bool show)
    {
        if (show)
        {
            gameObject.transform.localScale = _selectedScale;
            _image.color = _selectedColor;
            _letterLabel.color = Color.white;
            _scoreLabel.color = Color.white;
        }
        else
        {
            gameObject.transform.localScale = Vector3.one;
            _image.color = _unselectedColor;
            _letterLabel.color = _textInitColor;
            _scoreLabel.color = _textInitColor;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = false;
        _isDragging = true;

        // post a drag start event
        var evt = UILetterTileStartDragEvent.Get(this);
        GameEventHandler.Instance.TriggerEvent(evt);

        ShowSelectedVisuals(true);
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
        EndDrag();

        // post a drag end event
        var evt = UILetterTileEndDragEvent.Get(this);
        GameEventHandler.Instance.TriggerEvent(evt);
    }

    private void EndDrag()
    {
        _canvasGroup.blocksRaycasts = true;
        _isDragging = false;

        ShowSelectedVisuals(false);
    }

    private void OnReturnAllUncommittedTiles(ReturnAllUncommittedTilesToHolderEvent evt)
    {
        if (!_isDragging)
        {
            return;
        }

        EndDrag();

        GameEventHandler.Instance.TriggerEvent(ReturnTileToHolderEvent.Get(evt.PlayerIndex, LetterInfo.UniqueId));
    }
}
