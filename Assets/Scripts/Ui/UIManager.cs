using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Hearts")]
    public Image[] heartImages;

    [Header("UI Texts")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI feedbackText; // สำหรับโชว์คำว่า Hit! หรือ Miss!

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // เคลียร์ข้อความตอนเริ่มเกม
        if (feedbackText != null) feedbackText.text = "";
        UpdateScoreAndCombo(0, 0);
    }

    // ฟังก์ชันอัปเดตหัวใจ 
    public void UpdateHearts(int currentHP)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < currentHP)
                heartImages[i].color = Color.white;
            else
                heartImages[i].color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        }
    }

    
    public void UpdateScoreAndCombo(int score, int combo)
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
        if (comboText != null) comboText.text = "Combo: " + combo;
    }

    public void ShowFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color; // เปลี่ยนสีข้อความได้ด้วย
        }
    }
}