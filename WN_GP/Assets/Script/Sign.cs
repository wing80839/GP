using UnityEngine;

public class Sign : MonoBehaviour
{


    public Transform playerTrans;

    public GameObject signSprite;

    private Interactable targetItem;


    private bool canPress;
    


    private void Update()
    {
        if (signSprite != null)
        {
            signSprite.SetActive(canPress);
        }

        if (canPress && Input.GetKeyDown(KeyCode.F))
        {
            

            if (targetItem != null)
            {
                targetItem.TriggerAction();

                canPress = false;
            }

        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if(collision.gameObject.CompareTag("Player"))
        {
            canPress = true;
            targetItem = GetComponent<Interactable>();
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Player"))
        {
            canPress = false;
        }
    }


}
