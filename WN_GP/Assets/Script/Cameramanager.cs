using UnityEngine;

/// <summary>
/// 管理主攝影機與戰鬥攝影機的顯示/隱藏切換。
/// 戰鬥攝影機在 Inspector 預設設為 inactive（隱藏）。
/// </summary>
public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("攝影機物件")]
    [Tooltip("主要攝影機的 GameObject（預設啟用）")]
    [SerializeField] private GameObject mainCameraObject;

    [Tooltip("戰鬥攝影機的 GameObject（預設在 Inspector 設為隱藏）")]
    [SerializeField] private GameObject battleCameraObject;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 確保初始狀態正確：主攝影機顯示、戰鬥攝影機隱藏
        ShowMainCamera();
    }

    /// <summary>切換到主攝影機（戰鬥結束時呼叫）</summary>
    public void ShowMainCamera()
    {
        mainCameraObject.SetActive(true);
        battleCameraObject.SetActive(false);
        Debug.Log("[CameraManager] 顯示主攝影機");
    }

    /// <summary>切換到戰鬥攝影機（進入戰鬥時呼叫）</summary>
    public void ShowBattleCamera()
    {
        battleCameraObject.SetActive(true);
        mainCameraObject.SetActive(false);
        Debug.Log("[CameraManager] 顯示戰鬥攝影機");
    }
}