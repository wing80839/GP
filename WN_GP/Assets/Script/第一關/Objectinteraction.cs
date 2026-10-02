using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ObjectInteraction : MonoBehaviour, Interactable
{
    // ── 資料結構 ─────────────────────────────────────────────

    public enum OptionAction
    {
        None,     // 一般選項：顯示結果對話
        PickUp,   // 帶走：把 item 放進背包
        UseItem   // 使用：跳出背包讓玩家選道具
    }

    [Serializable]
    public class OptionData
    {
        public string buttonLabel;
        public OptionAction action = OptionAction.None;
        [Tooltip("action = PickUp 時要放進背包的道具")]
        public ItemData item;
        [TextArea(2, 5)]
        public string[] resultLines;        // 多行結果對話（可用 {item} 代表道具名）
        public OptionData[] nextOptions;    // 結果後的下一層選項（留空=關閉）

        [Tooltip("選到這個選項時觸發，例如理智值 +3")]
        public UnityEvent onSelected;
        public bool eventOnce = true;       // onSelected 只觸發一次（避免重複刷理智值）

        [NonSerialized] public bool used;   // 帶走後就不再顯示這個選項
        [NonSerialized] public bool eventFired;
    }

    [Serializable]
    public class UseRule
    {
        public ItemData requiredItem;       // 用哪個道具才有效
        public bool consumeItem = true;     // 用完從背包移除
        public bool oneTimeOnly = true;
        [TextArea(2, 5)]
        public string[] successLines;       // 成功時的對話
        public OptionData[] nextOptions;    // 成功後的下一層選項（留空=關閉）
        public UnityEvent onSuccess;        // 例如：開門、換圖、播動畫

        [NonSerialized] public bool done;
    }

    // ── Inspector ────────────────────────────────────────────

    [Header("主對話（多行，點擊推進）")]
    [TextArea(2, 5)]
    [SerializeField] private string[] openingLines;

    [Header("鎖定狀態（例如還沒放椅子，拿不到東西）")]
    [SerializeField] private bool startLocked = false;
    [Tooltip("鎖定時說這些，取代主對話；鎖定時不會給道具")]
    [TextArea(2, 5)]
    [SerializeField] private string[] lockedLines;
    [SerializeField] private bool showOptionsWhenLocked = true;
    [Tooltip("鎖定對話說完後，不出選項，直接跳出背包讓玩家選道具（背包是空的就直接結束）")]
    [SerializeField] private bool openBagWhenLocked = false;

    [Header("主對話結束後自動獲得道具（不需選項，留空=不給）")]
    [SerializeField] private ItemData giveItem;
    [Tooltip("已經拿過道具後，再調查時改說這些（留空=照舊說主對話，但不會再給）")]
    [TextArea(2, 5)]
    [SerializeField] private string[] afterGivenLines;
    [Tooltip("拿到道具時觸發，例如把畫面上的鑰匙藏起來")]
    [SerializeField] private UnityEvent onItemGiven;

    [Header("對話結束後跳出選項")]
    [SerializeField] private bool showOptionsAfter = false;

    [Header("選項（支援多層）")]
    [SerializeField] private OptionData[] options;

    [Header("帶走")]
    [SerializeField] private bool hideAfterPickUp = true;   // 對話結束後讓物件消失

    [Header("使用道具")]
    [Tooltip("背包是空的時候，不顯示「使用」選項")]
    [SerializeField] private bool hideUseWhenBagEmpty = true;
    [SerializeField] private UseRule[] useRules;
    [TextArea(2, 5)]
    [SerializeField] private string[] wrongItemLines = { "對它使用「{item}」……好像沒什麼反應。" };
    [TextArea(2, 5)]
    [SerializeField] private string[] emptyBagLines = { "背包裡什麼都沒有。" };

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
    private int _openingIndex = 0;
    private int _resultIndex = 0;
    private string[] _currentLines;              // 目前顯示的結果行
    private OptionData[] _currentOptions;        // 目前顯示中的這一層選項
    private OptionData[] _pendingNext;           // 結果對話結束後要跳的選項
    private readonly List<OptionData> _visible = new List<OptionData>();  // 實際顯示的按鈕
    private string _itemName = "";
    private bool _hideOnClose;
    private bool _given;                 // giveItem 是否已給過
    private string[] _activeOpening;     // 這次要說的主對話
    private bool _locked;
    private Action _afterResultAction;   // 結果對話說完後要接著做的事（例如跳出背包）
    private bool _showingResult;         // 目前顯示的是結果對話（不是主對話）

    // 目前正在對話的物件。多個物件共用同一個對話框時，只讓這個物件回應點擊
    private static ObjectInteraction _active;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic() => _active = null;

    // 給 UnityEvent 呼叫：例如椅子放好後解鎖書櫃
    public void Unlock() => _locked = false;
    public void Lock() => _locked = true;

    // ── 生命週期 ─────────────────────────────────────────────

    private void Awake()
    {
        _locked = startLocked;
        _canvas = GetComponentInChildren<Canvas>(true);

        // 沒有另外做結果框 → 直接用主對話框顯示結果
        if (resultPanel == null) resultPanel = dialoguePanel;
        if (resultText == null) resultText = dialogueText;
        if (_canvas) _canvas.gameObject.SetActive(false);
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
    }

    private void Start()
    {
        if (resultPanel == dialoguePanel)
        {
            // 對話框和結果框是同一個 → 依目前狀態決定點擊要推進哪一種
            AddClickEvent(dialoguePanel, () =>
            {
                if (_showingResult) OnResultClicked();
                else OnDialogueClicked();
            });
        }
        else
        {
            AddClickEvent(dialoguePanel, OnDialogueClicked);
            AddClickEvent(resultPanel, OnResultClicked);
        }
    }

    // ── Interactable ─────────────────────────────────────────

    public void TriggerAction()
    {
        if (_active != null && _active != this) return;   // 別的物件正在對話
        _active = this;

        _openingIndex = 0;
        _showingResult = false;
        _currentOptions = options;

        if (_canvas) _canvas.gameObject.SetActive(true);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
        if (dialoguePanel) dialoguePanel.SetActive(true);

        Player.CanMove = false;

        // 已經拿過道具 → 改說 afterGivenLines
        if (_locked)
            _activeOpening = lockedLines;
        else if (_given && afterGivenLines != null && afterGivenLines.Length > 0)
            _activeOpening = afterGivenLines;
        else
            _activeOpening = openingLines;

        if (_activeOpening != null && _activeOpening.Length > 0)
            dialogueText.text = _activeOpening[0];
    }

    // ── 主對話推進 ───────────────────────────────────────────

    private void OnDialogueClicked()
    {
        if (_active != this) return;

        _openingIndex++;

        if (_activeOpening != null && _openingIndex < _activeOpening.Length)
        {
            dialogueText.text = _activeOpening[_openingIndex];
        }
        else
        {
            // 主對話說完 → 自動獲得道具（只給一次）
            if (!_locked && giveItem != null && !_given)
            {
                Inventory.Instance.Add(giveItem);
                _given = true;
                onItemGiven?.Invoke();
            }

            // 鎖定中：直接跳出背包選道具
            if (_locked && openBagWhenLocked)
            {
                dialoguePanel.SetActive(false);
                if (Inventory.Instance.Items.Count == 0) { CloseAll(); return; }
                _currentOptions = null;      // 取消或用錯道具 → 直接結束
                OpenBagForUse();
                return;
            }

            bool show = _locked ? showOptionsWhenLocked : showOptionsAfter;
            if (show)
            {
                dialoguePanel.SetActive(false);
                ShowOptions(options);
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
        _currentOptions = opts;

        // 過濾掉已經帶走的選項
        _visible.Clear();
        bool bagEmpty = Inventory.Instance == null || Inventory.Instance.Items.Count == 0;
        bool hidUse = false;
        if (opts != null)
            foreach (var o in opts)
            {
                if (o == null) continue;
                if (o.action == OptionAction.PickUp && o.used) continue;
                // 背包是空的 → 不顯示「使用」
                if (o.action == OptionAction.UseItem && hideUseWhenBagEmpty && bagEmpty) { hidUse = true; continue; }
                _visible.Add(o);
            }

        // 「使用」被藏起來後，如果只剩「離開」這種純關閉的選項，就不用跳選項了
        if (hidUse && _visible.TrueForAll(o => o.action == OptionAction.None
                && (o.resultLines == null || o.resultLines.Length == 0)
                && (o.nextOptions == null || o.nextOptions.Length == 0)))
            _visible.Clear();

        if (_visible.Count == 0) { CloseAll(); return; }
        _showingResult = false;

        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] == null) continue;

            if (i < _visible.Count)
            {
                // 沒拖 Option Labels 也沒關係，會自動找按鈕底下的文字
                TMP_Text lbl = (optionLabels != null && i < optionLabels.Length && optionLabels[i] != null)
                    ? optionLabels[i]
                    : optionButtons[i].GetComponentInChildren<TMP_Text>(true);
                if (lbl != null) lbl.text = _visible[i].buttonLabel;

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
        if (idx >= _visible.Count) return;
        var opt = _visible[idx];

        if (!(opt.eventOnce && opt.eventFired))
        {
            opt.eventFired = true;
            opt.onSelected?.Invoke();
        }

        switch (opt.action)
        {
            case OptionAction.PickUp:
                DoPickUp(opt);
                break;
            case OptionAction.UseItem:
                bool empty = Inventory.Instance.Items.Count == 0;
                if (!empty && opt.resultLines != null && opt.resultLines.Length > 0)
                {
                    // 先說提示（例如「請選擇使用的物品。」），點掉後再跳出背包
                    _afterResultAction = OpenBagForUse;
                    ShowResult(opt.resultLines, null);
                }
                else
                {
                    OpenBagForUse();   // 背包是空的 → 會說 Empty Bag Lines
                }
                break;
            default:
                ShowResult(opt.resultLines, opt.nextOptions);
                break;
        }
    }

    // ── 帶走 ─────────────────────────────────────────────────

    private void DoPickUp(OptionData opt)
    {
        if (opt.item == null)
        {
            Debug.LogWarning($"{name}：PickUp 選項沒有設定 item");
            ShowResult(opt.resultLines, opt.nextOptions);
            return;
        }

        Inventory.Instance.Add(opt.item);
        opt.used = true;
        _itemName = opt.item.itemName;
        if (hideAfterPickUp) _hideOnClose = true;

        var lines = (opt.resultLines != null && opt.resultLines.Length > 0)
            ? opt.resultLines
            : new[] { "獲得了「{item}」。" };
        ShowResult(lines, opt.nextOptions);
    }

    // ── 使用道具 ─────────────────────────────────────────────

    private void OpenBagForUse()
    {
        if (optionsPanel) optionsPanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
        _showingResult = false;

        var layer = _currentOptions;   // 記住目前這層，取消或用錯時回來

        if (Inventory.Instance.Items.Count == 0)
        {
            ShowResult(emptyBagLines, layer);
            return;
        }

        InventoryUI.Instance.OpenForSelection(
            "要使用哪個道具？",
            item => OnItemChosen(item, layer),
            () => ShowOptions(layer));      // 按關閉 → 回到選項
    }

    private void OnItemChosen(ItemData item, OptionData[] layer)
    {
        _itemName = item.itemName;

        UseRule rule = null;
        if (useRules != null)
            foreach (var r in useRules)
                if (r.requiredItem == item && !(r.oneTimeOnly && r.done)) { rule = r; break; }

        if (rule == null)
        {
            ShowResult(wrongItemLines, layer);   // 用錯 → 說完後回到同一層選項
            return;
        }

        rule.done = true;
        if (rule.consumeItem) Inventory.Instance.Remove(item);
        rule.onSuccess?.Invoke();
        ShowResult(rule.successLines, rule.nextOptions);
    }

    // ── 結果對話 ─────────────────────────────────────────────

    private void ShowResult(string[] lines, OptionData[] next)
    {
        _pendingNext = next;

        if (lines == null || lines.Length == 0) { AfterResult(); return; }

        _currentLines = lines;
        _resultIndex = 0;
        _showingResult = true;

        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);
        resultText.text = Format(_currentLines[0]);
        if (resultPanel) resultPanel.SetActive(true);
    }

    private void OnResultClicked()
    {
        if (_active != this) return;

        _resultIndex++;

        if (_currentLines != null && _resultIndex < _currentLines.Length)
            resultText.text = Format(_currentLines[_resultIndex]);
        else
            AfterResult();
    }

    private void AfterResult()
    {
        if (_afterResultAction != null)
        {
            var action = _afterResultAction;
            _afterResultAction = null;
            action();
            return;
        }

        if (_pendingNext != null && _pendingNext.Length > 0)
            ShowOptions(_pendingNext);
        else
            CloseAll();
    }

    // ── 工具 ─────────────────────────────────────────────────

    private string Format(string s) => s.Replace("{item}", _itemName);

    private void CloseAll()
    {
        if (_active == this) _active = null;
        _afterResultAction = null;
        _showingResult = false;

        if (_canvas) _canvas.gameObject.SetActive(false);
        Player.CanMove = true;

        if (_hideOnClose) gameObject.SetActive(false);   // 帶走的物件在對話結束後消失
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