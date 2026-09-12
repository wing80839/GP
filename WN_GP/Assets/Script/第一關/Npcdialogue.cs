using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NPCDialogue : MonoBehaviour, Interactable
{
    [Serializable]
    public class PortraitData
    {
        public string speakerName;  // 對應文本裡的名字
        public Sprite portrait;     // 該角色的立繪
        public bool isPlayer;     // 是否為玩家
    }

    [Serializable]
    public class OptionData
    {
        public string buttonLabel;
        public string resultText;
    }

    // ── Inspector 設定 ────────────────────────────────────────

    [Header("對話文本（.txt）")]
    [SerializeField] private TextAsset dialogueFile;

    [Header("立繪對應表（名字 → 圖片）")]
    [SerializeField] private PortraitData[] portraits;

    [Header("對話結束後跳出選項")]
    [SerializeField] private bool showOptionsAfter = false;

    [Header("選項")]
    [SerializeField] private OptionData[] options;

    [Header("對話框 UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image leftPortrait;   // NPC 立繪（左）
    [SerializeField] private Image rightPortrait;  // 玩家立繪（右）

    [Header("選項 UI")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TextMeshProUGUI[] optionLabels;

    [Header("結果 UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    // ── 內部資料 ─────────────────────────────────────────────

    private struct ParsedLine
    {
        public string speakerName;
        public string text;
        public bool isPlayer;
        public Sprite portrait;
    }

    private List<ParsedLine> _lines = new List<ParsedLine>();
    private Canvas _canvas;
    private int _lineIndex = 0;

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
        AddClickEvent(resultPanel, ShowOptions);
        SetupOptionButtons();
    }

    // ── 解析文本 ─────────────────────────────────────────────

    private void ParseDialogueFile()
    {
        _lines.Clear();
        if (dialogueFile == null) return;

        string currentSpeaker = "";
        bool currentIsPlayer = false;
        Sprite currentPortrait = null;

        foreach (string raw in dialogueFile.text.Split('\n'))
        {
            string line = raw.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // 解析說話者標記 [NPC:名字] 或 [PLAYER:名字]
            if (line.StartsWith("[") && line.Contains(":") && line.EndsWith("]"))
            {
                string inner = line.Substring(1, line.Length - 2); // 去掉 [ ]
                string[] parts = inner.Split(':');
                string role = parts[0].ToUpper();
                string name = parts.Length > 1 ? parts[1] : "";

                currentSpeaker = name;
                currentIsPlayer = role == "PLAYER";
                currentPortrait = FindPortrait(name);
            }
            else
            {
                // 一般對話行
                _lines.Add(new ParsedLine
                {
                    speakerName = currentSpeaker,
                    text = line,
                    isPlayer = currentIsPlayer,
                    portrait = currentPortrait
                });
            }
        }
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
        _lineIndex = 0;
        if (_canvas) _canvas.gameObject.SetActive(true);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
        if (dialoguePanel) dialoguePanel.SetActive(true);
        Player.CanMove = false;
        ShowLine(_lineIndex);
    }

    // ── 對話推進 ─────────────────────────────────────────────

    private void OnDialogueClicked()
    {
        _lineIndex++;
        if (_lineIndex < _lines.Count)
        {
            ShowLine(_lineIndex);
        }
        else
        {
            if (showOptionsAfter)
            {
                dialoguePanel.SetActive(false);
                ShowOptions();
            }
            else
            {
                CloseAll();
            }
        }
    }

    private void ShowLine(int index)
    {
        if (index >= _lines.Count) return;
        var line = _lines[index];

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

    // ── 選項 ─────────────────────────────────────────────────

    private void ShowOptions()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(true);
    }

    private void OnOptionSelected(int idx)
    {
        if (options == null || idx >= options.Length) return;
        string text = options[idx].resultText;
        if (string.IsNullOrEmpty(text)) { CloseAll(); return; }
        if (optionsPanel) optionsPanel.SetActive(false);
        if (resultText) resultText.text = text;
        if (resultPanel) resultPanel.SetActive(true);
    }

    private void SetupOptionButtons()
    {
        int count = Mathf.Min(
            options != null ? options.Length : 0,
            optionButtons != null ? optionButtons.Length : 0
        );
        for (int i = 0; i < (optionButtons?.Length ?? 0); i++)
        {
            if (optionButtons[i] == null) continue;
            if (i < count)
            {
                if (i < optionLabels.Length && optionLabels[i] != null)
                    optionLabels[i].text = options[i].buttonLabel;
                optionButtons[i].gameObject.SetActive(true);
                int idx = i;
                optionButtons[i].onClick.AddListener(() => OnOptionSelected(idx));
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // ── 工具 ─────────────────────────────────────────────────

    private void CloseAll()
    {
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