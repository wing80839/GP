using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 負責偵測 J / K 按鍵，判定音符時機，顯示判定文字。
/// 將此腳本掛在場景中的 GameManager 或 BattleCanvas 物件上。
/// </summary>
public class RhythmJudge : MonoBehaviour
{
    public static RhythmJudge Instance { get; private set; }

    [Header("判定線位置（世界座標 X）")]
    [SerializeField] private float judgeLineX = -5f;

    [Header("判定範圍（世界單位）")]
    [SerializeField] private float perfectRange = 0.4f;
    [SerializeField] private float goodRange = 0.8f;

    [Header("UI")]
    [SerializeField] private Text judgeText;   // 顯示 PERFECT / GOOD / MISS
    [SerializeField] private Text comboText;   // 顯示 Combo 數字

    [Header("J / K 判定環（SpriteRenderer）")]
    [SerializeField] private SpriteRenderer ringJ;
    [SerializeField] private SpriteRenderer ringK;

    [Header("判定環顏色")]
    [SerializeField] private Color ringNormalColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color ringHitColor = new Color(1f, 0.88f, 0.2f, 1f);

    // ── 內部狀態 ─────────────────────────────────────────────
    private int combo = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        SetRingColor(ringJ, ringNormalColor);
        SetRingColor(ringK, ringNormalColor);
        UpdateComboUI();
        if (judgeText) judgeText.text = "";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J)) TryHit(RhythmNote.Lane.J, ringJ);
        if (Input.GetKeyDown(KeyCode.K)) TryHit(RhythmNote.Lane.K, ringK);
    }

    // ── 判定邏輯 ─────────────────────────────────────────────

    private void TryHit(RhythmNote.Lane lane, SpriteRenderer ring)
    {
        StartCoroutine(FlashRing(ring));

        // 找最近的對應軌道音符
        RhythmNote closest = FindClosestNote(lane);

        if (closest == null)
        {
            // 空打
            BreakCombo();
            ShowJudge("MISS", Color.red);
            return;
        }

        float dist = Mathf.Abs(closest.transform.position.x - judgeLineX);

        if (dist <= perfectRange)
        {
            closest.SetJudged();
            Destroy(closest.gameObject);
            combo++;
            UpdateComboUI();
            ShowJudge("PERFECT", new Color(1f, 0.88f, 0.2f));
        }
        else if (dist <= goodRange)
        {
            closest.SetJudged();
            Destroy(closest.gameObject);
            combo++;
            UpdateComboUI();
            ShowJudge("GOOD", new Color(0.5f, 0.9f, 0.6f));
        }
        else
        {
            // 按太早或太晚
            BreakCombo();
            ShowJudge("MISS", Color.red);
        }
    }

    /// <summary>找同軌道中最靠近判定線的音符</summary>
    private RhythmNote FindClosestNote(RhythmNote.Lane lane)
    {
        RhythmNote best = null;
        float bestDist = float.MaxValue;

        foreach (var note in FindObjectsByType<RhythmNote>(FindObjectsSortMode.None))
        {
            if (note.lane != lane) continue;
            float d = Mathf.Abs(note.transform.position.x - judgeLineX);
            if (d < bestDist) { bestDist = d; best = note; }
        }
        return best;
    }

    // ── Miss（音符自動超線呼叫）──────────────────────────────

    public void OnNoteMiss()
    {
        BreakCombo();
        ShowJudge("MISS", Color.red);
    }

    // ── UI 輔助 ──────────────────────────────────────────────

    private void BreakCombo()
    {
        combo = 0;
        UpdateComboUI();
    }

    private void UpdateComboUI()
    {
        if (comboText) comboText.text = combo > 1 ? $"{combo} COMBO" : "";
    }

    private Coroutine _judgeCoroutine;

    private void ShowJudge(string text, Color color)
    {
        if (judgeText == null) return;
        if (_judgeCoroutine != null) StopCoroutine(_judgeCoroutine);
        _judgeCoroutine = StartCoroutine(ShowJudgeRoutine(text, color));
    }

    private IEnumerator ShowJudgeRoutine(string text, Color color)
    {
        judgeText.text = text;
        judgeText.color = color;
        yield return new WaitForSeconds(0.5f);
        judgeText.text = "";
    }

    private IEnumerator FlashRing(SpriteRenderer ring)
    {
        SetRingColor(ring, ringHitColor);
        yield return new WaitForSeconds(0.12f);
        SetRingColor(ring, ringNormalColor);
    }

    private void SetRingColor(SpriteRenderer ring, Color color)
    {
        if (ring) ring.color = color;
    }
}