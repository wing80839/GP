using UnityEngine;

public class Door : MonoBehaviour
{


    public Transform playerTrans;

    public GameObject signSprite;

    private Interactable targetItem;

    private bool canPress;
    
    // 傳送位置(用空物件)
    public Transform teleportTarget;


    private void Update()
    {
        signSprite.SetActive(canPress);
        //signSprite.transform.localScale=playerTrans.localScale;

        if (canPress && Input.GetKeyDown(KeyCode.F))
        {

            // 傳送玩家
            playerTrans.position = teleportTarget.position;
            
            if (targetItem != null)
            {
                targetItem.TriggerAction();

                canPress = false;
            }

        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        if(collision.gameObject.CompareTag("Player"))
        {
            canPress = true;
            targetItem = GetComponent<Interactable>();
        }
    }
    private void OnCollisionExit(Collision collision)
    {

        if (collision.gameObject.CompareTag("Player"))
        {
            canPress = false;
        }
    }


}
