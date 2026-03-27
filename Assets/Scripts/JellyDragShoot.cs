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

    [Header("=== 音效 🎵 ===")]
    public AudioClip chargeSound;
    public AudioClip collisionSound;
    private AudioSource audioSource;

    private Vector3 originalJellyPos;
    private Collider2D jellyCollider;

    // 🛑 终极防鬼畜专用
    private bool isResting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jellyCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        originalScale = transform.localScale;
        originalJellyPos = transform.position;

        PhysicsMaterial2D bounceMat = new PhysicsMaterial2D();
        bounceMat.bounciness = 0.6f;
        bounceMat.friction = 0.2f;
        jellyCollider.sharedMaterial = bounceMat;

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
                isResting = false; 
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                startPos = new Vector2(mouseWorld.x, mouseWorld.y);
                rb.bodyType = RigidbodyType2D.Kinematic;

                if (audioSource && chargeSound)
                {
                    audioSource.clip = chargeSound;
                    audioSource.loop = true;
                    audioSource.Play();
                }
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
            isResting = false; 
            transform.localScale = originalScale;

            if (audioSource)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }

            rb.bodyType = RigidbodyType2D.Dynamic;
            Vector2 shootDir = startPos - dragPos;
            rb.velocity = shootDir * shootForce;

            float rotateSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);
            int rotateDir = Random.value > 0.5f ? 1 : -1;
            rb.angularVelocity = rotateSpeed * rotateDir;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // ==========================================
        // 🛑 ULTIMATE 终极防鬼畜（绝对不抖）
        // ==========================================
        if (other.gameObject.CompareTag("Boundary"))
        {
            if (rb.velocity.magnitude < 1.0f)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.isKinematic = true; 
                isResting = true;
            }
        }

        // 碰撞音效
        if (audioSource && collisionSound && !isResting)
        {
            audioSource.PlayOneShot(collisionSound);
        }

        // 果冻挤压效果
        transform.localScale = originalScale * 0.9f;
        Invoke(nameof(ResetScale), 0.08f);
    }

    private void ResetScale() => transform.localScale = originalScale;

    private bool IsPointOnJelly_Fixed()
    {
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld);
        return hit != null && hit.gameObject == gameObject;
    }

    public void ResetJelly()
    {
        isResting = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.position = originalJellyPos;
        transform.localScale = originalScale;
        isDragging = false;
    }

    void CheckJellyIdle()
    {
        if (!isDragging && !isResting && rb.velocity.magnitude < 0.2f)
        {
            ResetJelly();
        }
    }
}