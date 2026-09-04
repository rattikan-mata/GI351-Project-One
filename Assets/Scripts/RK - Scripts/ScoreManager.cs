using UnityEngine;
using System.Collections.Generic;

#region Rage Data Structure
[System.Serializable]
public class RagePhaseData
{
    public string phaseName = "Phase 1";
    public float requiredRage = 30f;           // แต้ม Rage ที่ต้องการเพื่อเข้าเฟสนี้
    public float bgSpeedMultiplier = 1.2f;     // ตัวคูณความเร็วฉากหลัง
    public float monsterSpeedMultiplier = 1.2f;// ตัวคูณความเร็วมอนสเตอร์
    public float scoreMultiplier = 1.5f;       // ตัวคูณคะแนนเมื่ออยู่ในเฟสนี้
}
#endregion

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    #region Score & Combo System
    [Header("Score Settings")]
    public int CurrentScore { get; private set; }
    public int CurrentCombo { get; private set; }
    public int TotalMisses { get; private set; }

    [Header("Feedback Colors")]
    [SerializeField] private Color hitColor = Color.green;
    [SerializeField] private Color missColor = Color.red;
    #endregion

    #region Rage System
    [Header("Rage Settings - Core")]
    public float maxRage = 100f;               // หลอด Rage สูงสุด
    public float currentRage = 0f;

    [Tooltip("The Rage gauge slowly depletes over time")]
    public float rageDecreasePerSec = 0.5f;

    [Tooltip("The more monsters you kill, the more Rage points you earn")]
    public float ragePerKill = 3f;

    [Tooltip("Drops significantly when you miss an attack")]
    public float ragePenaltyPerMiss = 5f;

    [Header("Rage Phases Configuration")]
    [Tooltip("ตั้งค่าเฟสของ Rage (สคริปต์จะจัดการเรียงลำดับให้อัตโนมัติ)")]
    public List<RagePhaseData> ragePhases = new List<RagePhaseData>();

    private RagePhaseData currentActivePhase = null; // เก็บข้อมูลเฟสที่กำลังทำงานอยู่
    #endregion

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // จัดเรียงเฟสตามแต้ม requiredRage จากมากไปน้อย (เพื่อการเช็คเงื่อนไขที่ถูกต้อง)
        ragePhases.Sort((a, b) => b.requiredRage.CompareTo(a.requiredRage));
    }

    private void Start()
    {
        ResetAll();
    }

    private void Update()
    {
        #region Rage Depletion Over Time
        // "Losing Rage: The Rage gauge slowly depletes over time"
        if (currentRage > 0)
        {
            currentRage -= rageDecreasePerSec * Time.deltaTime;
            currentRage = Mathf.Clamp(currentRage, 0f, maxRage);
            CheckRagePhase();
        }
        #endregion
    }

    #region Hit & Miss Logic
    public void RegisterHit(int baseMonsterScore = 100)
    {
        // 1. จัดการ Combo & Score
        CurrentCombo++;

        // เช็คว่ามีตัวคูณคะแนนจาก Rage Phase ไหม (ถ้าไม่มีให้คูณ 1)
        float phaseScoreMultiplier = (currentActivePhase != null) ? currentActivePhase.scoreMultiplier : 1f;
        int gainedScore = Mathf.RoundToInt((baseMonsterScore + (CurrentCombo * 10)) * phaseScoreMultiplier);

        CurrentScore += gainedScore;

        // 2. จัดการ Rage (Building Rage)
        currentRage += ragePerKill;
        currentRage = Mathf.Clamp(currentRage, 0f, maxRage);
        CheckRagePhase();

        Debug.Log($"HIT! Combo = {CurrentCombo} | Score = {CurrentScore} | Rage = {currentRage:F1}");

        // 3. อัปเดต UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScoreUI(CurrentScore, CurrentCombo);
            UIManager.Instance.ShowFeedback("Hit!", hitColor);
        }
    }

    public void RegisterMiss()
    {
        // 1. จัดการ Combo
        CurrentCombo = 0;
        TotalMisses++;

        // 2. จัดการ Rage (Drops significantly)
        currentRage -= ragePenaltyPerMiss;
        currentRage = Mathf.Clamp(currentRage, 0f, maxRage);
        CheckRagePhase();

        Debug.Log($"MISS! Combo Broken | Rage = {currentRage:F1}");

        // 3. อัปเดต UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScoreUI(CurrentScore, CurrentCombo);
            UIManager.Instance.ShowFeedback("Miss!", missColor);
        }
    }
    #endregion

    #region Rage Phase Calculation
    private void CheckRagePhase()
    {
        RagePhaseData newPhase = null;

        // ค้นหาเฟสที่เข้าเงื่อนไข (เนื่องจากเรียงจากมากไปน้อยแล้ว ตัวแรกที่ผ่านเงื่อนไขคือเฟสสูงสุดปัจจุบัน)
        foreach (var phase in ragePhases)
        {
            if (currentRage >= phase.requiredRage)
            {
                newPhase = phase;
                break;
            }
        }

        // ตรวจสอบว่ามีการเปลี่ยนเฟสหรือไม่
        if (newPhase != currentActivePhase)
        {
            currentActivePhase = newPhase;

            // ดึงค่า Speed Multiplier ส่งไปให้ GameManager
            float bgMult = (currentActivePhase != null) ? currentActivePhase.bgSpeedMultiplier : 1f;
            float monMult = (currentActivePhase != null) ? currentActivePhase.monsterSpeedMultiplier : 1f;
            string phaseName = (currentActivePhase != null) ? currentActivePhase.phaseName : "Normal";

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateRageMultipliers(bgMult, monMult);
            }

            Debug.Log($"[RAGE SYSTEM] Entered Phase: {phaseName} | BG Speed: {bgMult}x | Monster Speed: {monMult}x");
        }
    }
    #endregion

    public void ResetAll()
    {
        CurrentScore = 0;
        CurrentCombo = 0;
        TotalMisses = 0;
        currentRage = 0f;
        currentActivePhase = null;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateRageMultipliers(1f, 1f);
        }
    }
}