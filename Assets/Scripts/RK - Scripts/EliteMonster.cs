using UnityEngine;

public class EliteMonster : Monster
{
    [Header("Elite Monster Settings")]
    [Tooltip("ระยะเวลาที่มอนสเตอร์จะหยุดนิ่งให้ตีรัวๆ (วินาที)")]
    [SerializeField] private float spamDuration = 2f;

    private bool isSpamPhase = false;
    private float spamTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        monsterType = MonsterType.ElithMonster;
    }

    protected override void Update()
    {
        if (isSpamPhase)
        {
            spamTimer -= Time.deltaTime;
            if (spamTimer <= 0)
            {
                base.Die();
            }
            return;
        }
        base.Update();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (isSpamPhase) return;
        base.OnTriggerEnter2D(collision);
    }

    public override void Die()
    {
        if (!isSpamPhase)
        {
            isSpamPhase = true;
            spamTimer = spamDuration;

            if (timingRing != null)
            {
                timingRing.gameObject.SetActive(false);
            }
        }
    }
}