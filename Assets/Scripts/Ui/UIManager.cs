using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Hearts")]
    [SerializeField] private Image[] heartImages;

    [Header("UI Texts")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Feedback Settings")]
    [SerializeField] private float feedbackDisplayTime = 1f;
    private Coroutine feedbackCoroutine;
    private WaitForSeconds waitFeedback;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject creditPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameWinPanel;

    [Header("Result Score Texts")]
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameWinScoreText;

    private bool isPaused = false;
    private static readonly Color HeartDisabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        waitFeedback = new WaitForSeconds(feedbackDisplayTime);
    }

    private void Start()
    {
        if (feedbackText != null) feedbackText.text = "";
        UpdateScoreUI(0, 0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void UpdateHeartsUI(int currentHP)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            heartImages[i].color = (i < currentHP) ? Color.white : HeartDisabledColor;
        }
    }

    public void UpdateScoreUI(int score, int combo)
    {
        if (scoreText != null) scoreText.SetText("{0}", score);

        if (comboText != null)
        {
            if (combo > 0)
            {
                if (!comboText.gameObject.activeSelf) comboText.gameObject.SetActive(true);
                comboText.SetText("{0}", combo);
            }
            else
            {
                if (comboText.gameObject.activeSelf) comboText.gameObject.SetActive(false);
            }
        }
    }

    public void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null) return;

        feedbackText.text = message;
        feedbackText.color = color;

        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }

        feedbackCoroutine = StartCoroutine(HideFeedbackRoutine());
    }

    private IEnumerator HideFeedbackRoutine()
    {
        yield return waitFeedback;
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void OpenCredit()
    {
        if (creditPanel != null) creditPanel.SetActive(true);
    }

    public void CloseCredit()
    {
        if (creditPanel != null) creditPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("[GAME] Quit Game requested.");
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameOverScoreText != null && ScoreManager.Instance != null)
        {
            gameOverScoreText.SetText("Total Score: {0}", ScoreManager.Instance.CurrentScore);
        }
    }

    public void ShowGameWin()
    {
        if (gameWinPanel != null) gameWinPanel.SetActive(true);
        if (gameWinScoreText != null && ScoreManager.Instance != null)
        {
            gameWinScoreText.SetText("Total Score: {0}", ScoreManager.Instance.CurrentScore);
        }
    }
}