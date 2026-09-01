using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Monster : MonoBehaviour
{
    [SerializeField] private float despawnX = -15f;
    private Transform cachedTransform;

    private void Awake()
    {
        cachedTransform = transform;
    }

    private void Update()
    {
        float speed = (GameManager.Instance != null) ? GameManager.Instance.GameSpeed : 5f;
        cachedTransform.position += Vector3.left * (speed * Time.deltaTime);

        if (cachedTransform.position.x <= despawnX)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent<PlayerController>(out var player))
            {
                player.TakeDamage();
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMonsterDespawnedOrKilled();
        }
    }
}