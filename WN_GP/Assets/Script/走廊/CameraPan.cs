using UnityEngine;

// 用螢幕上的左右箭頭，讓攝影機在幾個指定的定點之間移動。
// 掛在要移動的攝影機上（Cinemachine 鏡頭或一般 Camera 都可以）。
// 左右箭頭按鈕的 OnClick 分別接 MoveLeft() / MoveRight()。
public class CameraPan : MonoBehaviour
{
    [Header("定點（由左到右）")]
    [Tooltip("攝影機可以停的位置，數字和 Inspector 上 Transform 的 Position 一樣（相對於父物件）。只看 X、Y")]
    [SerializeField] private Vector2[] points = new Vector2[3];

    [Tooltip("一開始停在第幾個定點（0 = 第一個）")]
    [SerializeField] private int startIndex = 0;

    [Tooltip("一開始就直接放在 Start Index 的位置")]
    [SerializeField] private bool snapOnStart = true;

    [Header("移動")]
    [Tooltip("移動速度（每秒幾個單位）")]
    [SerializeField] private float speed = 10f;

    [Header("箭頭按鈕（可留空）")]
    [Tooltip("在最左邊時會自動隱藏")]
    [SerializeField] private GameObject leftArrow;
    [Tooltip("在最右邊時會自動隱藏")]
    [SerializeField] private GameObject rightArrow;

    private int _index;

    public int CurrentIndex => _index;
    public bool IsMoving => points.Length > 0 && (Vector2)transform.localPosition != points[_index];

    private bool _ready;   // 定點有沒有設定好

    private void Start()
    {
        // 定點全是 (0,0) 代表還沒設定，不要亂移動攝影機
        _ready = false;
        foreach (var p in points) if (p != Vector2.zero) { _ready = true; break; }
        if (!_ready)
        {
            Debug.LogWarning($"[鏡頭] 「{name}」的 Points 還沒設定，攝影機先不移動");
            return;
        }

        _index = Mathf.Clamp(startIndex, 0, Mathf.Max(0, points.Length - 1));
        if (snapOnStart && points.Length > 0) SetPosition(points[_index]);
    }

    private void Update()
    {
        if (points.Length == 0 || !_ready) return;

        // 以固定速度移向目前的定點
        Vector2 now = transform.localPosition;
        Vector2 target = points[_index];
        if (now != target)
            SetPosition(Vector2.MoveTowards(now, target, speed * Time.deltaTime));

        UpdateArrows();
    }

    private void OnDisable()
    {
        // 這個鏡頭沒在用的時候，箭頭也一起藏起來
        if (leftArrow) leftArrow.SetActive(false);
        if (rightArrow) rightArrow.SetActive(false);
    }

    // ── 給按鈕呼叫 ───────────────────────────────────────────

    public void MoveLeft()
    {
        Debug.Log($"[鏡頭] 按了左箭頭（目前在定點 {_index}）");
        if (!CanControl(true)) return;
        if (_index > 0) _index--;
        else Debug.Log("[鏡頭] 已經在最左邊了");
    }

    public void MoveRight()
    {
        Debug.Log($"[鏡頭] 按了右箭頭（目前在定點 {_index}）");
        if (!CanControl(true)) return;
        if (_index < points.Length - 1) _index++;
        else Debug.Log("[鏡頭] 已經在最右邊了");
    }

    // 直接移到第幾個定點（給其他事件用，例如劇情後自動移鏡頭）
    public void MoveTo(int index)
    {
        if (points.Length == 0) return;
        _index = Mathf.Clamp(index, 0, points.Length - 1);
    }

    // ── 內部 ─────────────────────────────────────────────────

    private bool CanControl(bool log = false)
    {
        if (!isActiveAndEnabled)
        {
            if (log) Debug.LogWarning($"[鏡頭] 「{name}」目前是關著的，所以不會移動");
            return false;
        }
        if (points.Length == 0 || !_ready)
        {
            if (log) Debug.LogWarning("[鏡頭] Points 還沒設定，請先設定定點");
            return false;
        }
        // 對話、劇情、背包開著的時候不能移動鏡頭
        if (!Player.CanMove)
        {
            if (log) Debug.LogWarning("[鏡頭] Player.CanMove 是 false（對話或劇情進行中），所以不移動");
            return false;
        }
        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen)
        {
            if (log) Debug.LogWarning("[鏡頭] 背包開著，所以不移動");
            return false;
        }
        return true;
    }

    private void UpdateArrows()
    {
        bool canShow = CanControl();
        if (leftArrow)
        {
            bool show = canShow && _index > 0;
            if (leftArrow.activeSelf != show) leftArrow.SetActive(show);
        }
        if (rightArrow)
        {
            bool show = canShow && _index < points.Length - 1;
            if (rightArrow.activeSelf != show) rightArrow.SetActive(show);
        }
    }

    private void SetPosition(Vector2 p)
    {
        transform.localPosition = new Vector3(p.x, p.y, transform.localPosition.z);
    }

    // ── 編輯器小工具：在 Scene 視窗把攝影機擺好，再用 ⋮ 選單記下位置 ──

    [ContextMenu("把目前位置設為 定點 0")] private void SetPoint0() => SetPointHere(0);
    [ContextMenu("把目前位置設為 定點 1")] private void SetPoint1() => SetPointHere(1);
    [ContextMenu("把目前位置設為 定點 2")] private void SetPoint2() => SetPointHere(2);

    private void SetPointHere(int i)
    {
        if (i >= points.Length) System.Array.Resize(ref points, i + 1);
        points[i] = transform.localPosition;
    }

    private Vector3 ToWorld(Vector2 local)
    {
        var lp = new Vector3(local.x, local.y, transform.localPosition.z);
        return transform.parent ? transform.parent.TransformPoint(lp) : lp;
    }

    // 在 Scene 視窗畫出每個定點的位置
    private void OnDrawGizmosSelected()
    {
        if (points == null) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 p = ToWorld(points[i]);
            Gizmos.DrawWireSphere(p, 0.5f);
            if (i > 0) Gizmos.DrawLine(ToWorld(points[i - 1]), p);
        }
    }
}
