using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Monster : MonoBehaviour
{
    #region Variables
    public enum MonsterType { Monster1, ElithMonster }

    [Header("Monster Type")]
    [SerializeField] protected MonsterType monsterType = MonsterType.Monster1;

    [SerializeField] protected float despawnX = -15f;
    protected Transform cachedTransform;

    [Header("Timing Ring (Visual Only)")]
    [SerializeField] protected Transform timingRing;
    [SerializeField] protected float startShrinkDistance = 4f;
    [SerializeField] protected Vector3 maxRingScale = new Vector3(2.5f, 2.5f, 1f);
    [SerializeField] protected Vector3 targetRingScale = new Vector3(1f, 1f, 1f);
    #endregion

    protected virtual void Awake()
    {
        cachedTransform = transform;
        if (timingRing != null)
        {
            timingRing.gameObject.SetActive(false);
        }
    }

    protected virtual void Update()
    {
        float speed = (GameManager.Instance != null) ? GameManager.Instance.MonsterSpeed : 5f;
        cachedTransform.position += Vector3.left * (speed * Time.deltaTime);

        UpdateTimingRing();

        if (cachedTransform.position.x <= despawnX)
        {
            Destroy(gameObject);
        }
    }

    protected void UpdateTimingRing()
    {
        if (timingRing == null || PlayerController.Instance == null || PlayerController.Instance.HitZone == null) return;

        float distance = Mathf.Abs(PlayerController.Instance.HitZone.position.x - cachedTransform.position.x);

        if (distance <= startShrinkDistance)
        {
            if (!timingRing.gameObject.activeSelf) timingRing.gameObject.SetActive(true);
            float t = Mathf.InverseLerp(startShrinkDistance, 0f, distance);
            timingRing.localScale = Vector3.Lerp(maxRingScale, targetRingScale, t);
        }
        else
        {
            if (timingRing.gameObject.activeSelf) timingRing.gameObject.SetActive(false);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent<PlayerController>(out var player))
            {
                player.TakeDamage();
            }
        }
    }

    
    public virtual void Die()
    {
        PlayDeathSound();
        Destroy(gameObject);
    }

    protected void PlayDeathSound()
    {
        if (AudioManager.Instance == null) return;

        switch (monsterType)
        {
            case MonsterType.Monster1:
                AudioManager.Instance.PlayMonster1Dead();
                break;
            case MonsterType.ElithMonster:
                AudioManager.Instance.PlayElithMonsterDead();
                break;
        }
    }

    protected virtual void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMonsterDespawnedOrKilled();
        }
    }
}