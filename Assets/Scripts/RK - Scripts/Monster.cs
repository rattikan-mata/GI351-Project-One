using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Monster : MonoBehaviour
{
    [SerializeField] private float despawnX = -15f;

    private void Update()
    {
        float speed = (GameManager.Instance != null) ? GameManager.Instance.GameSpeed : 5f;
        transform.Translate(Vector2.left * (speed * Time.deltaTime));

        if (transform.position.x <= despawnX)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage();
            }
        }
    }
}