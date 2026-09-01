using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Texts")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("UI Hearts")]
    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (feedbackText != null) feedbackText.text = "";
    }

    public void UpdateScoreAndCombo(int score, int combo)
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
        if (comboText != null) comboText.text = "Combo: " + combo;
    }

    public void ShowFeedback(string message)
    {
        if (feedbackText != null) feedbackText.text = message;
    }

    public void UpdateHearts(int currentHP)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < currentHP)
            {
                heartImages[i].sprite = fullHeartSprite;
                heartImages[i].color = Color.white; // สีปกติ
            }
            else
            {
                if (emptyHeartSprite != null)
                {
                    heartImages[i].sprite = emptyHeartSprite;
                    heartImages[i].color = Color.white;
                }
                else
                {
                    // ถ้ายังไม่มีรูปหัวใจเปล่า ให้ปรับสีหัวใจเดิมให้มืดและโปร่งใสแทน
                    heartImages[i].color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                }
            }
        }
    }
}