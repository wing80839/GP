using UnityEngine;

public class Player: MonoBehaviour
{
    public GameObject myBag;
    bool isOpen;
    Vector3 direction;
    [Header("移動速度")]
    public float speed;

   
    void Update()
    {
        //角色移動
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Debug.Log("W");
            direction = Vector3.forward * (speed * Time.deltaTime);
            transform.Translate(direction);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            Debug.Log("D");
            direction = Vector3.right * (speed * Time.deltaTime);
            transform.Translate(direction);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            Debug.Log("S");
            direction = Vector3.back * (speed * Time.deltaTime);
            transform.Translate(direction);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Debug.Log("A");
            direction = Vector3.left * (speed * Time.deltaTime);
            transform.Translate(direction);
        }

        //開啟背包
        if (Input.GetKeyDown(KeyCode.B))
        {
            isOpen = !isOpen;
            myBag.SetActive(isOpen);
        }
    }
}
