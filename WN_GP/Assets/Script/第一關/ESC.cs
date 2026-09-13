using UnityEngine;

public class ESC : MonoBehaviour
{
    [Tooltip("控制的物件")]
    public GameObject target;

    void Start()
    {
        if (target == null)
            target = gameObject;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            target.SetActive(!target.activeSelf);
        }
    }
}