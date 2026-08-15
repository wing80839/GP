using UnityEngine;
 
public class Player : MonoBehaviour
{
    public GameObject myBag;
    bool isOpen;
 
    [Header("移動速度")]
    public float speed;
    // 不能斜著走
    public bool restrictToFourDirections = true;
 
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
 
    // ── 移動 + 動畫 ───────────────────────────
    void HandleMovement()
    {
        _moveDir.x = Input.GetAxisRaw("Horizontal");
        _moveDir.y = Input.GetAxisRaw("Vertical");
 
        // 若限制四方向移動：只保留數值較大的那一軸，避免斜著走
        Vector2 moveForMotion = _moveDir;
        if (restrictToFourDirections && _moveDir.magnitude >= 0.1f)
        {
            if (Mathf.Abs(_moveDir.y) >= Mathf.Abs(_moveDir.x))
                moveForMotion = new Vector2(0f, _moveDir.y);
            else
                moveForMotion = new Vector2(_moveDir.x, 0f);
        }
 
        Vector2 moveNormalized = moveForMotion.magnitude > 1f ? moveForMotion.normalized : moveForMotion;
 
        // 2D：移動平面是 X-Y，不是 X-Z
        Vector3 direction = new Vector3(moveNormalized.x, moveNormalized.y, 0f);
        transform.Translate(direction * speed * Time.deltaTime);
 
        // Blend Tree 只需要 DirX / DirY，移除 Speed
        // 斜向時取較大的軸，避免動畫混合抖動
        if (_moveDir.magnitude < 0.1f)
        {
            // 靜止：全部歸零 → 自動播 Idle
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
 
    //背包
    void HandleBag()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isOpen = !isOpen;
            myBag.SetActive(isOpen);
        }
    }
}