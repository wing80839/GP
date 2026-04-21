using System;
using UnityEngine;

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
}
