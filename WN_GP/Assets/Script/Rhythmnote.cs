using UnityEngine;

/// <summary>
/// 掛在每個音符 GameObject 上。
/// 從右往左移動，超過判定線還沒被打就算 Miss。
/// </summary>
public class RhythmNote : MonoBehaviour
{
    public enum Lane { J, K }

    [HideInInspector] public Lane lane;

    private float moveSpeed;
    private float missX;       // 超過這個 X 就 Miss
    private bool isJudged;    // 已被判定過就不再觸發

    public void Init(Lane noteLane, float speed, float missPositionX)
    {
        lane = noteLane;
        moveSpeed = speed;
        missX = missPositionX;
    }

    private void Update()
    {
        if (isJudged) return;

        // 往左移動
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        // 超過判定線還沒被打 → Miss
        if (transform.position.x < missX)
        {
            isJudged = true;
            RhythmJudge.Instance.OnNoteMiss();
            Destroy(gameObject);
        }
    }

    /// <summary>被 RhythmJudge 呼叫，標記為已判定</summary>
    public void SetJudged()
    {
        isJudged = true;
    }
}