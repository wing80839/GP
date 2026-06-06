using UnityEngine;

/// <summary>
/// 管理戰鬥狀態的進入與退出。
/// 進入戰鬥：顯示戰鬥攝影機 + 顯示戰鬥物件 + 隱藏主場景物件
/// 離開戰鬥：回到主攝影機 + 隱藏戰鬥物件 + 顯示主場景物件
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("戰鬥相關物件（進入戰鬥才顯示）")]
    [Tooltip("把 RingJ、RingK、JudgeText、ComboText 全部放在這個父物件下")]
    [SerializeField] private GameObject battleCanvas;

    [Header("主場景物件（戰鬥時隱藏）")]
    [Tooltip("戰鬥時要隱藏的主場景 UI 或物件，可以留空")]
    [SerializeField] private GameObject mainCanvas;

    public GameObject CurrentEnemy { get; private set; }
    public bool IsInBattle { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // 確保初始狀態：戰鬥物件全部隱藏
        SetBattleObjects(false);
    }

    // ── 進入戰鬥 ─────────────────────────────────────────────

    public void EnterBattle(GameObject enemy)
    {
        if (IsInBattle) return;

        CurrentEnemy = enemy;
        IsInBattle = true;

        CameraManager.Instance.ShowBattleCamera(); // 切換攝影機
        SetBattleObjects(true);                    // 顯示戰鬥物件
        SetMainObjects(false);                     // 隱藏主場景物件

        Debug.Log($"[BattleManager] 進入戰鬥！敵人：{enemy.name}");
    }

    // ── 離開戰鬥 ─────────────────────────────────────────────

    public void ExitBattle()
    {
        if (!IsInBattle) return;

        IsInBattle = false;
        CurrentEnemy = null;

        CameraManager.Instance.ShowMainCamera();   // 切回主攝影機
        SetBattleObjects(false);                   // 隱藏戰鬥物件
        SetMainObjects(true);                      // 顯示主場景物件

        Debug.Log("[BattleManager] 戰鬥結束，回到主畫面");
    }

    // ── 輔助方法 ─────────────────────────────────────────────

    private void SetBattleObjects(bool active)
    {
        if (battleCanvas != null)
            battleCanvas.SetActive(active);
    }

    private void SetMainObjects(bool active)
    {
        if (mainCanvas != null)
            mainCanvas.SetActive(active);
    }

    // 測試用：按 Escape 強制結束戰鬥
    private void Update()
    {
        if (IsInBattle && Input.GetKeyDown(KeyCode.Escape))
            ExitBattle();
    }
}