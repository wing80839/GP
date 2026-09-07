using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Npc : MonoBehaviour, Interactable
{
    
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        {
            CatchCube(collision);
        }
    }
    private void CatchCube(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        { 
            Debug.Log("玩家進入範圍");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("玩家離開範圍");
        }
    }
    

    public void TriggerAction()
    {
        // 切換到戰鬥攝影機
        CameraManager.Instance.ShowBattleCamera();

        // 通知 BattleManager 進入戰鬥（傳入此 NPC）
        BattleManager.Instance.EnterBattle(gameObject);

        

        //gameObject.SetActive(false);
        //SceneManager.LoadScene("戰鬥");
        return;
    }
}
