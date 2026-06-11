using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 拼圖核心管理器
/// 掛載於場景中的空物件上，負責切割圖片、生成碎片與監控完成狀態
/// </summary>
public class PuzzleManager : MonoBehaviour
{
    [Header("=== 拼圖設定 ===")]
    [Tooltip("作為拼圖來源的圖片（需設為 Sprite 格式，Read/Write Enabled = true）")]
    public Sprite puzzleImage;

    [Tooltip("拼圖片數（4 ~ 7）")]
    [Range(4, 7)]
    public int pieceCount = 6;

    [Header("=== UI 參考 ===")]
    [Tooltip("放置碎片的散落區域（Canvas 下的 Panel）")]
    public RectTransform pieceSpawnArea;

    [Tooltip("放置目標卡槽的區域（Canvas 下的 Panel）")]
    public RectTransform slotArea;

    [Tooltip("完成時顯示的UI物件（可為空）")]
    public GameObject completionUI;

    [Header("=== 碎片外觀 ===")]
    [Tooltip("每個碎片的顯示大小（像素）")]
    public Vector2 pieceDisplaySize = new Vector2(120f, 120f);

    [Tooltip("卡槽之間的間距")]
    public float slotSpacing = 10f;

    [Header("=== 預製物 ===")]
    [Tooltip("拼圖碎片 Prefab（內含 Image + PuzzlePiece 元件）")]
    public GameObject piecePrefab;

    [Tooltip("目標卡槽 Prefab（內含 Image + PuzzleSlot 元件）")]
    public GameObject slotPrefab;

    // ── 內部狀態 ──────────────────────────────────────────────
    private List<PuzzleSlot> _slots = new List<PuzzleSlot>();
    private int _completedCount = 0;

    // ─────────────────────────────────────────────────────────
    void Start()
    {
        if (puzzleImage == null)
        {
            Debug.LogError("[PuzzleManager] 請在 Inspector 指定 puzzleImage！");
            return;
        }
        GeneratePuzzle();
    }

    // ── 公開介面 ──────────────────────────────────────────────

    /// <summary>由 PuzzleSlot 呼叫，通知一個碎片已正確放置</summary>
    public void OnPiecePlaced()
    {
        _completedCount++;
        if (_completedCount >= pieceCount)
            StartCoroutine(ShowCompletion());
    }

    // ── 核心邏輯 ──────────────────────────────────────────────

    private void GeneratePuzzle()
    {
        // 計算切割方式：優先嘗試較接近正方形的分割
        GetGridSize(pieceCount, out int cols, out int rows);

        Texture2D srcTex = GetReadableTexture(puzzleImage);
        int texW = srcTex.width;
        int texH = srcTex.height;

        int cellW = texW / cols;
        int cellH = texH / rows;

        // 先生成卡槽，再生成散落碎片
        List<(Sprite sprite, int index)> pieces = new List<(Sprite, int)>();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int index = r * cols + c;
                if (index >= pieceCount) break;

                // 裁切子圖（UV 座標由左下起）
                int x = c * cellW;
                int y = texH - (r + 1) * cellH; // Unity texture Y 由下往上
                Rect rect = new Rect(x, y, cellW, cellH);
                Sprite pieceSprite = Sprite.Create(srcTex, rect, new Vector2(0.5f, 0.5f));

                pieces.Add((pieceSprite, index));

                // 建立目標卡槽
                CreateSlot(pieceSprite, index, c, r, cols, rows);
            }
        }

        // 打亂順序後散落生成碎片
        ShuffleList(pieces);
        foreach (var (sprite, idx) in pieces)
            CreatePiece(sprite, idx);
    }

    private void CreateSlot(Sprite sprite, int index, int col, int row, int cols, int rows)
    {
        GameObject go = Instantiate(slotPrefab, slotArea);
        RectTransform rt = go.GetComponent<RectTransform>();

        // 依格子位置排列卡槽
        float totalW = cols * (pieceDisplaySize.x + slotSpacing) - slotSpacing;
        float totalH = rows * (pieceDisplaySize.y + slotSpacing) - slotSpacing;
        float startX = -totalW / 2f + pieceDisplaySize.x / 2f;
        float startY =  totalH / 2f - pieceDisplaySize.y / 2f;

        rt.anchoredPosition = new Vector2(
            startX + col * (pieceDisplaySize.x + slotSpacing),
            startY - row * (pieceDisplaySize.y + slotSpacing)
        );
        rt.sizeDelta = pieceDisplaySize;

        PuzzleSlot slot = go.GetComponent<PuzzleSlot>();
        slot.Init(index, sprite, this);
        _slots.Add(slot);
    }

    private void CreatePiece(Sprite sprite, int index)
    {
        GameObject go = Instantiate(piecePrefab, pieceSpawnArea);
        RectTransform rt = go.GetComponent<RectTransform>();

        // 在散落區域內隨機位置
        float hw = pieceSpawnArea.rect.width  / 2f - pieceDisplaySize.x / 2f;
        float hh = pieceSpawnArea.rect.height / 2f - pieceDisplaySize.y / 2f;
        rt.anchoredPosition = new Vector2(
            Random.Range(-hw, hw),
            Random.Range(-hh, hh)
        );
        rt.sizeDelta = pieceDisplaySize;

        PuzzlePiece piece = go.GetComponent<PuzzlePiece>();
        piece.Init(index, sprite, _slots);
    }

    private IEnumerator ShowCompletion()
    {
        yield return new WaitForSeconds(0.3f);
        Debug.Log("[PuzzleManager] 🎉 拼圖完成！");
        if (completionUI != null)
            completionUI.SetActive(true);
    }

    // ── 工具方法 ──────────────────────────────────────────────

    /// <summary>決定最佳 cols × rows 分割（盡量接近正方形，且 cols*rows >= pieceCount）</summary>
    private void GetGridSize(int count, out int cols, out int rows)
    {
        // 預設表：4→2×2, 5→3×2, 6→3×2, 7→4×2
        switch (count)
        {
            case 4: cols = 2; rows = 2; break;
            case 5: cols = 3; rows = 2; break;
            case 6: cols = 3; rows = 2; break;
            case 7: cols = 4; rows = 2; break;
            default: cols = 3; rows = 2; break;
        }
    }

    /// <summary>將 Sprite 的 Texture 轉為可讀取版本（繞過 Read/Write 限制）</summary>
    private Texture2D GetReadableTexture(Sprite sprite)
    {
        Texture2D src = sprite.texture;
        RenderTexture rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(src, rt);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D readable = new Texture2D(src.width, src.height);
        readable.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readable.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return readable;
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
