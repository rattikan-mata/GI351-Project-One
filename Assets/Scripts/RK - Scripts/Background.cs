using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Background : MonoBehaviour
{
    private Transform cachedTransform;
    private float spriteWidth;
    private float resetThreshold;
    private float doubleWidth;

    private void Awake()
    {
        cachedTransform = transform;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        spriteWidth = sr.bounds.size.x;
        resetThreshold = -spriteWidth;
        doubleWidth = spriteWidth * 2f;
    }

    private void Update()
    {
        float speed = (GameManager.Instance != null) ? GameManager.Instance.GameSpeed : 5f;
        cachedTransform.position += Vector3.left * (speed * Time.deltaTime);

        if (cachedTransform.position.x <= resetThreshold)
        {
            cachedTransform.position += Vector3.right * doubleWidth;
        }
    }
}