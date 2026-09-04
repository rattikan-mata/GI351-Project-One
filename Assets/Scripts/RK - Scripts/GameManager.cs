using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveData
{
    public string waveName;
    public int monsterCount;

    [InspectorName("BG Speed")]
    public float bgSpeedMultiplier = 1f; // ตัว x ความเร็วฉากหลังของเวฟนี้

    public float monsterSpeedMultiplier = 1f; // ตัว x ความเร็วมอนของเวฟนี้

    public float secPerMonster = 2f; // ความถี่เกิดมอน (วินาทีต่อมอน)

    public float delayBeforeNextWave = 2f; // ดีเลย์ก่อนเวฟถัดไป (วินาที)
}

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    #endregion

    #region Game Speed (Base)
    [Header("Background Speed")]
    [SerializeField] private float baseBackgroundSpeed = 5f;
    public float GameSpeed { get; private set; }

    [Header("Monster Speed")]
    [SerializeField] private float baseMonsterSpeed = 5f;
    public float MonsterSpeed { get; private set; }

    // ความเร็วพื้นฐานของ Wave ปัจจุบัน (ก่อนคูณ Rage)
    private float currentWaveBgSpeed;
    private float currentWaveMonsterSpeed;

    // สถานะจบเกมเพื่อสั่งหยุดทุกอย่าง
    private bool isGameHalted = false;
    #endregion

    #region Rage Integration (Effects on Speed)
    // ตัวคูณที่ได้รับมาจาก ScoreManager ตาม Rage Phase ปัจจุบัน
    private float rageBgMultiplier = 1f;
    private float rageMonsterMultiplier = 1f;

    // ฟังก์ชันนี้ถูกเรียกโดย ScoreManager เวลามีการเปลี่ยน Rage Phase
    public void UpdateRageMultipliers(float bgMultiplier, float monsterMultiplier)
    {
        rageBgMultiplier = bgMultiplier;
        rageMonsterMultiplier = monsterMultiplier;
        CalculateFinalSpeeds();
    }

    private void CalculateFinalSpeeds()
    {
        if (isGameHalted)
        {
            GameSpeed = 0f;
            MonsterSpeed = 0f;
        }
        else
        {
            // ความเร็วสุดท้าย = (ความเร็ว Base * ตัวคูณของ Wave) * ตัวคูณจาก Rage Phase
            GameSpeed = currentWaveBgSpeed * rageBgMultiplier;
            MonsterSpeed = currentWaveMonsterSpeed * rageMonsterMultiplier;
        }
    }
    #endregion

    #region Wave Configuration (Inspector Fields)
    [Header("Wave Configuration")]
    [SerializeField]
    private List<WaveData> waves = new List<WaveData>
    {
        new WaveData { waveName = "Wave 1", monsterCount = 10, bgSpeedMultiplier = 1f, monsterSpeedMultiplier = 1f, secPerMonster = 2f, delayBeforeNextWave = 3f },
    };

    [Header("Spawner References")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private Transform spawnPoint;

    private int currentWaveIndex = 0;
    private int monstersRemainingToSpawn = 0;
    private int activeMonstersInScene = 0;
    private bool isSpawning = false;
    #endregion

    #region Wave System (Logic)
    private void Start()
    {
        currentWaveBgSpeed = baseBackgroundSpeed;
        currentWaveMonsterSpeed = baseMonsterSpeed;
        CalculateFinalSpeeds();

        StartNextWave();
    }

    public void StartNextWave()
    {
        if (currentWaveIndex < waves.Count)
        {
            WaveData wave = waves[currentWaveIndex];

            // เก็บความเร็วของเวฟนั้นๆ เป็น Base
            currentWaveBgSpeed = baseBackgroundSpeed * wave.bgSpeedMultiplier;
            currentWaveMonsterSpeed = baseMonsterSpeed * wave.monsterSpeedMultiplier;
            CalculateFinalSpeeds(); // คำนวณร่วมกับระบบ Rage ทันที

            monstersRemainingToSpawn = wave.monsterCount;
            StartCoroutine(SpawnWaveRoutine(wave));
        }
    }

    private IEnumerator SpawnWaveRoutine(WaveData wave)
    {
        isSpawning = true;
        WaitForSeconds waitInterval = new WaitForSeconds(wave.secPerMonster);

        while (monstersRemainingToSpawn > 0)
        {
            yield return waitInterval;
            if (monsterPrefab != null && spawnPoint != null)
            {
                Instantiate(monsterPrefab, spawnPoint.position, Quaternion.identity);
                monstersRemainingToSpawn--;
                activeMonstersInScene++;
            }
        }
        isSpawning = false;
    }

    public void OnMonsterDespawnedOrKilled()
    {
        activeMonstersInScene--;

        if (!isSpawning && monstersRemainingToSpawn <= 0 && activeMonstersInScene <= 0)
        {
            float delay = waves[currentWaveIndex].delayBeforeNextWave;
            currentWaveIndex++;

            if (currentWaveIndex < waves.Count)
            {
                StartCoroutine(NextWaveAfterDelayRoutine(delay));
            }
            else
            {
                StartCoroutine(SecretCharacterAfterDelayRoutine(delay));
            }
        }
    }

    private IEnumerator NextWaveAfterDelayRoutine(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        StartNextWave();
    }
    #endregion

    #region Secret Character (Win Condition)
    [Header("Secret Character (Win Condition)")]
    [SerializeField] private GameObject secretCharacterPrefab;
    [SerializeField] private Transform secretSpawnPoint;

    private IEnumerator SecretCharacterAfterDelayRoutine(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        TriggerSecretCharacterSpawn();
    }

    private void TriggerSecretCharacterSpawn()
    {
        Debug.Log("[ALL WAVES CLEARED] Spawning Secret Character...");

        isGameHalted = true;     // หยุดระบบความเร็วทั้งหมด
        CalculateFinalSpeeds();  // บังคับให้สปีดกลายเป็น 0 ทันที

        if (secretCharacterPrefab != null && secretSpawnPoint != null)
        {
            GameObject secretChar = Instantiate(secretCharacterPrefab, secretSpawnPoint.position, Quaternion.identity);
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.StartWalkingToSecret(secretChar.transform);
            }
        }
    }
    #endregion

    #region Game Over / Game Win
    private readonly WaitForSeconds waitGameOver = new WaitForSeconds(1.5f);

    public void TriggerGameOver()
    {
        isGameHalted = true;    // หยุดผู้เล่นตาย
        CalculateFinalSpeeds();
        StartCoroutine(GameOverDelayRoutine());
    }

    private IEnumerator GameOverDelayRoutine()
    {
        yield return waitGameOver;
        Time.timeScale = 0f;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }
    }

    public void TriggerGameWin()
    {
        isGameHalted = true;    // หยุดเมื่อชนะเกม
        CalculateFinalSpeeds();
        Time.timeScale = 0f;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameWin();
        }
    }
    #endregion
}