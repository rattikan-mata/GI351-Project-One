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

    private bool isInvincible = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PerformHit();
        }
    }

    private void PerformHit()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hit");
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

        ScoreManager.Instance.RegisterMiss();
        StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        isInvincible = false;
        Debug.Log("[PLAYER] Invincibility ended.");
    }

    private void OnDrawGizmosSelected()
    {
        if (hitPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPoint.position, hitRadius);
    }
}