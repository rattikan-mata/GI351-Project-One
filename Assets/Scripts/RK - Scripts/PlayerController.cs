using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

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
    [SerializeField] private AudioSource audioSource;

    [Header("Sound Settings")]
    [SerializeField] private AudioClip[] hurtSounds;
    [Range(0f, 1f)]
    [SerializeField] private float hurtSoundVolume = 1f;
    [SerializeField] private bool limitHurtSoundDuration = false;
    [SerializeField] private float maxHurtSoundDuration = 1f;

    private Coroutine stopHurtSoundCoroutine;

    private bool isInvincible = false;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
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

        Debug.Log("Monster Damage");
        ScoreManager.Instance.RegisterMiss();

        if (audioSource != null && hurtSounds != null && hurtSounds.Length > 0)
        {
            AudioClip clipToPlay = hurtSounds[Random.Range(0, hurtSounds.Length)];

            audioSource.Stop();
            audioSource.PlayOneShot(clipToPlay, hurtSoundVolume);

            if (stopHurtSoundCoroutine != null)
            {
                StopCoroutine(stopHurtSoundCoroutine);
            }

            if (limitHurtSoundDuration)
            {
                stopHurtSoundCoroutine = StartCoroutine(StopHurtSoundAfterDelay(maxHurtSoundDuration));
            }
        }

        StartCoroutine(InvincibilityRoutine());
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
    private IEnumerator StopHurtSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        stopHurtSoundCoroutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (hitPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPoint.position, hitRadius);
    }
}