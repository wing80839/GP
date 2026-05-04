using UnityEngine;

public class Bed : MonoBehaviour, Interactable
{
    public GameObject dialogueUI;

    private void Start()
    {
       
        if (dialogueUI != null) dialogueUI.SetActive(false);
    
    }

    public void TriggerAction()
    {
        Debug.Log("options");

        if (dialogueUI != null)
        {
            
            dialogueUI.SetActive(true);

        }
    }


}
