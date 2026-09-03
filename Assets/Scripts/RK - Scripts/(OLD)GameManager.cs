using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*[System.Serializable]
public class WaveData
{
    public string waveName;
    public int monsterCount;
    public float speedMultiplier = 1f;
    public float spawnInterval = 2f;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Speed")]
    [SerializeField] private float baseGameSpeed = 5f;
    public float GameSpeed { get; private set; }

    [Header("Wave Configuration")]
    [SerializeField]
    private List<WaveData> waves = new List<WaveData>
    {
        new WaveData { waveName = "Wave 1", monsterCount = 10, speedMultiplier = 1f, spawnInterval = 2f },
        new WaveData { waveName = "Wave 2", monsterCount = 15, speedMultiplier = 1.3f, spawnInterval = 1.6f }
    };

    [Header("Spawner References")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Secret Character (Win Condition)")]
    [SerializeField] private GameObject secretCharacterPrefab;
    [SerializeField] private Transform secretSpawnPoint;

    private int currentWaveIndex = 0;
    private int monstersRemainingToSpawn = 0;
    private int activeMonstersInScene = 0;
    private bool isSpawning = false;
    private readonly WaitForSeconds waitGameOver = new WaitForSeconds(1.5f);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GameSpeed = baseGameSpeed;
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (currentWaveIndex < waves.Count)
        {
            WaveData wave = waves[currentWaveIndex];
            GameSpeed = baseGameSpeed * wave.speedMultiplier;
            monstersRemainingToSpawn = wave.monsterCount;
            StartCoroutine(SpawnWaveRoutine(wave));
        }
    }

    private IEnumerator SpawnWaveRoutine(WaveData wave)
    {
        isSpawning = true;
        WaitForSeconds waitInterval = new WaitForSeconds(wave.spawnInterval);

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
            currentWaveIndex++;

            if (currentWaveIndex < waves.Count)
            {
                StartNextWave();
            }
            else
            {
                TriggerSecretCharacterSpawn();
            }
        }
    }

    private void TriggerSecretCharacterSpawn()
    {
        Debug.Log("[ALL WAVES CLEARED] Spawning Secret Character...");

        GameSpeed = 0f; //หยุดฉากหลังทันทีที่ประตูโผล่มา

        if (secretCharacterPrefab != null && secretSpawnPoint != null)
        {
            GameObject secretChar = Instantiate(secretCharacterPrefab, secretSpawnPoint.position, Quaternion.identity);
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.StartWalkingToSecret(secretChar.transform);
            }
        }
    }

    public void TriggerGameOver()
    {
        GameSpeed = 0f; // <--- หยุดฉากหลังทันทีที่ผู้เล่นตาย เพื่อให้ตอนตัวละครไม่เคลื่อนที่ไม่แปลกตา
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
        GameSpeed = 0f; // หยุดฉากหลังทันทีที่ชนะเกม เพื่อให้ตัวละครวิ่งไปหาประตู
        Time.timeScale = 0f;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameWin();
        }
    }
}*/