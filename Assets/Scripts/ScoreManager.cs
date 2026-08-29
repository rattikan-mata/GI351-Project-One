using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;

    public int CurrentScore { get; private set; }
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

    public void RegisterHit(int basePoints = 100)
    {
        CurrentCombo++;
        CurrentScore += basePoints + (CurrentCombo * 10);
        UpdateUI();
    }

    public void RegisterMiss()
    {
        CurrentCombo = 0;
        UpdateUI();
    }

    public void ResetAll()
    {
        CurrentScore = 0;
        CurrentCombo = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {CurrentScore}";
        if (comboText != null) comboText.text = $"Combo: {CurrentCombo}";
    }
}