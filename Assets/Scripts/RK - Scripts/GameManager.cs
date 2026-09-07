using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveData
{
    public string waveName;
    public int monsterCount;

    [InspectorName("BG Speed Multiplier")]
    public float bgSpeedMultiplier = 1f;

    [InspectorName("Monster Speed Multiplier")]
    public float monsterSpeedMultiplier = 1f;

    public float secPerMonster = 2f;
    public float delayBeforeNextWave = 2f;

    [Header("Elite Monster Config")]
    public bool spawnEliteMonsterAtEnd = false;
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

    private float currentWaveBgMult = 1f;
    private float currentWaveMonsterMult = 1f;

    private bool isGameHalted = false;
    #endregion

    #region Rage Integration (Additive Speed)
    private float rageBgBonus = 0f;
    private float rageMonsterBonus = 0f;

    public void UpdateRageBonus(float bgBonus, float monsterBonus)
    {
        rageBgBonus = bgBonus;
        rageMonsterBonus = monsterBonus;
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
            float finalBgMult = currentWaveBgMult + rageBgBonus;
            float finalMonsterMult = currentWaveMonsterMult + rageMonsterBonus;

            GameSpeed = baseBackgroundSpeed * finalBgMult;
            MonsterSpeed = baseMonsterSpeed * finalMonsterMult;
        }
    }
    #endregion

    #region Wave Configuration
    [Header("Wave Configuration")]
    [SerializeField]
    private List<WaveData> waves = new List<WaveData>
    {
        new WaveData { waveName = "Wave 1", monsterCount = 10, bgSpeedMultiplier = 1f, monsterSpeedMultiplier = 1f, secPerMonster = 2f, delayBeforeNextWave = 3f },
    };

    [Header("Spawner References")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private GameObject eliteMonsterPrefab;
    [SerializeField] private Transform spawnPoint;

    private int currentWaveIndex = 0;
    private int monstersRemainingToSpawn = 0;
    private int activeMonstersInScene = 0;
    private bool isSpawning = false;
    #endregion

    #region Wave System (Logic)
    private void Start()
    {
        currentWaveBgMult = 1f;
        currentWaveMonsterMult = 1f;
        CalculateFinalSpeeds();

        // ส่ง List ของ Waves ทั้งหมดให้ UIManager ไปสร้างไอคอนบน Minimap ให้พอดีกับจำนวนเวฟ
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SpawnMinimapFlags(waves);
        }

        StartNextWave();
    }

    public void StartNextWave()
    {
        if (currentWaveIndex < waves.Count)
        {
            WaveData wave = waves[currentWaveIndex];

            currentWaveBgMult = wave.bgSpeedMultiplier;
            currentWaveMonsterMult = wave.monsterSpeedMultiplier;

            CalculateFinalSpeeds();

            if (UIManager.Instance != null)
            {
                // เลื่อนหลอด Slider ตามจำนวนเวฟที่มีจริงๆ (+1 เผื่อจุดประตูตอนจบ)
                UIManager.Instance.UpdateMinimapByWave(currentWaveIndex, waves.Count + 1);
            }

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

        if (wave.spawnEliteMonsterAtEnd && eliteMonsterPrefab != null && spawnPoint != null)
        {
            yield return waitInterval;
            Instantiate(eliteMonsterPrefab, spawnPoint.position, Quaternion.identity);
            activeMonstersInScene++;
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
        Debug.Log("[ALL WAVES CLEARED] Spawning Secret Character/Door...");

        isGameHalted = true;
        CalculateFinalSpeeds();

        if (UIManager.Instance != null)
        {
            // ดันหลอด Minimap ไปที่จุดสุดท้าย (จุดที่เท่ากับ waves.Count)
            UIManager.Instance.UpdateMinimapByWave(waves.Count, waves.Count + 1);
        }

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
        isGameHalted = true;
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
        isGameHalted = true;
        CalculateFinalSpeeds();
        Time.timeScale = 0f;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameWin();
        }
    }
    #endregion
}