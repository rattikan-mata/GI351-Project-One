using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private float gameSpeed = 5f;
    public float GameSpeed => gameSpeed;

    [Header("Spawner Settings")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 2f;

    [Header("Wave Settings")]
    [SerializeField] private int monstersPerWave = 10;
    [SerializeField] private int totalWavesToWin = 2;
    [SerializeField] private float delayBetweenWaves = 3f;
    
    [Header("Win Settings")]
    [SerializeField] private GameObject winPanel;

    public int CurrentWave { get; private set; }
    private bool isGameRunning = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        while (isGameRunning && CurrentWave < totalWavesToWin)
        {
            CurrentWave++;

            for (int i = 0; i < monstersPerWave; i++)
            {
                if (monsterPrefab != null && spawnPoint != null)
                {
                    Instantiate(monsterPrefab, spawnPoint.position, Quaternion.identity);
                }
                yield return new WaitForSeconds(spawnInterval);
            }

            if (CurrentWave < totalWavesToWin)
            {
                yield return new WaitForSeconds(delayBetweenWaves);
            }
        }

        WinGame();
    }

    private void WinGame()
    {
        isGameRunning = false;
        Debug.Log("YOU WIN!");

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }
   
    public void SetGameSpeed(float newSpeed)
    {
        gameSpeed = newSpeed;
    }
}