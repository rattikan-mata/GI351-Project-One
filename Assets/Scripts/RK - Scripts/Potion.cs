using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Potion : MonoBehaviour
{
    [Header("Drop Pop Settings")]
    [Tooltip("ความสูงที่กระเด็นเด้งขึ้นตอนแรก")]
    [SerializeField] private float popHeight = 1.2f;
    [Tooltip("ระยะกระเด็นออกมาก่อนจะพุ่งเข้าหาผู้เล่น")]
    [SerializeField] private float popDistance = 1f;
    [Tooltip("เวลาในช่วงเด้งออกตอนแรก (วินาที)")]
    [SerializeField] private float popDuration = 0.25f;

    [Header("Homing Magnet Settings")]
    [Tooltip("ความเร่งในการพุ่งเข้าหาผู้เล่น (ยิ่งเข้าใกล้ ยิ่งเร็วขึ้นแบบสมูท)")]
    [SerializeField] private float acceleration = 18f;
    [Tooltip("ความเร็วสูงสุดในการพุ่ง")]
    [SerializeField] private float maxSpeed = 15f;

    [Header("Heal Settings")]
    [SerializeField] private int healAmount = 1;

    private void Start()
    {
        StartCoroutine(DropAndHomingRoutine());
    }

    private IEnumerator DropAndHomingRoutine()
    {
        Vector3 startPos = transform.position;
        // กระเด็นถอยหลังไปทางขวาเล็กน้อยก่อน (เนื่องจากมอนเดินมาทางซ้าย)
        Vector3 popTarget = startPos + new Vector3(popDistance, 0f, 0f);

        // --- เฟสที่ 1: เด้งออกด้านนอกชั่วครู่ ---
        float elapsed = 0f;
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;

            float currentX = Mathf.Lerp(startPos.x, popTarget.x, t);
            float currentY = startPos.y + Mathf.Sin(t * Mathf.PI) * popHeight;

            transform.position = new Vector3(currentX, currentY, startPos.z);
            yield return null;
        }

        // --- เฟสที่ 2: พุ่งเข้าหาตัวผู้เล่นแบบแม่เหล็ก (Homing Magnet) ---
        float currentSpeed = 3f; // เริ่มต้นด้วยความเร็วต้นเบาๆ

        while (true)
        {
            // ป้องกัน Error กรณีผู้เล่นหายไปหรือเกมจบ
            if (PlayerController.Instance == null) yield break;

            // ติดตามตำแหน่งของผู้เล่นตลอดเวลาแบบเรียลไทม์
            Vector3 targetPos = PlayerController.Instance.transform.position;

            // เพิ่มความเร็วขึ้นเรื่อยๆ ตามเวลา (Acceleration) จนถึง MaxSpeed
            currentSpeed = Mathf.Min(currentSpeed + (acceleration * Time.deltaTime), maxSpeed);

            // เคลื่อนที่เข้าหาตัวผู้เล่นอย่างสมูท
            transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent<PlayerController>(out var player))
            {
                player.Heal(healAmount);
                Destroy(gameObject); // เก็บแล้วขวดยาหายไปทันที
            }
        }
    }
}