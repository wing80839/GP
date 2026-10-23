using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// 切換到另一個鏡頭（同一個場景裡）。在 UnityEvent（例如門的 On Success）裡選 CameraSwitch → Go()。
// 做法：打開「要切過去的鏡頭」，關掉「目前的鏡頭」。Cinemachine 鏡頭或一般 Camera 都可以。
public class CameraSwitch : MonoBehaviour
{
    [Tooltip("要切過去的鏡頭（以及那個區域要出現的東西）")]
    [SerializeField] private GameObject[] turnOn;

    [Tooltip("目前的鏡頭（以及要藏起來的東西）")]
    [SerializeField] private GameObject[] turnOff;

    [Tooltip("等目前的對話結束（玩家看完「門打開了。」）才切換")]
    [SerializeField] private bool waitForDialogueEnd = true;

    [Tooltip("切換完成後要做的事，例如換提示文字、播新的劇情")]
    [SerializeField] private UnityEvent onSwitched;

    private bool _running;

    public void Go()
    {
        if (_running) return;
        _running = true;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        yield return null;   // 讓這一幀的對話先顯示出來

        if (waitForDialogueEnd)
            while (!Player.CanMove) yield return null;

        foreach (var go in turnOff) if (go) go.SetActive(false);
        foreach (var go in turnOn) if (go) go.SetActive(true);

        _running = false;
        onSwitched?.Invoke();
    }
}