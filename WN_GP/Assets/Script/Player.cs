using UnityEngine;

public class Player : MonoBehaviour
{
    // ── 背包（原封不動）──────────────────────
    public GameObject myBag;
    bool isOpen;

    // ── 移動設定 ─────────────────────────────
    [Header("移動速度")]
    public float speed;

    // ── 動畫 ─────────────────────────────────
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

        Vector2 moveNormalized = _moveDir.magnitude > 1f ? _moveDir.normalized : _moveDir;

        Vector3 direction = new Vector3(moveNormalized.x, 0f, moveNormalized.y);
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

    // ── 背包（原封不動）──────────────────────
    void HandleBag()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isOpen = !isOpen;
            myBag.SetActive(isOpen);
        }
    }
}