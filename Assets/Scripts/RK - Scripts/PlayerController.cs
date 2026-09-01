using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Hit Settings")]
    [SerializeField] private Transform hitPoint;
    [SerializeField] private float hitRadius = 1.2f;
    [SerializeField] private LayerMask monsterLayer;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 2f;
    [SerializeField] private float flashInterval = 0.15f;

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Secret Win Settings")]
    [SerializeField] private float walkSpeed = 3f; 

    private Transform cachedTransform;
    private bool isInvincible = false;
    private bool isDead = false;
    private bool isDashing = false;
    private bool isWalkingToSecret = false;
    private Transform targetSecretChar;
    private Vector3 originalLocalPos;

    // Cache Animator Hashes ป้องกัน GC และเพิ่มความเร็วแอนิเมชัน
    private static readonly int HitTrigger = Animator.StringToHash("Hit");
    private static readonly int DeathTrigger = Animator.StringToHash("Death");

    // Cache Yield Instructions
    private WaitForSeconds waitFlash;
    private WaitForSeconds waitDash;

    private void Awake()
    {
        Instance = this;
        cachedTransform = transform;
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        waitFlash = new WaitForSeconds(flashInterval);
        waitDash = new WaitForSeconds(0.2f);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        originalLocalPos = cachedTransform.localPosition;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHeartsUI(currentHealth);
        }
    }

    private void Update()
    {
        if (isDead) return;

        if (isWalkingToSecret)
        {
            if (targetSecretChar != null)
            {
                cachedTransform.position = Vector3.MoveTowards(cachedTransform.position, targetSecretChar.position, walkSpeed * Time.deltaTime);
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PerformHit();
        }
    }

    private void PerformHit()
    {
        if (animator != null) animator.SetTrigger(HitTrigger);

        if (!isDashing && !isWalkingToSecret)
        {
            StartCoroutine(QuickDashRoutine());
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
        if (isInvincible || isDead) return;

        currentHealth--;
        ScoreManager.Instance.RegisterMiss();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHeartsUI(currentHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    private void Die()
    {
        isDead = true;
        if (animator != null) animator.SetTrigger(DeathTrigger);
        GameManager.Instance.TriggerGameOver();
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
            yield return waitFlash;
            elapsed += flashInterval;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        isInvincible = false;
    }

    private IEnumerator QuickDashRoutine()
    {
        isDashing = true;
        Vector3 basePos = cachedTransform.localPosition;
        cachedTransform.localPosition = basePos + (Vector3.right * 1.2f);

        yield return waitDash;

        if (!isWalkingToSecret)
        {
            cachedTransform.localPosition = basePos;
        }
        isDashing = false;
    }

    public void StartWalkingToSecret(Transform secretTarget)
    {
        isWalkingToSecret = true;
        targetSecretChar = secretTarget;
    }

    private void OnDrawGizmosSelected()
    {
        if (hitPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPoint.position, hitRadius);
    }
}