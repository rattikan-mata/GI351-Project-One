using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Hit Settings")]
    [SerializeField] private Transform hitPoint;
    [SerializeField] private float hitRadius = 1.2f;
    [SerializeField] private LayerMask monsterLayer;

    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 2f;
    [SerializeField] private float flashInterval = 0.15f;

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite deathSprite;

    private bool isDead = false;
    private bool isInvincible = false;
    private int hp = 3;

    private bool isDashing = false;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHearts(hp);
        }
    }

    private void Update()
    {
        if (isDead) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PerformHit();
        }
    }

    private void PerformHit()
    {
        Debug.Log("Player Hit");

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        if (!isDashing)
        {
            StartCoroutine(QuickDash());
        }

        Collider2D hitMonster = Physics2D.OverlapCircle(hitPoint.position, hitRadius, monsterLayer);


        if (hitMonster != null)
        {
            ScoreManager.Instance.RegisterHit();
            Destroy(hitMonster.gameObject);
        }
        else
        {
            ScoreManager.Instance.RegisterMiss();
        }
    }

    public void TakeDamage()
    {
        if (isInvincible) return;

        hp--;

        Debug.Log("HP = " + hp);

        if (UIManager.Instance != null) { UIManager.Instance.UpdateHearts(hp); }

        if (hp <= 0)
        {
            Die();
            return;
        }

        ScoreManager.Instance.RegisterMiss();
        StartCoroutine(InvincibilityRoutine());
    }

    void Die()
    {
        isDead = true;

        if (animator != null)
            animator.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.sprite = deathSprite;

        Debug.Log("PLAYER HAS DIED");
    }
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float elapsed = 0f;
        Color originalColor = (spriteRenderer != null) ? spriteRenderer.color : Color.white;

        while (elapsed < invincibilityDuration)
        {
            if (spriteRenderer != null)
            {
                float alpha = (spriteRenderer.color.a == 1f) ? 0.2f : 1f;
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            }
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        isInvincible = false;
        Debug.Log("Normal State");
    }

    private void OnDrawGizmosSelected()
    {
        if (hitPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPoint.position, hitRadius);
    }

    private IEnumerator QuickDash()
    {
        isDashing = true; // ล็อคการพุ่ง

        transform.position += Vector3.right * 1.5f;
        yield return new WaitForSeconds(0.3f);
        transform.position -= Vector3.right * 1.5f;

        isDashing = false; // ปลดล็อคเมื่อกลับมาที่เดิมแล้ว
    }
}