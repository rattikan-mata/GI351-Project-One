using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Monster : MonoBehaviour
{
    [SerializeField] private float despawnX = -15f;
    private Transform cachedTransform;

    [Header("Timing Ring (Visual Only)")]
    [Tooltip("ลาก Empty GameObject ที่เป็นวงแหวนมาใส่ตรงนี้")]
    [SerializeField] private Transform timingRing;

    [Tooltip("ระยะห่างที่จะเริ่มโชว์วงแหวนและค่อยๆ หด")]
    [SerializeField] private float startShrinkDistance = 4f;

    [Tooltip("สเกลของวงแหวนตอนที่เพิ่งเริ่มโชว์")]
    [SerializeField] private Vector3 maxRingScale = new Vector3(2.5f, 2.5f, 1f);

    [Tooltip("สเกลของวงแหวนตอนที่ทับจุดตีพอดีเป๊ะ")]
    [SerializeField] private Vector3 targetRingScale = new Vector3(1f, 1f, 1f);

    private void Awake()
    {
        cachedTransform = transform;

        // ซ่อนวงแหวนไว้ก่อนตอนมอนสเตอร์เพิ่งเกิด
        if (timingRing != null)
        {
            timingRing.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        float speed = (GameManager.Instance != null) ? GameManager.Instance.MonsterSpeed : 5f;
        cachedTransform.position += Vector3.left * (speed * Time.deltaTime);

        // อัปเดตขนาดวงแหวนตลอดเวลา
        UpdateTimingRing();

        if (cachedTransform.position.x <= despawnX)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateTimingRing()
    {
        // ถ้าไม่ได้ใส่วงแหวนมา หรือหาตัวผู้เล่นไม่เจอ ให้ข้ามไปเลย
        if (timingRing == null || PlayerController.Instance == null || PlayerController.Instance.HitZone == null) return;

        // คำนวณระยะห่างแกน X ระหว่างจุดตีของผู้เล่นกับมอนสเตอร์
        float distance = Mathf.Abs(PlayerController.Instance.HitZone.position.x - cachedTransform.position.x);

        // ถ้าระยะทางเข้าน้อยกว่าที่ตั้งไว้ ให้เริ่มหดวงแหวน
        if (distance <= startShrinkDistance)
        {
            if (!timingRing.gameObject.activeSelf) timingRing.gameObject.SetActive(true);

            // คำนวณอัตราส่วนการหด (t = 0 คือเพิ่งเข้าระยะ, t = 1 คืออยู่ทับจุดตีพอดี)
            float t = Mathf.InverseLerp(startShrinkDistance, 0f, distance);

            // ย่อขนาดวงแหวน
            timingRing.localScale = Vector3.Lerp(maxRingScale, targetRingScale, t);
        }
        else
        {
            // ถ้ายังอยู่ไกลเกิน ให้ซ่อนไว้
            if (timingRing.gameObject.activeSelf) timingRing.gameObject.SetActive(false);
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