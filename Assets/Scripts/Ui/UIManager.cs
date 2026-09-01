using System.Collections; 
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
    public TextMeshProUGUI feedbackText;

    [Header("Feedback Settings")]
    [Tooltip("ระยะเวลา (วินาที) ที่จะให้ข้อความ Hit / Miss ค้างอยู่บนจอ")]
    [SerializeField] private float feedbackDisplayTime = 1f;

    private Coroutine feedbackCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (feedbackText != null) feedbackText.text = "";
        UpdateScoreAndCombo(0, 0);
    }

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
        if (scoreText != null) scoreText.text = score.ToString();

        if (comboText != null)
        {
            if (combo > 0)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = combo.ToString();
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }

    // ฟังก์ชันโชว์คำว่า Hit / Miss แบบตั้งเวลาหายได้
    public void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null) return;

        feedbackText.text = message;
        feedbackText.color = color;

        // ถ้ามี Coroutine เดิมค้างอยู่ ให้หยุดอันเก่าก่อน เพื่อไม่ให้เวลาทับซ้อนกัน
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }

        // เริ่มนับเวลาถอยหลังเพื่อซ่อนข้อความ
        feedbackCoroutine = StartCoroutine(HideFeedbackRoutine());
    }

    private IEnumerator HideFeedbackRoutine()
    {
        yield return new WaitForSeconds(feedbackDisplayTime);
        if (feedbackText != null)
        {
            feedbackText.text = ""; // ลบข้อความออกเมื่อหมดเวลา
        }
    }
}