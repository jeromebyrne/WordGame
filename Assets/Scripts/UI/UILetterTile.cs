using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILetterTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TMP_Text _letterLabel;
    [SerializeField] private TMP_Text _pointsLabel;
    [SerializeField] RectTransform _rectTransform;
    [SerializeField] Image _image;

    static readonly Vector3 _selectedScale = new Vector3(1.75f, 1.75f, 1.0f);
    static readonly Color _selectedColor = new Color(0.72f, 0.36f, 0.125f, 0.5f);
    static readonly Color _unselectedColor = new Color(0.72f, 0.36f, 0.125f, 1.0f);

    public RectTransform RectTransform { get { return _rectTransform; } }

    private Canvas _canvas;
    private CanvasGroup _canvasGroup;

    private SingleLetterInfo _letterInfo;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();


        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
    }

    public void Populate(SingleLetterInfo letterInfo)
    {
        _letterInfo = letterInfo;
        _letterLabel.text = letterInfo._letter.ToString();
        _pointsLabel.text = letterInfo._points.ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = false;

        // post a drag start event
        var evt = UILetterTileStartDragEvent.Get(this);
        GameEventHandler.Instance.TriggerEvent(evt);

        gameObject.transform.localScale = _selectedScale;
        _image.color = _selectedColor;
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
    }
}
