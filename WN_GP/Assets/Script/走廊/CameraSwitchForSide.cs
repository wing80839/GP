using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// 切換到另一個鏡頭（同一個場景裡）。
// 用法一：掛在有 Collider2D 的物件上（例如「往右上」），玩家點它就切換。
// 用法二：在 UnityEvent（例如門的 On Success）裡選 CameraSwitch → Go()。
// 做法：打開「要切過去的鏡頭」，關掉「目前的鏡頭」。
public class CameraSwitchForSide : MonoBehaviour, Interactable
{
    [Tooltip("要切過去的鏡頭（以及那個區域要出現的東西）")]
    [SerializeField] private GameObject[] turnOn;

    [Tooltip("目前的鏡頭（以及要藏起來的東西）")]
    [SerializeField] private GameObject[] turnOff;

    [Tooltip("切過去後，新攝影機要出現在它的第幾個定點（0 = 定點 0）。-1 = 不指定，維持上次離開時的位置")]
    [SerializeField] private int entryPoint = -1;

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
        Debug.Log($"[鏡頭] 點到「{name}」（由點擊系統觸發）");
        if (!clickToSwitch || !CanClick()) return;
        Go();
    }

    // 備用：就算原本的點擊系統沒有呼叫到，直接點碰撞箱也會觸發
    private void OnMouseDown()
    {
        Debug.Log($"[鏡頭] 點到「{name}」的碰撞箱");
        if (!clickToSwitch || !CanClick()) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.LogWarning($"[鏡頭] 點擊被 UI 擋住了：滑鼠下面有 UI 物件");
            return;
        }
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
        if (!Player.CanMove)
        {
            Debug.LogWarning("[鏡頭] Player.CanMove 是 false（對話或劇情中），所以不切換");
            return false;
        }
        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen)
        {
            Debug.LogWarning("[鏡頭] 背包開著，所以不切換");
            return false;
        }
        return true;
    }

    private IEnumerator Run()
    {
        yield return null;   // 等這一次點擊處理完，避免同一下點到新畫面的東西

        if (waitForDialogueEnd)
            while (!Player.CanMove) yield return null;

        foreach (var go in turnOff) if (go) go.SetActive(false);
        foreach (var go in turnOn)
        {
            if (!go) continue;
            go.SetActive(true);

            // 決定新攝影機從哪個定點出現
            if (entryPoint >= 0)
            {
                var pan = go.GetComponent<CameraPan>();
                if (pan) pan.JumpTo(entryPoint);
            }
        }

        Debug.Log($"[鏡頭] 「{name}」切換鏡頭完成");
        _running = false;
        onSwitched?.Invoke();
    }
}