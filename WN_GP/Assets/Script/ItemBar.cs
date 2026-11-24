using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 物品欄（簡單版）：點「物品欄_1」打開選單，選單裡列出拿到的道具。
// 對物件選「使用」時，選單會自動打開，點道具就使用。
// 按鈕的 On Click 不用自己設定，程式會接好。
public class ItemBar : MonoBehaviour
{
    public static ItemBar Instance { get; private set; }

    [Header("按鈕")]
    [Tooltip("打開選單的按鈕（物品欄_1）")]
    [SerializeField] private Button openButton;
    [Tooltip("關閉按鈕（選單_群組 底下的 Button）")]
    [SerializeField] private Button closeButton;
    [Tooltip("灰色遮罩（要有 Button 元件，點了也會關閉。可留空）")]
    [SerializeField] private Button maskButton;

    [Header("選單")]
    [Tooltip("要開關的選單（選單）")]
    [SerializeField] private GameObject menu;
    [Tooltip("道具格子要排在哪裡（選單裡放一個空物件，加 Grid Layout Group）")]
    [SerializeField] private Transform slotContainer;
    [Tooltip("道具格子 Prefab（掛 InventorySlotUI）")]
    [SerializeField] private InventorySlotUI slotPrefab;
    [Tooltip("點道具時顯示說明（可留空）")]
    [SerializeField] private TMP_Text descriptionText;

    private Action<ItemData> _onPicked;   // 有值 = 正在選道具來使用
    private Action _onCancel;
    private readonly List<GameObject> _slots = new List<GameObject>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic() => Instance = null;

    private void Awake()
    {
        Instance = this;
        if (openButton) openButton.onClick.AddListener(Open);
        if (closeButton) closeButton.onClick.AddListener(Close);
        if (maskButton) maskButton.onClick.AddListener(Close);
        if (menu) menu.SetActive(false);
    }

    private void Start()
    {
        if (Inventory.Instance != null) Inventory.Instance.OnChanged += Refresh;
        else Debug.LogError("[物品欄] 場景裡找不到 Inventory（GameManager 上要掛 Inventory）");
    }

    private void OnDestroy()
    {
        if (Inventory.Instance != null) Inventory.Instance.OnChanged -= Refresh;
    }

    // ── 開關 ─────────────────────────────────────────────────

    public void Open()
    {
        if (_onPicked == null && !Player.CanMove) return;   // 對話中不能自己打開
        if (descriptionText) descriptionText.text = "";
        if (menu) menu.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        if (menu) menu.SetActive(false);

        // 選道具時關掉 = 取消
        if (_onPicked != null)
        {
            var cb = _onCancel;
            _onPicked = null;
            _onCancel = null;
            cb?.Invoke();
        }
    }

    public bool IsOpen => menu != null && menu.activeSelf;

    // ── 對物件「使用」時（ObjectInteraction 會呼叫） ──────────

    public void OpenForSelection(string prompt, Action<ItemData> picked, Action cancelled = null)
    {
        _onPicked = picked;
        _onCancel = cancelled;
        if (descriptionText) descriptionText.text = prompt;
        if (menu) menu.SetActive(true);
        Refresh();
    }

    // ── 道具格子 ─────────────────────────────────────────────

    private void Refresh()
    {
        if (!IsOpen || slotContainer == null || slotPrefab == null || Inventory.Instance == null) return;

        foreach (var s in _slots) if (s) Destroy(s);
        _slots.Clear();

        foreach (var item in Inventory.Instance.Items)
        {
            var slot = Instantiate(slotPrefab, slotContainer);
            slot.Setup(item, OnSlotClicked);
            _slots.Add(slot.gameObject);
        }
    }

    private void OnSlotClicked(ItemData item)
    {
        if (_onPicked != null)   // 選道具來使用：點了就用
        {
            var cb = _onPicked;
            _onPicked = null;
            _onCancel = null;
            if (menu) menu.SetActive(false);
            cb(item);
            return;
        }

        // 平常：顯示說明
        if (descriptionText)
            descriptionText.text = string.IsNullOrEmpty(item.description)
                ? item.itemName
                : $"{item.itemName}：{item.description}";
    }
}