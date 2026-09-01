using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score { get; private set; }
    public int CurrentCombo { get; private set; }

    [Header("Feedback Colors")]
    [SerializeField] private Color hitColor = Color.green;
    [SerializeField] private Color missColor = Color.red;

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

        // UI hit และ สี
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScoreAndCombo(Score, CurrentCombo);
            UIManager.Instance.ShowFeedback("Hit!", hitColor); // เปลี่ยนมาใช้สีจาก Inspector
        }
    }

    public void RegisterMiss()
    {
        CurrentCombo = 0;
        Debug.Log("MISS!");

        // UI miss และ สี
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScoreAndCombo(Score, CurrentCombo);
            UIManager.Instance.ShowFeedback("Miss!", missColor); // เปลี่ยนมาใช้สีจาก Inspector
        }
    }

    public void ResetAll()
    {
        CurrentCombo = 0;
    }
}