using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Background : MonoBehaviour
{
    private float spriteWidth;

    private void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        spriteWidth = sr.bounds.size.x;
    }

    private void Update()
    {
        float speed = (GameManager.Instance != null) ? GameManager.Instance.GameSpeed : 5f;
        transform.position += Vector3.left * (speed * Time.deltaTime);

        if (transform.position.x <= -spriteWidth)
        {
            transform.position += Vector3.right * (spriteWidth * 2f);
        }
    }
}