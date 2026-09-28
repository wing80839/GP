using System;
using System.Collections.Generic;
using UnityEngine;

// 背包資料（單例）。放在場景中一個「根物件」上，例如 GameManager。
public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [Tooltip("遊戲開始時就在背包裡的道具（測試用），正式遊戲請保持空的")]
    [SerializeField] private List<ItemData> startingItems = new List<ItemData>();
    [Tooltip("同一種道具能不能拿好幾個（解謎道具通常不行）")]
    [SerializeField] private bool allowDuplicates = false;

    private readonly List<ItemData> items = new List<ItemData>();
    public IReadOnlyList<ItemData> Items => items;

    // 背包內容改變時通知 UI 刷新
    public event Action OnChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[背包] 場景裡有多個 Inventory，已移除「{name}」上多餘的那個");
            Destroy(this);   // 只移除多餘的元件，不刪整個物件
            return;
        }
        Instance = this;
        Debug.Log($"[背包] Inventory 啟動於「{name}」");

        // 換場景時保留背包（只有放在最上層的物件才有效）
        if (transform.parent == null) DontDestroyOnLoad(gameObject);
        else Debug.LogWarning($"[背包] 「{name}」不在 Hierarchy 最上層，換場景時背包會清空");

        // 背包一律從空的開始，只放 startingItems
        items.Clear();
        foreach (var it in startingItems) if (it != null) items.Add(it);
        if (items.Count > 0) Debug.LogWarning($"[背包] 開局就有 {items.Count} 個道具（來自 Starting Items）");
    }

    public void Add(ItemData item)
    {
        if (item == null) return;
        if (!allowDuplicates && items.Contains(item))
        {
            Debug.LogWarning($"[背包] 已經有「{item.itemName}」，不重複加入");
            return;
        }
        items.Add(item);
        // 印出是誰加的，方便追查
        Debug.Log($"[背包] 加入「{item.itemName}」\n{Environment.StackTrace}");
        OnChanged?.Invoke();
    }

    public bool Remove(ItemData item)
    {
        bool removed = items.Remove(item);
        if (removed) OnChanged?.Invoke();
        return removed;
    }

    public bool Has(ItemData item) => items.Contains(item);
}