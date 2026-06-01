using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    public GameObject mainSceneRoot;   // 拖入 MainSceneRoot
    public GameObject battleSceneRoot; // 拖入 BattleSceneRoot

    void Start()
    {
        // 確保遊戲一開始，主場景顯示，戰鬥場景隱藏
        mainSceneRoot.SetActive(true);
        battleSceneRoot.SetActive(false);
    }

    // 當與 NPC 對話完，呼叫這個方法進入戰鬥
    public void GoToBattle()
    {
        mainSceneRoot.SetActive(false);   // 隱藏主場景
        battleSceneRoot.SetActive(true);  // 顯示戰鬥場景

        // (選填) 如果戰鬥場景有獨立的相機，記得啟用它；並關閉主場景相機
    }

    // 戰鬥結束，回到主場景
    public void ReturnToMain()
    {
        battleSceneRoot.SetActive(false); // 隱藏戰鬥場景
        mainSceneRoot.SetActive(true);    // 顯示主場景
    }
}