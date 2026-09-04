using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveData
{
    public string waveName;
    public int monsterCount;

    [InspectorName("BG Speed")]
    public float bgSpeedMultiplier = 1f;// ตัว x ความเร็วฉากหลัง

    public float monsterSpeedMultiplier = 1f; //ตัว x ความเร็วมอน 

    public float secPerMonster = 2f; //ความถี่เกิดมอน (วินาทีต่อมอน)

    public float delayBeforeNextWave = 2f; // ดีเลย์ก่อนเวฟถัดไป (วินาที) หลังจาก spawn มอนครบแล้ว
}

public class GameManager : MonoBehaviour
{
    #region Singleton
    // ทำให้ GameManager มีตัวเดียวในซีน

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    #endregion

    #region Game Speed
    // แยกความเร็วออกเป็น 2 ส่วน
    // - GameSpeed (BG)  : ใช้คุมความเร็วฉากหลัง ) -> อ่านโดย Background.cs
    // - MonsterSpeed    : ใช้คุมความเร็วมอน-> อ่านโดย Monster.cs

    [Header("Background Speed")]
    [SerializeField] private float baseBackgroundSpeed = 5f;
    public float GameSpeed { get; private set; }

    [Header("Monster Speed")]
    [SerializeField] private float baseMonsterSpeed = 5f;
    public float MonsterSpeed { get; private set; }

    #endregion

    #region Wave Configuration (Inspector Fields)
    // ตั้งค่าเวฟทั้งหมดตรงนี้ผ่าน Inspector: จำนวนเวฟ, มอนต่อเวฟ, ความเร็ว, ความถี่เกิด, ดีเลย์ก่อนเวฟถัดไป

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
    // ตัวควบคุมการเล่นเวฟจริงๆ: เริ่มเวฟ, ทยอย spawn มอนตามความถี่ที่ตั้ง,
    // เช็คว่าเวฟนี้เคลียร์หมดหรือยัง แล้วหน่วงเวลาก่อนขึ้นเวฟถัดไปอัตโนมัติ

    private void Start()
    {
        GameSpeed = baseBackgroundSpeed;
        MonsterSpeed = baseMonsterSpeed;
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (currentWaveIndex < waves.Count)
        {
            WaveData wave = waves[currentWaveIndex];
            GameSpeed = baseBackgroundSpeed * wave.bgSpeedMultiplier;
            MonsterSpeed = baseMonsterSpeed * wave.monsterSpeedMultiplier;
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
            // ใช้ delayBeforeNextWave ของเวฟที่เพิ่งจบ เป็นตัวหน่วงก่อนขึ้นเวฟถัดไป
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
    // ทำงานตอนเคลียร์เวฟสุดท้ายครบแล้ว: หยุด GameSpeed/MonsterSpeed, spawn ตัวละครลับขึ้นมา
    // แล้วสั่งให้ PlayerController เดินเข้าไปหา (เงื่อนไขชนะเกม)

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

        GameSpeed = 0f;     // หยุดฉากหลังทันทีที่ประตูโผล่มา
        MonsterSpeed = 0f;  // หยุดมอนที่เหลือ (ถ้ามี) ด้วยเช่นกัน

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
    // เรียกจากภายนอก (เช่น PlayerController ตอนโดนมอนชน) เพื่อจบเกมแบบแพ้หรือชนะ
    // หยุด GameSpeed/MonsterSpeed ทันที แล้วรอสักครู่/หยุดเวลาเกม ก่อนเปิด UI ผลลัพธ์ผ่าน UIManager

    private readonly WaitForSeconds waitGameOver = new WaitForSeconds(1.5f);

    public void TriggerGameOver()
    {
        GameSpeed = 0f;     // <--- หยุดฉากหลังทันทีที่ผู้เล่นตาย
        MonsterSpeed = 0f;  // หยุดมอน
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
        GameSpeed = 0f;     // หยุดฉากหลังทันทีที่ชนะเกม 
        MonsterSpeed = 0f;  // หยุดมอน
        Time.timeScale = 0f;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameWin();
        }
    }

    #endregion
}