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
    [SerializeField] private float judgeRadius = 3f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI hitText;
    [SerializeField] private TextMeshProUGUI missText;
    [SerializeField] private TextMeshProUGUI judgeText;

    [Header("結果畫面")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button returnButton;

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
        if (resultPanel) resultPanel.SetActive(false);
        if (returnButton) returnButton.onClick.AddListener(OnReturnClicked);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J)) TryHit(RhythmNote.Lane.J, ringJTransform, ringJSprite);
        if (Input.GetKeyDown(KeyCode.K)) TryHit(RhythmNote.Lane.K, ringKTransform, ringKSprite);
        // 結果畫面顯示時按 F 返回
        if (resultPanel != null && resultPanel.activeSelf && Input.GetKeyDown(KeyCode.F))
            OnReturnClicked();
    }

    private void TryHit(RhythmNote.Lane lane, Transform ringTransform, SpriteRenderer ringSprite)
    {
        if (ringTransform == null) return;
        StartCoroutine(FlashRing(ringSprite));

        RhythmNote bestNote = null;
        float bestDist = float.MaxValue;

        foreach (var note in FindObjectsByType<RhythmNote>(FindObjectsSortMode.None))
        {
            if (note.lane != lane) continue;
            float d = Mathf.Abs(note.transform.position.x - ringTransform.position.x);
            if (d < bestDist) { bestDist = d; bestNote = note; }
        }

        if (bestNote == null || bestDist > judgeRadius) return;

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

    // 音符全部結束後由 RhythmSpawner 呼叫
    public void OnBattleEnd()
    {
        StartCoroutine(ShowResultRoutine());
    }

    private IEnumerator ShowResultRoutine()
    {
        yield return new WaitForSeconds(0.8f);

        if (resultPanel == null) yield break;

        bool isWin = hitCount >= missCount;
        resultPanel.SetActive(true);

        if (resultText)
        {
            resultText.text = isWin ? "戰鬥勝利" : "戰鬥失敗";
            resultText.color = isWin ? new Color(1f, 0.88f, 0.2f) : new Color(0.8f, 0.3f, 0.3f);
        }
    }

    private void OnReturnClicked()
    {
        if (resultPanel) resultPanel.SetActive(false);
        ResetCount();
        // 跳回戰鬥前記錄的場景
        string returnScene = BattleManager.Instance.PreviousSceneName;
        UnityEngine.SceneManagement.SceneManager.LoadScene(returnScene);
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