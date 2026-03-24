using UnityEngine;

// 靶心击中脚本，挂载到3个靶心上
public class TargetHit : MonoBehaviour
{
    private Vector3 originalPos; // 靶心原始位置（刷新用）
    private SpriteRenderer sr;   // 靶心渲染组件（实现放大效果）
    private Color originalColor; // 靶心原始颜色

    [Header("靶心效果参数")]
    public float hitScale = 1.5f; // 击中放大比例
    public float resetTime = 3f;  // 刷新时间，策划要求3秒
    public int scoreAdd = 10;     // 击中加分，策划要求分数直接涨
    public static int totalScore; // 总分数（全局生效）

    void Start()
    {
        // 初始化原始位置、颜色、渲染组件
        originalPos = transform.position;
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
        totalScore = 0; // 初始分数为0
    }

    // 2D触发检测（果冻击中靶心时执行）
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 判断是否是果冻球击中
        if (other.gameObject.CompareTag("JellyBall"))
        {
            HitEffect(); // 执行击中效果
            Invoke("ResetTarget", resetTime); // 3秒后刷新靶心
        }
    }

    // 击中特效（策划要求：放大+粒子+音效+加分）
    private void HitEffect()
    {
        // 靶心放大
        transform.localScale = Vector3.one * hitScale;
        // 分数增加
        totalScore += scoreAdd;
        Debug.Log("当前分数：" + totalScore); // 控制台打印分数，后续可加UI显示
        // 播放击中音效+粒子特效（后续第四步加，先留位置）
        // PlayHitAudio();
        // SpawnHitParticle();
    }

    // 3秒后刷新靶心（复原位置+大小+激活）
    private void ResetTarget()
    {
        transform.position = originalPos;
        transform.localScale = Vector3.one;
        gameObject.SetActive(true);
    }
}