using UnityEngine;

public class Sign : MonoBehaviour
{
    public Transform playerTrans;

    public GameObject signSprite;

    private Interactable targetItem;

    private bool canPress;


    private void Update()
    {
        signSprite.SetActive(canPress);
        signSprite.transform.localScale=playerTrans.localScale;

        if (canPress && Input.GetKeyDown(KeyCode.X))
        {
            targetItem.TriggerAction();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        if(collision.gameObject.CompareTag("Interactable"))
        {
            canPress = true;
            targetItem = collision.gameObject.GetComponent<Interactable>();
        }
    }
    private void OnCollisionExit(Collision collision)
    {

        if (collision.gameObject.CompareTag("Interactable"))
        {
            canPress = false;
        }
    }


}
