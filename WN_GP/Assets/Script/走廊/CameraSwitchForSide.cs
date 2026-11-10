using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// 切換到另一個鏡頭（同一個場景裡）。
// 用法一：掛在有 Collider2D 的物件上（例如「往右上」），玩家點它就切換。
// 用法二：在 UnityEvent（例如門的 On Success）裡選 CameraSwitch → Go()。
// 做法：打開「要切過去的鏡頭」，關掉「目前的鏡頭」。
public class CameraSwitchForSide: MonoBehaviour, Interactable
{
    [Tooltip("要切過去的鏡頭（以及那個區域要出現的東西）")]
    [SerializeField] private GameObject[] turnOn;

    [Tooltip("目前的鏡頭（以及要藏起來的東西）")]
    [SerializeField] private GameObject[] turnOff;

    [Tooltip("等目前的對話結束才切換（從門的 On Success 呼叫時用）")]
    [SerializeField] private bool waitForDialogueEnd = true;

    [Tooltip("滑鼠直接點這個物件的碰撞箱就切換")]
    [SerializeField] private bool clickToSwitch = true;

    [Tooltip("切換完成後要做的事，例如換提示文字、播新的劇情")]
    [SerializeField] private UnityEvent onSwitched;

    private bool _running;

    // ── 觸發方式 ─────────────────────────────────────────────

    // 你原本的點擊系統（呼叫 Interactable.TriggerAction）
    public void TriggerAction()
    {
        if (!clickToSwitch || !CanClick()) return;
        Go();
    }

    // 備用：就算原本的點擊系統沒有呼叫到，直接點碰撞箱也會觸發
    private void OnMouseDown()
    {
        if (!clickToSwitch || !CanClick()) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return; // 點在 UI 上
        Go();
    }

    public void Go()
    {
        if (_running) return;
        _running = true;
        StartCoroutine(Run());
    }

    // ── 內部 ─────────────────────────────────────────────────

    private bool CanClick()
    {
        if (!Player.CanMove) return false;   // 對話、劇情中
        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen) return false;
        return true;
    }

    private IEnumerator Run()
    {
        yield return null;   // 等這一次點擊處理完，避免同一下點到新畫面的東西

        if (waitForDialogueEnd)
            while (!Player.CanMove) yield return null;

        foreach (var go in turnOff) if (go) go.SetActive(false);
        foreach (var go in turnOn) if (go) go.SetActive(true);

        Debug.Log($"[鏡頭] 「{name}」切換鏡頭完成");
        _running = false;
        onSwitched?.Invoke();
    }
}
