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
    public float stretchAmount = 0.1f;        
    public float squashAmount = 0.08f;
    public float maxStretchScale = 1.2f;
    public float minSquashScale = 0.85f;
    private Vector3 originalScale;

    [Header("=== 自转效果（发射后旋转）===")]
    public float minRotateSpeed = 200f;
    public float maxRotateSpeed = 500f;

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

        // 反弹物理材质
        PhysicsMaterial2D bounceMat = new PhysicsMaterial2D();
        bounceMat.bounciness = 0.6f;
        bounceMat.friction = 0.2f;
        jellyCollider.sharedMaterial = bounceMat;

        jellyCollider.isTrigger = false;
        jellyCollider.enabled = true;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic; // 这里已修复

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
                rb.bodyType = RigidbodyType2D.Kinematic; // 这里已修复
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

            float dragMag = dragDir.magnitude;
            float targetScaleX = originalScale.x + dragMag * stretchAmount;
            float targetScaleY = originalScale.y - dragMag * squashAmount;
            
            targetScaleX = Mathf.Clamp(targetScaleX, originalScale.x, originalScale.x * maxStretchScale);
            targetScaleY = Mathf.Clamp(targetScaleY, originalScale.y * minSquashScale, originalScale.y);

            transform.localScale = new Vector3(targetScaleX, targetScaleY, 1);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            transform.localScale = originalScale;

            // 发射
            rb.bodyType = RigidbodyType2D.Dynamic; // 这里已修复
            Vector2 shootDir = startPos - dragPos;
            rb.velocity = shootDir * shootForce;

            // 自转效果
            float rotateSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);
            int rotateDir = Random.value > 0.5f ? 1 : -1;
            rb.angularVelocity = rotateSpeed * rotateDir;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        transform.localScale = originalScale * 0.9f;
        Invoke(nameof(ResetScale), 0.08f);
    }

    private void ResetScale() => transform.localScale = originalScale;

    private bool IsPointOnJelly_Fixed()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);

        Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorld);
        return hitCollider != null && hitCollider.gameObject == gameObject;
    }

    public void ResetJelly()
    {
        rb.bodyType = RigidbodyType2D.Kinematic; // 这里已修复
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