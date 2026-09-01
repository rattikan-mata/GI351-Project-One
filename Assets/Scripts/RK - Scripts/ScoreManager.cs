using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int CurrentScore { get; private set; }
    public int CurrentCombo { get; private set; }
    public int TotalMisses { get; private set; }

    [Header("Feedback Colors")]
    [SerializeField] private Color hitColor = Color.green;
    [SerializeField] private Color missColor = Color.red;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ResetAll();
    }

    public void RegisterHit(int baseMonsterScore = 100)
    {
        CurrentCombo++;
        int gainedScore = baseMonsterScore + (CurrentCombo * 10);
        CurrentScore += gainedScore;
        Debug.Log($"COMBO = {CurrentCombo} | SCORE = {CurrentScore}");

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
        Debug.Log("MISS!");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScoreUI(CurrentScore, CurrentCombo);
            UIManager.Instance.ShowFeedback("Miss!", missColor);
        }
    }

    public void ResetAll()
    {
        CurrentScore = 0;
        CurrentCombo = 0;
        TotalMisses = 0;
    }
}