using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 拼圖碎片
/// 掛載在每個碎片 Prefab 上，處理拖曳、吸附與鎖定
/// </summary>
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class PuzzlePiece : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    // ── 設定（由 PuzzleManager.Init 注入）──────────────────────
    public int PieceIndex { get; private set; }

    [Header("=== 吸附設定 ===")]
    [Tooltip("距離卡槽中心多少像素內自動吸附")]
    public float snapDistance = 60f;

    [Tooltip("吸附動畫速度（越大越快）")]
    public float snapLerpSpeed = 15f;

    [Tooltip("已放置時的碎片透明度")]
    [Range(0f, 1f)]
    public float placedAlpha = 0.5f;

    // ── 私有狀態 ──────────────────────────────────────────────
    private Image _image;
    private CanvasGroup _cg;
    private RectTransform _rt;
    private Canvas _rootCanvas;

    private List<PuzzleSlot> _allSlots;
    private PuzzleSlot _nearestSlot;
    private Vector2 _dragOffset;
    private bool _isPlaced = false;
    private bool _isSnapping = false;

    // 拖曳前的層級（用於拖曳時置頂）
    private Transform _originalParent;
    private int _originalSiblingIndex;

    // ── 初始化 ────────────────────────────────────────────────

    public void Init(int index, Sprite sprite, List<PuzzleSlot> slots)
    {
        PieceIndex = index;
        _allSlots = slots;

        _image = GetComponent<Image>();
        _image.sprite = sprite;
        _image.preserveAspect = true;

        _cg = GetComponent<CanvasGroup>();
        _rt = GetComponent<RectTransform>();
        _rootCanvas = GetComponentInParent<Canvas>();
    }

    // ── Unity 事件 ────────────────────────────────────────────

    void Update()
    {
        if (_isSnapping)
        {
            _rt.position = Vector3.Lerp(_rt.position, _nearestSlot.transform.position, snapLerpSpeed * Time.deltaTime);
            if (Vector2.Distance(_rt.position, _nearestSlot.transform.position) < 0.5f)
            {
                _rt.position = _nearestSlot.transform.position;
                ConfirmPlacement();
            }
        }
    }

    // ── 拖曳介面實作 ──────────────────────────────────────────

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isPlaced) return;

        // 置頂顯示
        _originalParent = transform.parent;
        _originalSiblingIndex = transform.GetSiblingIndex();
        transform.SetParent(_rootCanvas.transform);
        transform.SetAsLastSibling();

        _cg.blocksRaycasts = false;

        // 計算滑鼠與碎片中心的偏移
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rt, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        _dragOffset = localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isPlaced) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rootCanvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        _rt.localPosition = localPoint - _dragOffset;

        // 即時高亮最近卡槽
        HighlightNearestSlot();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isPlaced) return;

        _cg.blocksRaycasts = true;

        PuzzleSlot target = FindNearestSlot();
        if (target != null && !target.IsOccupied)
        {
            _nearestSlot = target;
            _isSnapping = true;
        }
        else
        {
            // 放回原本父層（散落區）
            transform.SetParent(_originalParent);
            transform.SetSiblingIndex(_originalSiblingIndex);
            ClearSlotHighlights();
        }
    }

    // ── 指標提示 ──────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isPlaced)
            transform.localScale = Vector3.one * 1.05f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }

    // ── 私有方法 ──────────────────────────────────────────────

    /// <summary>找出距離最近且匹配的卡槽（Index 相符才算）</summary>
    private PuzzleSlot FindNearestSlot()
    {
        PuzzleSlot best = null;
        float bestDist = snapDistance;

        foreach (PuzzleSlot slot in _allSlots)
        {
            if (slot.IsOccupied) continue;
            if (slot.SlotIndex != PieceIndex) continue; // 必須是對應的卡槽

            float dist = Vector2.Distance(_rt.position, slot.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = slot;
            }
        }
        return best;
    }

    private void HighlightNearestSlot()
    {
        PuzzleSlot nearest = FindNearestSlot();
        foreach (var slot in _allSlots)
            slot.SetHighlight(slot == nearest);
    }

    private void ClearSlotHighlights()
    {
        foreach (var slot in _allSlots)
            slot.SetHighlight(false);
    }

    private void ConfirmPlacement()
    {
        _isSnapping = false;
        _isPlaced = true;

        // 視覺上半透明，表示已放置
        _cg.alpha = placedAlpha;
        _cg.blocksRaycasts = false;

        ClearSlotHighlights();
        _nearestSlot.OnPiecePlaced(this);

        Debug.Log($"[PuzzlePiece] 碎片 {PieceIndex} 已正確放置 ✅");
    }
}
