using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ObjectInteraction : MonoBehaviour, Interactable
{
    // ── 資料結構 ─────────────────────────────────────────────

    [Serializable]
    public class OptionData
    {
        public string buttonLabel;
        [TextArea(2, 5)]
        public string[] resultLines;        // 多行結果對話
        public OptionData[] nextOptions;    // 結果後的下一層選項（留空=關閉）
    }

    // ── Inspector ────────────────────────────────────────────

    [Header("主對話（多行，點擊推進）")]
    [TextArea(2, 5)]
    [SerializeField] private string[] openingLines;

    [Header("對話結束後跳出選項")]
    [SerializeField] private bool showOptionsAfter = false;

    [Header("選項（支援多層）")]
    [SerializeField] private OptionData[] options;

    [Header("UI 物件")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TextMeshProUGUI[] optionLabels;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    // ── 內部狀態 ─────────────────────────────────────────────

    private Canvas _canvas;
    private int _openingIndex = 0;  // 主對話目前行
    private int _resultIndex = 0;  // 結果對話目前行
    private string[] _currentLines;       // 目前顯示的結果行
    private OptionData[] _currentOptions;     // 目前層的選項

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
        // 點擊主對話框推進
        AddClickEvent(dialoguePanel, OnDialogueClicked);

        // 點擊結果框推進結果對話
        AddClickEvent(resultPanel, OnResultClicked);
    }

    // ── Interactable ─────────────────────────────────────────

    public void TriggerAction()
    {
        _openingIndex = 0;
        _currentOptions = options;

        if (_canvas) _canvas.gameObject.SetActive(true);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
        if (dialoguePanel) dialoguePanel.SetActive(true);

        Player.CanMove = false;

        // 顯示第一行主對話
        if (openingLines != null && openingLines.Length > 0)
            dialogueText.text = openingLines[0];
    }

    // ── 主對話推進 ───────────────────────────────────────────

    private void OnDialogueClicked()
    {
        _openingIndex++;

        if (openingLines != null && _openingIndex < openingLines.Length)
        {
            // 還有下一行主對話
            dialogueText.text = openingLines[_openingIndex];
        }
        else
        {
            // 主對話結束
            if (showOptionsAfter)
            {
                dialoguePanel.SetActive(false);
                ShowOptions(_currentOptions);
            }
            else
            {
                CloseAll();
            }
        }
    }

    // ── 選項顯示 ─────────────────────────────────────────────

    private void ShowOptions(OptionData[] opts)
    {
        if (opts == null || opts.Length == 0) { CloseAll(); return; }

        _currentOptions = opts;

        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);

        // 設定按鈕
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] == null) continue;

            if (i < opts.Length)
            {
                if (i < optionLabels.Length && optionLabels[i] != null)
                    optionLabels[i].text = opts[i].buttonLabel;

                optionButtons[i].gameObject.SetActive(true);
                optionButtons[i].onClick.RemoveAllListeners();

                int idx = i;
                optionButtons[i].onClick.AddListener(() => OnOptionSelected(idx));
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }

        if (optionsPanel) optionsPanel.SetActive(true);
    }

    private void OnOptionSelected(int idx)
    {
        if (_currentOptions == null || idx >= _currentOptions.Length) return;

        var opt = _currentOptions[idx];

        // 沒有結果文字 → 直接關閉
        if (opt.resultLines == null || opt.resultLines.Length == 0)
        {
            CloseAll();
            return;
        }

        // 顯示結果對話（第一行）
        _currentLines = opt.resultLines;
        _resultIndex = 0;

        if (optionsPanel) optionsPanel.SetActive(false);
        resultText.text = _currentLines[0];
        if (resultPanel) resultPanel.SetActive(true);

        // 記住這個選項的下一層選項
        _currentOptions = opt.nextOptions;
    }

    // ── 結果對話推進 ─────────────────────────────────────────

    private void OnResultClicked()
    {
        _resultIndex++;

        if (_resultIndex < _currentLines.Length)
        {
            // 還有下一行結果
            resultText.text = _currentLines[_resultIndex];
        }
        else
        {
            // 結果結束 → 跳下一層選項或關閉
            if (_currentOptions != null && _currentOptions.Length > 0)
                ShowOptions(_currentOptions);
            else
                CloseAll();
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