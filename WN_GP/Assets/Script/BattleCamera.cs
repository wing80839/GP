using UnityEngine;

/// <summary>
/// 管理戰鬥狀態的進入與退出。
/// 進入戰鬥時切換到戰鬥攝影機，結束後切回主攝影機。
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public GameObject CurrentEnemy { get; private set; }
    public bool IsInBattle { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>進入戰鬥</summary>
    public void EnterBattle(GameObject enemy)
    {
        if (IsInBattle) return;

        CurrentEnemy = enemy;
        IsInBattle = true;

        // 隱藏主攝影機，顯示戰鬥攝影機
        CameraManager.Instance.ShowBattleCamera();

        Debug.Log($"[BattleManager] 進入戰鬥！敵人：{enemy.name}");

        // TODO: 啟動你的戰鬥系統
        // BattleUI.Instance.Show();
    }

    /// <summary>離開戰鬥</summary>
    public void ExitBattle()
    {
        if (!IsInBattle) return;

        IsInBattle = false;
        CurrentEnemy = null;

        // 隱藏戰鬥攝影機，顯示主攝影機
        CameraManager.Instance.ShowMainCamera();

        Debug.Log("[BattleManager] 戰鬥結束，回到主攝影機");

        // TODO: 關閉戰鬥 UI、結算等
        // BattleUI.Instance.Hide();
    }

    // 測試用：按 Escape 強制結束戰鬥
    private void Update()
    {
        if (IsInBattle && Input.GetKeyDown(KeyCode.Escape))
            ExitBattle();
    }
}