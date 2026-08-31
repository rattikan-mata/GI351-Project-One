using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score { get; private set; }
    public int CurrentCombo { get; private set; }

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
        ResetAll();
    }

    public void RegisterHit()
    {
        Score += 100;
        CurrentCombo++;
        Debug.Log("COMBO = " + CurrentCombo + " | SCORE = " + Score);
    }

    public void RegisterMiss()
    {
        CurrentCombo = 0;
        Debug.Log("MISS!");
    }

    public void ResetAll()
    {
        CurrentCombo = 0;
    }
}