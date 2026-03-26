using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

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

    [Header("击中效果")]
    public GameObject hitParticlePrefab;
    private float fixedScale = 0.1f;
    private SpriteRenderer sr;
    private CircleCollider2D col;
    private static GameObject[] allTargets;
    private Color originalColor;

    //积分系统
    public static int count;
    public TextMeshProUGUI countText;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<CircleCollider2D>();
        originalColor = sr.color;

        PhysicsMaterial2D bounceMaterial = new PhysicsMaterial2D();
        bounceMaterial.bounciness = 0.6f;
        bounceMaterial.friction = 0.2f;
        col.sharedMaterial = bounceMaterial;

        allTargets = GameObject.FindGameObjectsWithTag("Target");
        SetRandomPosition();
        ForceScale();

        count = 0;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("JellyBall"))
        {
            SpawnHitParticle(originalColor);
            StartCoroutine(FadeOutAndHide());
            AddCount();
        }
    }

    // ==============================================
    // ✨ 渐变消失效果（超级顺滑）
    // ==============================================
    IEnumerator FadeOutAndHide()
    {
        col.enabled = false; // 击中后立刻关闭碰撞
        float fadeDuration = 0.25f;
        float elapsed = 0;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, a);
            yield return null;
        }

        sr.enabled = false;
        yield return new WaitForSeconds(refreshTime);
        RefreshTarget();
    }

    void SpawnHitParticle(Color targetColor)
    {
        if (hitParticlePrefab == null) return;

        GameObject particle = Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
        ParticleSystem ps = particle.GetComponent<ParticleSystem>();

        if (ps != null)
        {
            ps.Stop();
            ps.Clear();
            var main = ps.main;
            main.startColor = new Color(targetColor.r, targetColor.g, targetColor.b, 1f);
            ps.Play();
        }

        ParticleSystemRenderer renderer = particle.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = "UI";
            renderer.sortingOrder = 200;
        }

        Destroy(particle, 1f);
    }

    void RefreshTarget()
    {
        sr.color = originalColor;
        sr.enabled = true;
        col.enabled = true;
        SetRandomPosition();
        ForceScale();
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
            newPos = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0);
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

    void AddCount()
    {
        count++;
        countText.text =count.ToString();
    }

    public void Restart()
    {
        count = 0;
        SceneManager.LoadScene("StartScene");
        Time.timeScale = 1f;
    }
}