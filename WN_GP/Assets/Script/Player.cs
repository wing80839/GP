using UnityEngine;

public class Player: MonoBehaviour
{
    Vector3 direction;
    [Header("移動速度")]
    public float speed;

   
    void Update()
    {
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
    }
}
