using UnityEngine;

public class BattleManager: MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("戰鬥相關物件（進入戰鬥才顯示）")]
    [SerializeField] private GameObject battleCanvas;

    [Header("主場景物件（戰鬥時隱藏）")]
    [SerializeField] private GameObject mainCanvas;

    [Header("音符生成器")]
    [SerializeField] private RhythmSpawner rhythmSpawner;

    public GameObject CurrentEnemy { get; private set; }
    public bool IsInBattle { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        SetBattleObjects(false);
    }

    public void EnterBattle(GameObject enemy)
    {
        if (IsInBattle) return;

        CurrentEnemy = enemy;
        IsInBattle = true;

        CameraManager.Instance.ShowBattleCamera();
        SetBattleObjects(true);
        SetMainObjects(false);
        rhythmSpawner?.StartSpawning();   // 開始生成音符

        Debug.Log($"[BattleController] 進入戰鬥！敵人：{enemy.name}");
    }

    public void ExitBattle()
    {
        if (!IsInBattle) return;

        IsInBattle = false;
        CurrentEnemy = null;

        rhythmSpawner?.StopSpawning();    // 停止生成並清除音符
        CameraManager.Instance.ShowMainCamera();
        SetBattleObjects(false);
        SetMainObjects(true);

        Debug.Log("[BattleController] 戰鬥結束，回到主畫面");
    }

    private void SetBattleObjects(bool active)
    {
        if (battleCanvas != null) battleCanvas.SetActive(active);
    }

    private void SetMainObjects(bool active)
    {
        if (mainCanvas != null) mainCanvas.SetActive(active);
    }

    private void Update()
    {
        if (IsInBattle && Input.GetKeyDown(KeyCode.Escape))
            ExitBattle();
    }
}