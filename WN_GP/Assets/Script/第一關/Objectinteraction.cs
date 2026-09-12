using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ObjectInteraction : MonoBehaviour, Interactable
{
    [Serializable]
    public class OptionData
    {
        public string buttonLabel;
        public string resultText;
    }

    [Header("主對話")]
    [SerializeField] private string openingText;

    [Header("選項內容（最多4個）")]
    [SerializeField] private OptionData[] options;

    [Header("UI 物件")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button[] buttons;
    [SerializeField] private TextMeshProUGUI[] buttonLabels;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    private Canvas _canvas;
    private int _buttonCount = 0;

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
        // 點擊對話框 Panel 繼續到選項
        if (dialoguePanel != null)
        {
            var trigger = dialoguePanel.GetComponent<EventTrigger>();
            if (trigger == null) trigger = dialoguePanel.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((_) => ShowOptions());
            trigger.triggers.Add(entry);
        }

        // 點擊結果面板關閉
        if (resultPanel != null)
        {
            var trigger = resultPanel.GetComponent<EventTrigger>();
            if (trigger == null) trigger = resultPanel.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((_) => ShowOptions());
            trigger.triggers.Add(entry);
        }

        // 設定選項按鈕
        _buttonCount = Mathf.Min(options.Length, buttons.Length);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            if (i < _buttonCount)
            {
                if (i < buttonLabels.Length && buttonLabels[i] != null)
                    buttonLabels[i].text = options[i].buttonLabel;
                buttons[i].gameObject.SetActive(true);
                int idx = i;
                buttons[i].onClick.AddListener(() => OnOptionSelected(idx));
            }
            else
            {
                buttons[i].gameObject.SetActive(false);
            }
        }
    }

    public void TriggerAction()
    {
        if (_canvas) _canvas.gameObject.SetActive(true);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
        if (dialogueText) dialogueText.text = openingText;
        if (dialoguePanel) dialoguePanel.SetActive(true);
        Player.CanMove = false;
    }

    private void ShowOptions()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(true);
    }

    private void OnOptionSelected(int idx)
    {
        if (idx >= options.Length) return;

        string text = options[idx].resultText;

        // Result 留空 = 結束關閉
        if (string.IsNullOrEmpty(text))
        {
            CloseAll();
            return;
        }

        if (optionsPanel) optionsPanel.SetActive(false);
        if (resultText) resultText.text = text;
        if (resultPanel) resultPanel.SetActive(true);
    }

    private void CloseAll()
    {
        if (_canvas) _canvas.gameObject.SetActive(false);
        Player.CanMove = true;
    }
}