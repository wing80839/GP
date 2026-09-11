using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class Sign : MonoBehaviour
{
    [Header("游標懸停提示（可選）")]
    public GameObject signSprite;

    private Interactable targetItem;

    private void Awake()
    {
        targetItem = GetComponent<Interactable>();

        if (signSprite != null)
        {
            signSprite.SetActive(false);
        }
    }

    // 滑鼠點擊物件
    private void OnMouseDown()
    {
        // 防止穿透：如果點擊到了 UI（例如對話框、選單按鈕），不觸發地圖物件
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (targetItem != null)
        {
            targetItem.TriggerAction();
        }
    }

    // 滑鼠懸停：顯示提示圖示（若不需要可整段刪除）
    private void OnMouseEnter()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (signSprite != null)
        {
            signSprite.SetActive(true);
        }
    }

    // 滑鼠移開：隱藏提示圖示（若不需要可整段刪除）
    private void OnMouseExit()
    {
        if (signSprite != null)
        {
            signSprite.SetActive(false);
        }
    }
}
