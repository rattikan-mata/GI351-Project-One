using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Background : MonoBehaviour
{
    private float spriteWidth;
    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        spriteWidth = sr.bounds.size.x;
    }

    private void Update()
    {
        float speed = (GameManager.Instance != null) ? GameManager.Instance.GameSpeed : 5f;
        transform.Translate(Vector2.left * (speed * Time.deltaTime));

        // เมื่อเลื่อนจนสุดความกว้างของ Sprite ให้ย้ายกลับไปตำแหน่งเริ่มต้น
        if (transform.position.x <= startPosition.x - spriteWidth)
        {
            transform.position = startPosition;
        }
    }
}