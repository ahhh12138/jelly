using UnityEngine;

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

    private float fixedScale = 0.1f;
    private SpriteRenderer sr;
    private CircleCollider2D col;
    private static GameObject[] allTargets;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<CircleCollider2D>();

        PhysicsMaterial2D bounceMaterial = new PhysicsMaterial2D();
        bounceMaterial.bounciness = 0.6f;
        bounceMaterial.friction = 0.2f;
        col.sharedMaterial = bounceMaterial;

        // 每次启动都重新找所有靶子（解决黄绿靶子不生效问题）
        allTargets = GameObject.FindGameObjectsWithTag("Target");

        SetRandomPosition();
        ForceScale();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("JellyBall"))
        {
            Invoke(nameof(HideTarget), 0.1f);
        }
    }

    void HideTarget()
    {
        sr.enabled = false;
        col.enabled = false;
        Invoke(nameof(RefreshTarget), refreshTime);
    }

    void RefreshTarget()
    {
        SetRandomPosition();
        ForceScale(); // 强制大小0.1
        sr.enabled = true;
        col.enabled = true;
    }

    void ForceScale()
    {
        transform.localScale = new Vector3(fixedScale, fixedScale, 1f);
    }

    void SetRandomPosition()
    {
        Vector3 newPos = Vector3.zero;
        bool positionValid = false;
        int attempts = 0;

        while (!positionValid && attempts < 10)
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
            if (Vector3.Distance(newPos, target.transform.position) < safeDistance)
                return false;
        }
        return true;
    }
}