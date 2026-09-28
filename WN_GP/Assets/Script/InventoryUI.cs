using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 背包面板。兩種模式：
//  - 選擇模式 OpenForSelection：點道具 → 把道具回傳給呼叫者（用來「使用」）
//  - 瀏覽模式 OpenBrowse：點道具 → 只顯示說明
public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform slotContainer;      // 建議掛 Grid Layout Group
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text descriptionText;     // 可留空

    private Action<ItemData> onPicked;   // null = 瀏覽模式
    private Action onCancel;
    private readonly List<InventorySlotUI> slots = new List<InventorySlotUI>();

    public bool IsOpen => panel.activeSelf;

    void Awake()
    {
        Instance = this;
        Debug.Log($"[背包] InventoryUI 啟動於「{name}」");

        if (panel == null) { Debug.LogError("[背包] InventoryUI 的 Panel 沒有設定，請拖入 Bag"); return; }
        if (panel == gameObject)
            Debug.LogError("[背包] Panel 不能是掛 InventoryUI 的物件自己，請改成它底下的 Bag");

        panel.SetActive(false);
        if (closeButton) closeButton.onClick.AddListener(Cancel);

        // 清掉手動放在 Grid 裡的格子，格子只由程式產生
        if (slotContainer != null && slotContainer.childCount > 0)
        {
            Debug.LogWarning($"[背包] Grid 裡原本有 {slotContainer.childCount} 個物件，已清除（請在編輯器裡刪掉它們）");
            for (int i = slotContainer.childCount - 1; i >= 0; i--)
                Destroy(slotContainer.GetChild(i).gameObject);
        }
    }

    void Start()
    {
        if (Inventory.Instance == null)
        {
            Debug.LogError("[背包] 場景中找不到 Inventory，請在 GameManager 上加 Inventory 元件");
            return;
        }
        Inventory.Instance.OnChanged += Refresh;
    }

    // 背包自己偵測按鍵，不再依賴 Player 的 HandleBag
    [SerializeField] private KeyCode toggleKey = KeyCode.B;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) ToggleBrowse();
    }

    void OnDestroy()
    {
        if (Inventory.Instance != null) Inventory.Instance.OnChanged -= Refresh;
    }

    public void OpenForSelection(string title, Action<ItemData> picked, Action cancelled = null)
    {
        onPicked = picked;
        onCancel = cancelled;
        titleText.text = title;
        if (descriptionText) descriptionText.text = "";
        panel.SetActive(true);
        Refresh();
    }

    // 可以綁在畫面上的「背包」按鈕
    public void OpenBrowse() => OpenForSelection("背包", null);

    // 按 B 用：開著就關（選道具中按 B = 取消），關著就打開瀏覽
    public void ToggleBrowse()
    {
        Debug.Log("[背包] 收到 B 鍵");
        if (IsOpen) { Cancel(); return; }
        if (!Player.CanMove)            // 對話進行中不讓玩家自己開背包
        {
            Debug.LogWarning("[背包] Player.CanMove 是 false（對話中或被鎖住），所以不打開背包");
            return;
        }
        OpenBrowse();
        Debug.Log($"[背包] 已打開，目前有 {Inventory.Instance.Items.Count} 個道具");
    }

    private void Refresh()
    {
        if (!panel.activeSelf) return;

        foreach (var s in slots) Destroy(s.gameObject);
        slots.Clear();

        foreach (var item in Inventory.Instance.Items)
        {
            var slot = Instantiate(slotPrefab, slotContainer);
            slot.Setup(item, OnSlotClicked);
            slots.Add(slot);
        }
    }

    private void OnSlotClicked(ItemData item)
    {
        if (onPicked == null)
        {
            if (descriptionText) descriptionText.text = $"{item.itemName}\n{item.description}";
            return;
        }
        var callback = onPicked;
        Close();
        callback(item);
    }

    private void Cancel()
    {
        var callback = onCancel;
        Close();
        callback?.Invoke();
    }

    private void Close()
    {
        panel.SetActive(false);
        onPicked = null;
        onCancel = null;
    }
}