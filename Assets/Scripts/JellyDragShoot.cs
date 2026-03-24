using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))] 
public class JellyDragShoot : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 startPos;
    private Vector2 dragPos;
    private bool isDragging = false;

    [Header("=== 弹射力度 ===")]
    public float shootForce = 14f;
    public float maxDragDistance = 3.5f;

    [Header("=== 果冻变形手感（Q弹核心）===")]
    // 核心优化：大幅降低变形系数，让变形更克制
    public float stretchAmount = 0.1f;        // 从0.35→0.1，横向拉伸幅度缩小70%
    public float squashAmount = 0.08f;        // 从0.2→0.08，纵向压扁幅度缩小60%
    public float maxStretchScale = 1.2f;      // 新增：横向最大缩放（不超过原始的1.2倍）
    public float minSquashScale = 0.85f;      // 新增：纵向最小缩放（不低于原始的0.85倍）
    private Vector3 originalScale;

    [Header("=== 边界限制 ===")]
    public float minDragY = -4.5f;

    private Vector3 originalJellyPos;
    private Collider2D jellyCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jellyCollider = GetComponent<Collider2D>();
        originalScale = transform.localScale;
        originalJellyPos = transform.position;

        jellyCollider.isTrigger = false;
        jellyCollider.enabled = true;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        InvokeRepeating("CheckJellyIdle", 6f, 6f);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointOnJelly_Fixed())
            {
                isDragging = true;
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                startPos = new Vector2(mouseWorld.x, mouseWorld.y);
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dragPos = new Vector2(mouseWorld.x, mouseWorld.y);
            
            Vector2 dragDir = dragPos - startPos;

            if (dragDir.magnitude > maxDragDistance)
            {
                dragDir = dragDir.normalized * maxDragDistance;
                dragPos = startPos + dragDir;
            }

            if (dragPos.y < minDragY) dragPos.y = minDragY;

            transform.position = new Vector3(dragPos.x, dragPos.y, transform.position.z);

            // ========== 核心优化：限制变形范围，避免过度变形 ==========
            float dragMag = dragDir.magnitude;
            // 计算目标缩放，但限制最大/最小值
            float targetScaleX = originalScale.x + dragMag * stretchAmount;
            float targetScaleY = originalScale.y - dragMag * squashAmount;
            
            // 强制限制缩放范围，确保变形不夸张
            targetScaleX = Mathf.Clamp(targetScaleX, originalScale.x, originalScale.x * maxStretchScale);
            targetScaleY = Mathf.Clamp(targetScaleY, originalScale.y * minSquashScale, originalScale.y);

            transform.localScale = new Vector3(targetScaleX, targetScaleY, 1);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;

            // 优化：松手后平滑恢复形状（不是瞬间变圆，更像果冻）
            Invoke(nameof(ResetScaleSmooth), 0.01f);

            rb.bodyType = RigidbodyType2D.Dynamic;
            Vector2 shootDir = startPos - dragPos;
            rb.velocity = shootDir * shootForce;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // 优化：碰撞变形也更克制（从0.85→0.9）
        transform.localScale = originalScale * 0.9f;
        Invoke(nameof(ResetScale), 0.08f);
    }

    // 新增：平滑恢复形状（替代瞬间恢复）
    private void ResetScaleSmooth()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, originalScale, 0.5f);
        // 确保最终恢复到原始形状
        if (Vector3.Distance(transform.localScale, originalScale) > 0.01f)
        {
            Invoke(nameof(ResetScaleSmooth), 0.01f);
        }
        else
        {
            transform.localScale = originalScale;
        }
    }

    private void ResetScale() => transform.localScale = originalScale;

    private bool IsPointOnJelly_Fixed()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);

        Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorld);
        return hitCollider != null && hitCollider.gameObject == this.gameObject;
    }

    public void ResetJelly()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.position = originalJellyPos;
        transform.localScale = originalScale;
        isDragging = false;
    }

    void CheckJellyIdle()
    {
        if (rb.velocity.magnitude < 0.1f && Vector2.Distance(transform.position, originalJellyPos) > 0.2f)
            ResetJelly();
    }
}