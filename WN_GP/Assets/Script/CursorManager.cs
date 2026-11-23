using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 滑鼠經過「可以點的東西」時換游標。場景裡放一個就好（例如掛在 GameManager 上）。
//
// Detect Mode：
//   Automatic  ─ 自動判斷：有 Interactable 的物件（ObjectInteraction、NPCDialogue、CameraSwitchForSide…）
//                和可以按的 UI 按鈕都會換游標。個別物件可以掛 CursorHover 來排除或換成別的圖案。
//   OnlyMarked ─ 只有掛了 CursorHover 的物件才會換游標，其他一律不換。
public class CursorManager : MonoBehaviour
{
    public enum DetectMode { Automatic, OnlyMarked }

    [Header("判斷方式")]
    [SerializeField] private DetectMode detectMode = DetectMode.Automatic;
    [Tooltip("Automatic 時，經過可以按的 UI 按鈕也換游標")]
    [SerializeField] private bool includeUIButtons = true;

    [Header("平常的游標")]
    [SerializeField] private Texture2D defaultCursor;
    [Tooltip("點擊點在圖片上的位置（左上角是 0,0）")]
    [SerializeField] private Vector2 defaultHotspot = Vector2.zero;

    [Header("經過可以點的東西時")]
    [SerializeField] private Texture2D hoverCursor;
    [SerializeField] private Vector2 hoverHotspot = Vector2.zero;

    private Texture2D _currentTex;
    private Vector2 _currentHot;
    private bool _initialized;
    private readonly List<RaycastResult> _uiHits = new List<RaycastResult>();
    private PointerEventData _pointer;
    private EventSystem _pointerES;   // _pointer 是用哪個 EventSystem 建的

    private void Start() => Set(defaultCursor, defaultHotspot);

    private void Update()
    {
        Texture2D tex = defaultCursor;
        Vector2 hot = defaultHotspot;

        if (FindUI(ref tex, ref hot) || FindWorld(ref tex, ref hot)) { }

        if (!_initialized || tex != _currentTex || hot != _currentHot) Set(tex, hot);
    }

    private void OnDisable()
    {
        Cursor.SetCursor(defaultCursor, defaultHotspot, CursorMode.Auto);
        _initialized = false;
    }

    // ── 判斷 ─────────────────────────────────────────────────

    // 依某個物件決定游標。回傳 true = 這個物件要換游標
    private bool Decide(GameObject go, bool isInteractable, ref Texture2D tex, ref Vector2 hot)
    {
        var mark = go.GetComponentInParent<CursorHover>();
        if (mark != null && mark.isActiveAndEnabled)
        {
            if (!mark.ShowHover) return false;                 // 被標成「不要換」
            tex = mark.CustomCursor != null ? mark.CustomCursor : hoverCursor;
            hot = mark.CustomCursor != null ? mark.CustomHotspot : hoverHotspot;
            return tex != null;
        }

        if (detectMode == DetectMode.OnlyMarked || !isInteractable) return false;
        if (hoverCursor == null) return false;
        tex = hoverCursor;
        hot = hoverHotspot;
        return true;
    }

    // UI：滑鼠在 UI 上時只看最上面那個（UI 會擋住後面的物件）
    private bool FindUI(ref Texture2D tex, ref Vector2 hot)
    {
        var es = EventSystem.current;
        if (es == null || !es.IsPointerOverGameObject()) return false;

        if (_pointer == null || _pointerES != es)
        {
            _pointer = new PointerEventData(es);
            _pointerES = es;
        }
        _pointer.position = Input.mousePosition;
        _uiHits.Clear();
        es.RaycastAll(_pointer, _uiHits);
        if (_uiHits.Count == 0) return false;

        var top = _uiHits[0].gameObject;
        var sel = top.GetComponentInParent<Selectable>();
        bool clickable = includeUIButtons && sel != null && sel.IsInteractable();
        return Decide(top, clickable, ref tex, ref hot);
    }

    // 場景裡的物件
    private bool FindWorld(ref Texture2D tex, ref Vector2 hot)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return false;

        // 對話、劇情進行中，或背包開著 → 場景物件點不到
        if (!Player.CanMove) return false;
        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen) return false;

        Camera cam = GetActiveCamera();
        if (cam == null) return false;

        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        foreach (var col in Physics2D.OverlapPointAll(world))
        {
            if (col == null || !col.enabled) continue;

            var interactable = col.GetComponentInParent<Interactable>();
            bool clickable = interactable is MonoBehaviour mb && mb.isActiveAndEnabled;

            if (Decide(col.gameObject, clickable, ref tex, ref hot)) return true;
        }
        return false;
    }

    // 找目前正在顯示畫面的攝影機（你的場景會切換好幾台）
    private Camera GetActiveCamera()
    {
        Camera best = null;
        foreach (var c in Camera.allCameras)
            if (c.isActiveAndEnabled && (best == null || c.depth > best.depth)) best = c;
        return best != null ? best : Camera.main;
    }

    private void Set(Texture2D tex, Vector2 hot)
    {
        _currentTex = tex;
        _currentHot = hot;
        _initialized = true;
        Cursor.SetCursor(tex, hot, CursorMode.Auto);
    }
}
