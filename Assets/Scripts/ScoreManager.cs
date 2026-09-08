using UnityEngine;
using System.Collections.Generic;

#region Rage Data Structure
[System.Serializable]
public class RagePhaseData
{
    public string phaseName = "Phase 1";
    public float requiredRage = 30f;

    [Tooltip("ค่าบวกเพิ่มให้ความเร็วฉากหลัง (เช่น 0.2 คือบวกเพิ่มไปอีก +20%)")]
    public float bgSpeedBonus = 0.2f;

    [Tooltip("ค่าบวกเพิ่มให้ความเร็วมอนสเตอร์ (เช่น 0.2 คือบวกเพิ่มไปอีก +20%)")]
    public float monsterSpeedBonus = 0.2f;

    [Tooltip("ตัวคูณคะแนนเมื่ออยู่ในเฟสนี้ (อันนี้เป็นคูณเหมือนเดิม)")]
    public float scoreMultiplier = 1.5f;
}
#endregion

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    #region Score & Combo System
    [Header("Score Settings")]
    public int CurrentScore { get; private set; }
    public int CurrentCombo { get; private set; }
    public int MaxCombo { get; private set; }
    public int TotalMisses { get; private set; }

    [Header("Feedback Colors")]
    [SerializeField] private Color hitColor = Color.green;
    [SerializeField] private Color missColor = Color.red;
    #endregion

    #region Rage System
    [Header("Rage Settings - Core")]
    public float maxRage = 100f;
    public float currentRage = 0f;

    [Tooltip("The Rage gauge slowly depletes over time")]
    public float rageDecreasePerSec = 0.5f;

    [Tooltip("The more monsters you kill, the more Rage points you earn")]
    public float ragePerKill = 3f;

    [Tooltip("Drops significantly when you miss an attack")]
    public float ragePenaltyPerMiss = 5f;

    [Header("Rage Phases Configuration")]
    public List<RagePhaseData> ragePhases = new List<RagePhaseData>();

    private RagePhaseData currentActivePhase = null;
    #endregion

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // จัดเรียงเฟสจากมากไปน้อย
        ragePhases.Sort((a, b) => b.requiredRage.CompareTo(a.requiredRage));
    }

    private void Start()
    {
        ResetAll();
    }

    private void Update()
    {
        #region Rage Depletion Over Time
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
        CurrentCombo++;
        if (CurrentCombo > MaxCombo) MaxCombo = CurrentCombo;

        float phaseScoreMultiplier = (currentActivePhase != null) ? currentActivePhase.scoreMultiplier : 1f;
        int gainedScore = Mathf.RoundToInt((baseMonsterScore + (CurrentCombo * 10)) * phaseScoreMultiplier);

        CurrentScore += gainedScore;

        currentRage += ragePerKill;
        currentRage = Mathf.Clamp(currentRage, 0f, maxRage);
        CheckRagePhase();

        Debug.Log($"HIT! Combo = {CurrentCombo} | Score = {CurrentScore} | Rage = {currentRage:F1}");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScoreUI(CurrentScore, CurrentCombo);
            UIManager.Instance.ShowFeedback("Hit!", hitColor);
        }
    }

    public void RegisterMiss()
    {
        CurrentCombo = 0;
        TotalMisses++;

        currentRage -= ragePenaltyPerMiss;
        currentRage = Mathf.Clamp(currentRage, 0f, maxRage);
        CheckRagePhase();

        Debug.Log($"MISS! Combo Broken | Rage = {currentRage:F1}");

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

        foreach (var phase in ragePhases)
        {
            if (currentRage >= phase.requiredRage)
            {
                newPhase = phase;
                break;
            }
        }

        if (newPhase != currentActivePhase)
        {
            bool wasTopPhase = ragePhases.Count > 0 && currentActivePhase == ragePhases[0];
            bool isNewTopPhase = ragePhases.Count > 0 && newPhase == ragePhases[0];

            if (isNewTopPhase && !wasTopPhase)
            {
                AudioManager.Instance?.PlayRageUp();
                AudioManager.Instance?.PlayRageStage3Loop();
            }
            else if (!isNewTopPhase && wasTopPhase)
            {
                AudioManager.Instance?.StopRageStage3Loop();
            }

            float oldRequiredRage = (currentActivePhase != null) ? currentActivePhase.requiredRage : 0f;
            float newRequiredRage = (newPhase != null) ? newPhase.requiredRage : 0f;
            bool isLevelingUp = newRequiredRage > oldRequiredRage;

            if (isLevelingUp && newPhase != null && !isNewTopPhase)
            {
                AudioManager.Instance?.PlayRageUp();
            }

            currentActivePhase = newPhase;

            float bgBonus = (currentActivePhase != null) ? currentActivePhase.bgSpeedBonus : 0f;
            float monBonus = (currentActivePhase != null) ? currentActivePhase.monsterSpeedBonus : 0f;
            string phaseName = (currentActivePhase != null) ? currentActivePhase.phaseName : "Normal";

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateRageBonus(bgBonus, monBonus);
            }

            // --- ส่วนที่ต้องเพิ่มใหม่ (อัปเดตแอนิเมชันหัวใจ) ---
            int phaseLevel = 0; // 0 = ปิดหมด (โหมดปกติ)
            if (currentActivePhase != null)
            {
                // คำนวณหาเฟส 1, 2, 3 โดยอิงจากลำดับลิสต์ (ลิสต์ถูกเรียงจากมากไปน้อย)
                int index = ragePhases.IndexOf(currentActivePhase);
                phaseLevel = ragePhases.Count - index;
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdatePulsePhase(phaseLevel); // สั่งเปิด GameObject ให้ตรงกับเฟส[cite: 38, 40]
            }
            // ------------------------------------------

            Debug.Log($"[RAGE SYSTEM] Phase: {phaseName} | BG Bonus: +{bgBonus} | Monster Bonus: +{monBonus}");
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

        AudioManager.Instance?.StopRageStage3Loop(); // กันเสียงค้างถ้ารีเซ็ตตอนอยู่ Stage 3 พอดี

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateRageBonus(0f, 0f); // ตอนเริ่มเกม Bonus = 0
        }
    }
}