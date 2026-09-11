using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject myBag;
    bool isOpen;

    [Header("移動速度")]
    public float speed;
    public bool restrictToFourDirections = true;

    // 外部可控制是否停止移動（選項介面開啟時用）
    public static bool CanMove = true;

    private Animator _animator;
    private Vector2 _moveDir;

    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        HandleMovement();
        HandleBag();
    }

    void HandleMovement()
    {
        // 不能移動時，動畫歸零並直接返回
        if (!CanMove)
        {
            _animator.SetFloat("DirX", 0f);
            _animator.SetFloat("DirY", 0f);
            return;
        }

        _moveDir.x = Input.GetAxisRaw("Horizontal");
        _moveDir.y = Input.GetAxisRaw("Vertical");

        Vector2 moveForMotion = _moveDir;
        if (restrictToFourDirections && _moveDir.magnitude >= 0.1f)
        {
            if (Mathf.Abs(_moveDir.y) >= Mathf.Abs(_moveDir.x))
                moveForMotion = new Vector2(0f, _moveDir.y);
            else
                moveForMotion = new Vector2(_moveDir.x, 0f);
        }

        Vector2 moveNormalized = moveForMotion.magnitude > 1f ? moveForMotion.normalized : moveForMotion;
        Vector3 direction = new Vector3(moveNormalized.x, moveNormalized.y, 0f);
        transform.Translate(direction * speed * Time.deltaTime);

        if (_moveDir.magnitude < 0.1f)
        {
            _animator.SetFloat("DirX", 0f);
            _animator.SetFloat("DirY", 0f);
        }
        else if (Mathf.Abs(_moveDir.y) >= Mathf.Abs(_moveDir.x))
        {
            _animator.SetFloat("DirX", 0f);
            _animator.SetFloat("DirY", _moveDir.y);
        }
        else
        {
            _animator.SetFloat("DirX", _moveDir.x);
            _animator.SetFloat("DirY", 0f);
        }
    }

    void HandleBag()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isOpen = !isOpen;
            myBag.SetActive(isOpen);
        }
    }
}