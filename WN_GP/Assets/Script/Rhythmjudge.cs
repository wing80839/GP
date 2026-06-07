using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class RhythmJudge : MonoBehaviour
{
    public static RhythmJudge Instance { get; private set; }

    [Header("判定環的 Transform")]
    [SerializeField] private Transform ringJTransform;
    [SerializeField] private Transform ringKTransform;

    [Header("判定半徑")]
    [SerializeField] private float judgeRadius = 3f;  // 調這個到剛好碰到算 Good

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI hitText;
    [SerializeField] private TextMeshProUGUI missText;
    [SerializeField] private TextMeshProUGUI judgeText;

    [Header("判定環 SpriteRenderer")]
    [SerializeField] private SpriteRenderer ringJSprite;
    [SerializeField] private SpriteRenderer ringKSprite;

    [Header("判定環顏色")]
    [SerializeField] private Color ringNormalColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color ringHitColor = new Color(1f, 0.88f, 0.2f, 1f);

    private int hitCount = 0;
    private int missCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        SetRingColor(ringJSprite, ringNormalColor);
        SetRingColor(ringKSprite, ringNormalColor);
        UpdateCountUI();
        if (judgeText) judgeText.text = "";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J)) TryHit(RhythmNote.Lane.J, ringJTransform, ringJSprite);
        if (Input.GetKeyDown(KeyCode.K)) TryHit(RhythmNote.Lane.K, ringKTransform, ringKSprite);
    }

    private void TryHit(RhythmNote.Lane lane, Transform ringTransform, SpriteRenderer ringSprite)
    {
        if (ringTransform == null) return;
        StartCoroutine(FlashRing(ringSprite));

        // 找同軌道最近的音符（只比較 X 軸距離，忽略 Y / Z 差異）
        RhythmNote bestNote = null;
        float bestDist = float.MaxValue;

        foreach (var note in FindObjectsByType<RhythmNote>(FindObjectsSortMode.None))
        {
            if (note.lane != lane) continue;
            float d = Mathf.Abs(note.transform.position.x - ringTransform.position.x);
            if (d < bestDist) { bestDist = d; bestNote = note; }
        }

        // 沒有音符或距離太遠 → 空按忽略
        if (bestNote == null || bestDist > judgeRadius) return;

        // 越靠近中心越好：前半段 Perfect，後半段 Good
        if (bestDist <= judgeRadius * 0.5f)
        {
            HitNote(bestNote);
            ShowJudge("PERFECT", new Color(1f, 0.88f, 0.2f));
        }
        else
        {
            HitNote(bestNote);
            ShowJudge("GOOD", new Color(0.5f, 0.9f, 0.6f));
        }
    }

    private void HitNote(RhythmNote note)
    {
        note.SetJudged();
        Destroy(note.gameObject);
        hitCount++;
        UpdateCountUI();
    }

    public void OnNoteMiss()
    {
        missCount++;
        UpdateCountUI();
        ShowJudge("MISS", Color.red);
    }

    public void ResetCount()
    {
        hitCount = 0;
        missCount = 0;
        UpdateCountUI();
    }

    private void UpdateCountUI()
    {
        if (hitText) hitText.text = $"O：{hitCount}";
        if (missText) missText.text = $"X：{missCount}";
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