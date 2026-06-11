using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 拼圖目標卡槽
/// 掛載在卡槽 Prefab 上，顯示提示輪廓並接收對應碎片
/// </summary>
[RequireComponent(typeof(Image))]
public class PuzzleSlot : MonoBehaviour
{
    // ── 狀態 ──────────────────────────────────────────────────
    public int SlotIndex { get; private set; }
    public bool IsOccupied { get; private set; } = false;

    [Header("=== 外觀設定 ===")]
    [Tooltip("未放置時的卡槽底色（半透明即可）")]
    public Color emptyColor = new Color(1f, 1f, 1f, 0.15f);

    [Tooltip("碎片靠近時的高亮顏色")]
    public Color highlightColor = new Color(0.4f, 0.9f, 0.4f, 0.4f);

    [Tooltip("放置完成後的顏色")]
    public Color placedColor = new Color(1f, 1f, 1f, 1f);

    [Tooltip("卡槽邊框圖片（可為空，使用預設白色方塊也可）")]
    public Image borderImage;

    // ── 私有 ──────────────────────────────────────────────────
    private Image _bgImage;
    private PuzzleManager _manager;

    // ── 初始化 ────────────────────────────────────────────────

    public void Init(int index, Sprite hintSprite, PuzzleManager manager)
    {
        SlotIndex = index;
        _manager = manager;

        _bgImage = GetComponent<Image>();

        // 顯示淡化的提示圖作為背景
        _bgImage.sprite = hintSprite;
        _bgImage.color = emptyColor;
        _bgImage.preserveAspect = true;
    }

    // ── 公開方法 ──────────────────────────────────────────────

    /// <summary>由 PuzzlePiece 呼叫，確認此卡槽被正確填入</summary>
    public void OnPiecePlaced(PuzzlePiece piece)
    {
        IsOccupied = true;
        _bgImage.color = placedColor;

        // 顯示完整清晰圖
        _bgImage.sprite = piece.GetComponent<Image>().sprite;

        if (borderImage != null)
            borderImage.color = new Color(0.2f, 0.8f, 0.2f, 1f); // 綠色邊框

        _manager.OnPiecePlaced();
    }

    /// <summary>設定靠近時的高亮效果</summary>
    public void SetHighlight(bool highlight)
    {
        if (IsOccupied) return;
        _bgImage.color = highlight ? highlightColor : emptyColor;
    }
}
