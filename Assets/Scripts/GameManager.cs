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
        StartCoroutine(SpawnMonsterRoutine());
    }

    private IEnumerator SpawnMonsterRoutine()
    {
        while (isGameRunning)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (monsterPrefab != null && spawnPoint != null)
            {
                Instantiate(monsterPrefab, spawnPoint.position, Quaternion.identity);
            }
        }
    }

    public void SetGameSpeed(float newSpeed)
    {
        gameSpeed = newSpeed;
    }
}