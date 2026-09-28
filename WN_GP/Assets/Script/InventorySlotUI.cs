using System;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 背包裡的一格（做成 Prefab）
public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Button button;

    [Header("外觀")]
    [Tooltip("隱藏格子的白色底，只顯示道具圖片")]
    [SerializeField] private bool transparentBackground = true;
    [Tooltip("有圖片時就不顯示文字；沒圖片的道具仍會顯示名稱")]
    [SerializeField] private bool hideLabelWhenHasIcon = true;
    [Tooltip("圖片離格子邊緣的距離")]
    [SerializeField] private float iconPadding = 6f;

    public void Setup(ItemData item, Action<ItemData> onClick)
    {
        bool hasIcon = item.icon != null;

        // 圖片：填滿格子（留一點邊），保持比例
        icon.sprite = item.icon;
        icon.enabled = hasIcon;
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        var rt = icon.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(iconPadding, iconPadding);
        rt.offsetMax = new Vector2(-iconPadding, -iconPadding);

        // 文字
        label.text = item.itemName;
        label.gameObject.SetActive(!(hideLabelWhenHasIcon && hasIcon));

        // 白底變透明（仍然可以點）
        if (transparentBackground && button.targetGraphic != null && button.targetGraphic != icon)
        {
            var c = button.targetGraphic.color;
            c.a = 0f;
            button.targetGraphic.color = c;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(item));
    }
}