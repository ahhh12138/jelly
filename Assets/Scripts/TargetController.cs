using UnityEngine;

// 靶心碰撞+消失+随机刷新核心脚本，挂载到每个靶心
public class TargetController : MonoBehaviour
{
    [Header("刷新配置")]
    public float refreshTime = 3f;
    [Header("随机位置范围")]
    public float minX = -7f;
    public float maxX = 7f;
    public float minY = -4f;
    public float maxY = 4f;
    [Header("避免重叠")]
    public float safeDistance = 1.5f;

    // 固定大小 0.1（已经写死，不用改）
    private float fixedScale = 0.1f;

    private SpriteRenderer sr;
    private CircleCollider2D col;
    private static GameObject[] allTargets;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<CircleCollider2D>();

        if (allTargets == null)
        {
            allTargets = GameObject.FindGameObjectsWithTag("Target");
        }

        SetRandomPosition();

        // 强制设置大小为 0.1
        transform.localScale = Vector3.one * fixedScale;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("JellyBall"))
        {
            sr.enabled = false;
            col.enabled = false;
            Invoke("RefreshTarget", refreshTime);
        }
    }

    void RefreshTarget()
    {
        SetRandomPosition();

        // 刷新时 强制锁定大小为 0.1
        transform.localScale = Vector3.one * fixedScale;

        sr.enabled = true;
        col.enabled = true;
    }

    void SetRandomPosition()
    {
        Vector3 newPos = Vector3.zero;
        bool positionValid = false;
        int maxAttempts = 10;
        int attempts = 0;
        
        while (!positionValid && attempts < maxAttempts)
        {
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);
            newPos = new Vector3(randomX, randomY, 0);
            
            positionValid = IsPositionSafe(newPos);
            attempts++;
        }
        
        transform.position = newPos;
    }

    bool IsPositionSafe(Vector3 newPos)
    {
        if (allTargets == null) return true;
        
        foreach (GameObject target in allTargets)
        {
            if (target == gameObject) continue;
            
            float distance = Vector3.Distance(newPos, target.transform.position);
            if (distance < safeDistance)
            {
                return false;
            }
        }
        return true;
    }
}