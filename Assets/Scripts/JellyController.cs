using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class JellyController : MonoBehaviour
{
    #region 核心参数（可在Inspector面板调整）
    [Header("果冻基础设置")]
    [Tooltip("果冻初始大小")]
    public Vector2 originalSize = new Vector2(1f, 1f);
    [Tooltip("果冻默认半径（适配碰撞检测）")]
    public float jellyRadius = 0.5f;

    [Header("拖拽形变参数")]
    [Tooltip("最大拖拽距离（超过后不再形变）")]
    public float maxDragDistance = 3f;
    [Tooltip("拉伸系数（越大越易拉长）")]
    public float stretchFactor = 100f;
    [Tooltip("压扁系数（越大越易压扁）")]
    public float squashFactor = 170f;
    [Tooltip("形变缓动系数（越小越粘滞）")]
    [Range(0.05f, 0.5f)] public float deformLerpSpeed = 0.22f;

    [Header("物理参数（核心软糯感）")]
    [Tooltip("极低重力（模拟轻飘飘的感觉）")]
    [Range(0.01f, 0.5f)] public float gravityScale = 0.08f;
    [Tooltip("移动阻力（越大越粘滞）")]
    [Range(1f, 10f)] public float linearDrag = 5f;
    [Tooltip("旋转阻力（防止过度旋转）")]
    [Range(1f, 10f)] public float angularDrag = 6f;
    [Tooltip("碰撞回弹系数（几乎不反弹）")]
    [Range(0f, 0.3f)] public float bounciness = 0.1f;
    [Tooltip("碰撞速度衰减（越大越瘫软）")]
    [Range(0.3f, 0.8f)] public float collisionSpeedDamp = 0.6f;

    [Header("飞行晃动参数")]
    [Tooltip("晃动增量（越大越晃）")]
    public float wobbleSpeed = 0.22f;
    [Tooltip("速度拉伸系数")]
    public float speedStretchFactor = 72f;
    [Tooltip("速度压扁系数")]
    public float speedSquashFactor = 110f;
    [Tooltip("飞行形变缓动系数")]
    [Range(0.05f, 0.5f)] public float flightLerpSpeed = 0.18f;

    [Header("发射参数")]
    [Tooltip("发射力度系数")]
    public float launchPower = 0.162f;
    [Tooltip("发射初始形变（更夸张的拉伸）")]
    public float launchStretch = 1.75f;
    public float launchSquash = 0.58f;
    #endregion

    #region 私有变量
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector3 basePosition; // 果冻初始位置（弹弓位置）
    private Vector3 currentScaleVelocity; // 形变缓动速度
    private bool isDragging = false;
    private bool isLaunched = false;
    private float wobble; // 飞行晃动值
    #endregion

    void Awake()
    {
        // 获取组件
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 初始化物理参数（核心软糯感）
        rb.gravityScale = gravityScale;
        rb.drag = linearDrag;
        rb.angularDrag = angularDrag;
        rb.freezeRotation = false; // 允许旋转，模拟倾斜

        // 设置碰撞材质（低回弹）
        PhysicsMaterial2D material = new PhysicsMaterial2D();
        material.bounciness = bounciness;
        material.friction = 0.5f;
        rb.sharedMaterial = material;

        // 记录初始位置
        basePosition = transform.position;
        // 初始化大小
        transform.localScale = originalSize;
    }

    void Update()
    {
        // 拖拽逻辑（仅在未发射时生效）
        if (!isLaunched)
        {
            HandleDrag();
        }
        // 飞行形变逻辑
        else
        {
            HandleFlightDeformation();
        }

        // 边界检测（防止飞出屏幕）
        CheckBounds();
    }

    #region 拖拽逻辑（核心形变）
    private void HandleDrag()
    {
        // 鼠标/触摸输入
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(mousePos, transform.position) < jellyRadius + 0.2f)
            {
                isDragging = true;
                rb.isKinematic = true; // 拖拽时关闭物理
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = mousePos - (Vector2)basePosition;
            float distance = direction.magnitude;

            // 限制最大拖拽距离
            if (distance > maxDragDistance)
            {
                transform.position = basePosition + (Vector3)(direction.normalized * maxDragDistance);
            }
            else
            {
                transform.position = mousePos;
            }

            // 计算拖拽形变（核心：拉伸+压扁+倾斜）
            float dragDistance = Vector2.Distance(transform.position, basePosition);
            float stretch = Mathf.Lerp(transform.localScale.y, 1 + dragDistance / stretchFactor, deformLerpSpeed);
            float squash = Mathf.Lerp(transform.localScale.x, 1 - dragDistance / squashFactor, deformLerpSpeed);
            transform.localScale = new Vector3(squash, stretch, 1f);

            // 跟随拖拽方向倾斜
            float angle = Mathf.Atan2(direction.y, direction.x) + Mathf.PI / 2;
            transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            rb.isKinematic = false; // 恢复物理

            // 计算发射方向和力度
            Vector2 launchDir = (basePosition - transform.position);
            float launchDistance = launchDir.magnitude;

            // 只有拖拽距离足够才发射
            if (launchDistance > 0.14f)
            {
                LaunchJelly(launchDir);
            }
            else
            {
                // 重置位置和形变
                ResetJelly();
            }
        }
    }
    #endregion

    #region 发射逻辑
    private void LaunchJelly(Vector2 launchDir)
    {
        isLaunched = true;

        // 发射初始形变（更夸张的拉伸）
        transform.localScale = new Vector3(launchSquash, launchStretch, 1f);

        // 应用发射力度
        rb.velocity = launchDir * launchPower;
    }
    #endregion

    #region 飞行形变（晃动+速度关联形变）
    private void HandleFlightDeformation()
    {
        // 持续晃动
        wobble += wobbleSpeed * Time.deltaTime;

        // 速度关联形变
        float speed = rb.velocity.magnitude;
        float stretch = Mathf.Lerp(transform.localScale.y, 1 + speed / speedStretchFactor, flightLerpSpeed);
        float squash = Mathf.Lerp(transform.localScale.x, 1 - speed / speedSquashFactor, flightLerpSpeed);
        transform.localScale = new Vector3(squash, stretch, 1f);

        // 跟随运动方向倾斜
        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) - Mathf.PI / 2;
        transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

        // 形变缓慢恢复（阻尼感）
        transform.localScale = Vector3.SmoothDamp(
            transform.localScale, 
            originalSize, 
            ref currentScaleVelocity, 
            0.4f // 恢复时间（越大越慢，越糯）
        );
    }
    #endregion

    #region 碰撞处理（低回弹+速度衰减）
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 碰撞后速度大幅衰减（瘫软感）
        rb.velocity *= collisionSpeedDamp;
        // 碰撞后轻微旋转（更自然）
        rb.angularVelocity = Random.Range(-1f, 1f);
    }
    #endregion

    #region 辅助函数
    // 重置果冻到初始状态
    public void ResetJelly()
    {
        isLaunched = false;
        transform.position = basePosition;
        transform.localScale = originalSize;
        transform.rotation = Quaternion.identity;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    // 边界检测（超出屏幕后重置）
    private void CheckBounds()
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPos.x < -50 || screenPos.x > Screen.width + 50 || screenPos.y < -50)
        {
            ResetJelly();
        }
    }
    #endregion

    // Gizmos辅助调试（场景视图显示果冻半径）
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, jellyRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(basePosition, 0.1f); // 初始位置标记
    }
}