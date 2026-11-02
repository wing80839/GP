using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// NPC 對話（讀 .txt）。支援在文本裡寫選項和分支，選完選項會接著播對應的整段對話。
//
// 文本格式：
//   [NPC:名字]        之後的行是這個 NPC 說的（名字留空 = 旁白）
//   [PLAYER:名字]     之後的行是玩家說的
//   [CHOICE]          開始列選項，下一行起每行一個：  按鈕文字 -> 標籤
//   [LABEL:標籤]      分支的起點
//   [GOTO:標籤]       跳到某個標籤（例如兩個分支結束後接回同一段）
//   [END]             對話結束
//   一句話裡要換行用 <br>
public class NPCDialogue : MonoBehaviour, Interactable
{
    [Serializable]
    public class PortraitData
    {
        public string speakerName;  // 對應文本裡的名字
        public Sprite portrait;     // 該角色的立繪
        public bool isPlayer;       // 是否為玩家
    }

    [Serializable]
    public class OptionData
    {
        public string buttonLabel;
        public string resultText;
    }

    // ── Inspector 設定 ────────────────────────────────────────

    [Header("一進遊戲就自動播放（一般 NPC 不要勾）")]
    [SerializeField] private bool playOnStart = false;

    [Header("對話文本（.txt）")]
    [SerializeField] private TextAsset dialogueFile;

    [Header("立繪對應表（名字 → 圖片）")]
    [SerializeField] private PortraitData[] portraits;

    [Header("對話結束後跳出選項（文本裡沒寫 [CHOICE] 時才用）")]
    [SerializeField] private bool showOptionsAfter = false;

    [Header("選項（文本裡沒寫 [CHOICE] 時才用）")]
    [SerializeField] private OptionData[] options;

    [Header("對話框 UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image leftPortrait;   // 玩家立繪（左）
    [SerializeField] private Image rightPortrait;  // NPC 立繪（右）

    [Header("選項 UI")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TextMeshProUGUI[] optionLabels;

    [Header("結果 UI（舊的單句結果用）")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    // ── 內部資料 ─────────────────────────────────────────────

    private enum EntryType { Line, Choice, Label, Goto, End }

    private class Entry
    {
        public EntryType type;
        public string speakerName;
        public string text;            // Line 的內容；Label / Goto 的標籤名
        public bool isPlayer;
        public Sprite portrait;
        public List<string> choiceTexts = new List<string>();
        public List<string> choiceTargets = new List<string>();
    }

    private readonly List<Entry> _entries = new List<Entry>();
    private readonly Dictionary<string, int> _labels = new Dictionary<string, int>();
    private bool _hasChoices;

    private Canvas _canvas;
    private int _index;

    // 多個 NPC 共用同一個對話框時，只讓正在對話的那個回應點擊
    private static NPCDialogue _active;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic() => _active = null;

    // ── 生命週期 ─────────────────────────────────────────────

    private void Awake()
    {
        _canvas = GetComponentInChildren<Canvas>(true);
        if (_canvas) _canvas.gameObject.SetActive(false);
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
    }

    private void Start()
    {
        ParseDialogueFile();
        AddClickEvent(dialoguePanel, OnDialogueClicked);
        AddClickEvent(resultPanel, OnResultClicked);

        if (playOnStart) TriggerAction();
    }

    // ── 解析文本 ─────────────────────────────────────────────

    private void ParseDialogueFile()
    {
        _entries.Clear();
        _labels.Clear();
        _hasChoices = false;
        if (dialogueFile == null) return;

        string speaker = "";
        bool isPlayer = false;
        Sprite portrait = null;
        Entry choice = null;   // 正在讀的 [CHOICE] 區塊

        foreach (string raw in dialogueFile.text.Split('\n'))
        {
            string line = raw.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            bool isTag = line.StartsWith("[") && line.EndsWith("]");

            // 正在讀選項：每行「按鈕文字 -> 標籤」，遇到下一個 [標記] 就結束
            if (choice != null)
            {
                if (!isTag)
                {
                    int arrow = line.LastIndexOf("->", StringComparison.Ordinal);
                    if (arrow < 0)
                    {
                        Debug.LogWarning($"[對話] {dialogueFile.name}：選項「{line}」沒有寫「-> 標籤」");
                        continue;
                    }
                    choice.choiceTexts.Add(line.Substring(0, arrow).Trim());
                    choice.choiceTargets.Add(line.Substring(arrow + 2).Trim());
                    continue;
                }
                choice = null;
            }

            if (isTag)
            {
                string inner = line.Substring(1, line.Length - 2).Trim();
                int colon = inner.IndexOf(':');
                string key = (colon >= 0 ? inner.Substring(0, colon) : inner).Trim().ToUpper();
                string value = colon >= 0 ? inner.Substring(colon + 1).Trim() : "";

                switch (key)
                {
                    case "NPC":
                    case "PLAYER":
                        speaker = value;
                        isPlayer = key == "PLAYER";
                        portrait = FindPortrait(value);
                        continue;
                    case "CHOICE":
                        choice = new Entry { type = EntryType.Choice };
                        _entries.Add(choice);
                        _hasChoices = true;
                        continue;
                    case "LABEL":
                        _labels[value] = _entries.Count;
                        _entries.Add(new Entry { type = EntryType.Label, text = value });
                        continue;
                    case "GOTO":
                        _entries.Add(new Entry { type = EntryType.Goto, text = value });
                        continue;
                    case "END":
                        _entries.Add(new Entry { type = EntryType.End });
                        continue;
                }
                // 不認得的 [ ] 就當成一般台詞
            }

            _entries.Add(new Entry
            {
                type = EntryType.Line,
                speakerName = speaker,
                text = line,
                isPlayer = isPlayer,
                portrait = portrait
            });
        }

        // 檢查選項指到的標籤都存在
        foreach (var e in _entries)
            if (e.type == EntryType.Choice)
                foreach (var t in e.choiceTargets)
                    if (!_labels.ContainsKey(t))
                        Debug.LogWarning($"[對話] {dialogueFile.name}：找不到標籤 [LABEL:{t}]");
    }

    private Sprite FindPortrait(string name)
    {
        if (portraits == null) return null;
        foreach (var p in portraits)
            if (p.speakerName == name) return p.portrait;
        return null;
    }

    // ── Interactable 介面 ─────────────────────────────────────

    public void TriggerAction()
    {
        if (_active != null && _active != this) return;
        _active = this;

        _index = 0;
        if (_canvas) _canvas.gameObject.SetActive(true);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
        if (dialoguePanel) dialoguePanel.SetActive(true);
        Player.CanMove = false;
        Advance();
    }

    // ── 對話推進 ─────────────────────────────────────────────

    private void OnDialogueClicked()
    {
        if (_active != this) return;
        if (optionsPanel && optionsPanel.activeSelf) return;   // 選項出現時要點按鈕
        _index++;
        Advance();
    }

    // 從 _index 開始，一路處理標記，直到停在一句台詞或一組選項
    private void Advance()
    {
        int guard = 0;
        while (_index < _entries.Count && guard++ < 1000)
        {
            var e = _entries[_index];
            switch (e.type)
            {
                case EntryType.Line:
                    if (dialoguePanel) dialoguePanel.SetActive(true);
                    ShowLine(e);
                    return;

                case EntryType.Choice:
                    ShowChoice(e);
                    return;

                case EntryType.Label:
                    _index++;
                    continue;

                case EntryType.Goto:
                    if (_labels.TryGetValue(e.text, out int target)) _index = target;
                    else { Debug.LogWarning($"[對話] 找不到標籤 {e.text}"); _index++; }
                    continue;

                case EntryType.End:
                    CloseAll();
                    return;
            }
        }

        // 文本播完
        if (!_hasChoices && showOptionsAfter && options != null && options.Length > 0)
        {
            if (dialoguePanel) dialoguePanel.SetActive(false);
            ShowInspectorOptions();
        }
        else
        {
            CloseAll();
        }
    }

    private void ShowLine(Entry line)
    {
        if (speakerNameText) speakerNameText.text = line.speakerName;
        if (dialogueText) dialogueText.text = line.text;

        if (line.isPlayer)
        {
            // 玩家在左
            if (leftPortrait)
            {
                leftPortrait.gameObject.SetActive(line.portrait != null);
                if (line.portrait != null) leftPortrait.sprite = line.portrait;
            }
            if (rightPortrait) rightPortrait.gameObject.SetActive(false);
        }
        else
        {
            // NPC 在右
            if (rightPortrait)
            {
                rightPortrait.gameObject.SetActive(line.portrait != null);
                if (line.portrait != null) rightPortrait.sprite = line.portrait;
            }
            if (leftPortrait) leftPortrait.gameObject.SetActive(false);
        }
    }

    // ── 文本裡的選項 [CHOICE] ─────────────────────────────────

    private void ShowChoice(Entry choice)
    {
        // 對話框保留著（顯示選項前的最後一句），選項疊在上面
        SetupButtons(choice.choiceTexts, i =>
        {
            if (optionsPanel) optionsPanel.SetActive(false);
            string target = choice.choiceTargets[i];
            if (_labels.TryGetValue(target, out int idx)) _index = idx;
            else { Debug.LogWarning($"[對話] 找不到標籤 {target}"); _index++; }
            Advance();
        });
        if (optionsPanel) optionsPanel.SetActive(true);
    }

    // ── Inspector 裡的舊式選項（單句結果） ─────────────────────

    private void ShowInspectorOptions()
    {
        var labels = new List<string>();
        foreach (var o in options) labels.Add(o.buttonLabel);

        SetupButtons(labels, OnInspectorOptionSelected);
        if (resultPanel) resultPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(true);
    }

    private void OnInspectorOptionSelected(int idx)
    {
        if (options == null || idx >= options.Length) return;
        string text = options[idx].resultText;
        if (string.IsNullOrEmpty(text)) { CloseAll(); return; }
        if (optionsPanel) optionsPanel.SetActive(false);
        if (resultText) resultText.text = text;
        if (resultPanel) resultPanel.SetActive(true);
    }

    private void OnResultClicked()
    {
        if (_active != this) return;
        if (resultPanel) resultPanel.SetActive(false);
        ShowInspectorOptions();
    }

    // ── 按鈕 ─────────────────────────────────────────────────

    private void SetupButtons(List<string> labels, Action<int> onSelect)
    {
        if (optionButtons == null) return;

        if (labels.Count > optionButtons.Length)
            Debug.LogWarning($"[對話] 有 {labels.Count} 個選項，但只有 {optionButtons.Length} 顆按鈕");

        for (int i = 0; i < optionButtons.Length; i++)
        {
            var btn = optionButtons[i];
            if (btn == null) continue;

            if (i < labels.Count)
            {
                TMP_Text lbl = (optionLabels != null && i < optionLabels.Length && optionLabels[i] != null)
                    ? optionLabels[i]
                    : btn.GetComponentInChildren<TMP_Text>(true);
                if (lbl) lbl.text = labels[i];

                btn.gameObject.SetActive(true);
                btn.onClick.RemoveAllListeners();
                int idx = i;
                btn.onClick.AddListener(() => onSelect(idx));
            }
            else
            {
                btn.gameObject.SetActive(false);
            }
        }
    }

    // ── 工具 ─────────────────────────────────────────────────

    private void CloseAll()
    {
        if (_active == this) _active = null;
        if (optionsPanel) optionsPanel.SetActive(false);
        if (_canvas) _canvas.gameObject.SetActive(false);
        Player.CanMove = true;
    }

    private void AddClickEvent(GameObject obj, Action callback)
    {
        if (obj == null) return;
        var trigger = obj.GetComponent<EventTrigger>();
        if (trigger == null) trigger = obj.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((_) => callback());
        trigger.triggers.Add(entry);
    }
}