using UnityEngine;

// 掛在個別物件上，指定滑鼠經過它時游標要不要換、換成什麼。
// 場景物件要有 Collider2D；UI 物件要是能被點到的 UI（例如 Button、Image）。
public class CursorHover : MonoBehaviour
{
    [Tooltip("打勾 = 經過時換游標；不打勾 = 經過時不要換（就算它可以點）")]
    [SerializeField] private bool showHover = true;

    [Tooltip("這個物件專用的游標圖案。留空 = 用 CursorManager 的 Hover Cursor")]
    [SerializeField] private Texture2D customCursor;
    [SerializeField] private Vector2 customHotspot = Vector2.zero;

    public bool ShowHover => showHover;
    public Texture2D CustomCursor => customCursor;
    public Vector2 CustomHotspot => customHotspot;

    // 給 UnityEvent 用：例如事件結束後就不再顯示可點擊游標
    public void SetShowHover(bool value) => showHover = value;
}
