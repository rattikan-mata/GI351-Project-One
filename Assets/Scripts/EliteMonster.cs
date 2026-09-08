using UnityEngine;
using System.Collections;

public class EliteMonster : Monster
{
    [Header("Elite Monster Settings")]
    [Tooltip("ระยะเวลาที่มอนสเตอร์จะหยุดนิ่งให้ตีรัวๆ (วินาที)")]
    [SerializeField] private float spamDuration = 2f;

    [Header("Potion Drop Settings")]
    [Tooltip("ลาก Prefab ของขวดยามาใส่ตรงนี้")]
    [SerializeField] private GameObject potionPrefab;

    [Header("Juiciness (ตีแล้วสะใจ)")]
    [Tooltip("ขนาดของมอนสเตอร์ตอนโดนตี (เช่น 1.3 คือใหญ่ขึ้น 30%)")]
    [SerializeField] private float hitScaleMultiplier = 1.3f;
    [Tooltip("เวลาที่ใช้ในการเด้งขยายแล้วหดกลับ (วินาที)")]
    [SerializeField] private float hitScaleDuration = 0.1f;
    [Tooltip("ระยะเวลาจอสั่นตอนโดนตีแต่ละครั้ง")]
    [SerializeField] private float shakeDuration = 0.1f;
    [Tooltip("ความแรงของการสั่นของหน้าจอ")]
    [SerializeField] private float shakeMagnitude = 0.15f;

    private bool isSpamPhase = false;
    private float spamTimer = 0f;
    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    protected override void Awake()
    {
        base.Awake();
        monsterType = MonsterType.ElithMonster; //[cite: 23]
        originalScale = transform.localScale; // จำขนาดดั้งเดิมไว้
    }

    protected override void Update()
    {
        if (isSpamPhase)
        {
            spamTimer -= Time.deltaTime; //[cite: 23]
            if (spamTimer <= 0)
            {
                // สร้างขวดยาก่อนที่มอนสเตอร์จะหายไป //[cite: 23]
                DropPotion(); //[cite: 23]
                base.Die(); // ตายจริง //[cite: 23]
            }
            return; // ให้อยู่เฉยๆ ไม่ต้องเดินหน้า //[cite: 23]
        }
        base.Update(); //[cite: 23]
    }

    private void DropPotion()
    {
        if (potionPrefab != null)
        {
            Instantiate(potionPrefab, cachedTransform.position, Quaternion.identity); //[cite: 23]
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (isSpamPhase) return; //[cite: 23]
        base.OnTriggerEnter2D(collision); //[cite: 23]
    }

    // ฟังก์ชัน Die() จะถูกเรียกทุกครั้งที่ผู้เล่นกดตีโดน (จาก PlayerController)
    public override void Die()
    {
        if (!isSpamPhase)
        {
            // --- โดนตีครั้งแรก เข้าสู่โหมดให้ตีรัวๆ ---
            isSpamPhase = true; //[cite: 23]
            spamTimer = spamDuration; //[cite: 23]

            if (timingRing != null)
            {
                timingRing.gameObject.SetActive(false); //[cite: 23]
            }

            TriggerHitFeedback();
        }
        else
        {
            // --- โดนตีซ้ำตอนกำลังหยุดนิ่ง ---
            TriggerHitFeedback();
        }
    }

    // รวม Effect ความสะใจไว้ที่นี่
    private void TriggerHitFeedback()
    {
        // 1. สั่งจอสั่น
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.TriggerShake(shakeDuration, shakeMagnitude);
        }

        // 2. สั่งตัวเด้ง (Squash & Stretch)
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleRoutine());

        // 3. เล่นเสียงโดนตี (มีระบบกันเสียงพังใน AudioManager กรองให้แล้ว)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEliteMonsterHit();
        }
    }

    // แอนิเมชันตอนตัวเด้ง
    private IEnumerator ScaleRoutine()
    {
        // ขยายตัวขึ้นทันที
        transform.localScale = originalScale * hitScaleMultiplier;

        float elapsed = 0f;
        while (elapsed < hitScaleDuration)
        {
            elapsed += Time.deltaTime;
            // ค่อยๆ หดกลับมาขนาดเดิม
            transform.localScale = Vector3.Lerp(originalScale * hitScaleMultiplier, originalScale, elapsed / hitScaleDuration);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}