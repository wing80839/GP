using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Npc : MonoBehaviour
{
    private bool isPlayerNearby = false;
    
    private void OnCollisionEnter(Collision collision)
    {
        {
            CatchCube(collision);
        }
    }
    private void CatchCube(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        { 
            Debug.Log("玩家進入範圍");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("玩家離開範圍");
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            gameObject.SetActive(false);
            SceneManager.LoadScene("戰鬥");
            return;
        }
    }
}
