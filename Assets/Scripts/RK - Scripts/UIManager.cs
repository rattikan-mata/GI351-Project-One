using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("MainMenu")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;

    [Header("CutScene")]
    [SerializeField] private bool isCutSceneScene = false;

    [Header("Credit")]
    [SerializeField] private GameObject creditPanel;

    [Header("GamePlay")]
    [SerializeField] private GameObject gameplayHUD;
    [SerializeField] private GameObject hitCircleSprite;
    [SerializeField] private GameObject comboContainer;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Feedback Settings")]
    [SerializeField] private float feedbackDisplayTime = 1f;
    private Coroutine feedbackCoroutine;
    private WaitForSeconds waitFeedback;

    [Header("[ Array Sprite Pills ]")]
    [SerializeField] private Image[] pillImages = new Image[3];
    private static readonly Color PillDisabledColor = new Color(0.2f, 0.2f, 0.2f, 0.25f);

    [Header("[ Array Animation Hearts ]")]
    [SerializeField] private GameObject[] pulseAnimPhases = new GameObject[3];

    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Image pauseDimImage;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseRestartButton;
    [SerializeField] private Button pauseMainMenuButton;

    [Header("EndGame")]
    [SerializeField] private GameObject gameWinPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI maxComboText;
    [SerializeField] private TextMeshProUGUI totalMissText;
    [SerializeField] private Button endRestartButton;
    [SerializeField] private Button endMainMenuButton;

    [Header("Minimap")]
    [SerializeField] private Slider minimapSlider;

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
        if (isCutSceneScene)
        {
            Time.timeScale = 1f;
            return;
        }

        if (gameplayHUD != null)
        {
            Time.timeScale = 1f;
            UpdatePillsUI(3);
            UpdateScoreUI(0, 0);
            UpdatePulsePhase(0);
            UpdateMinimapProgress(0f);
            if (feedbackText != null) feedbackText.text = "";
        }
    }

    private void Update()
    {
        if (isCutSceneScene && Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
            return;
        }

        if (gameplayHUD != null && Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    #region Main Menu & CutScene
    public void OnStartButtonClicked()
    {
        AudioManager.Instance?.PlayMenuStart();
        SceneManager.LoadScene(1);
    }

    public void OnExitButtonClicked()
    {
        AudioManager.Instance?.PlayUIClick();
        Debug.Log("Exit Game");
        Application.Quit();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(2);
    }

    public void OpenCredit()
    {
        if (creditPanel != null) creditPanel.SetActive(true);
    }

    public void CloseCredit()
    {
        if (creditPanel != null) creditPanel.SetActive(false);
    }
    #endregion

    #region In-Game HUD Updates
    public void UpdatePillsUI(int currentPills)
    {
        for (int i = 0; i < pillImages.Length; i++)
        {
            if (pillImages[i] != null)
            {
                pillImages[i].color = (i < currentPills) ? Color.white : PillDisabledColor;
            }
        }
    }

    public void UpdateScoreUI(int score, int combo)
    {
        if (scoreText != null) scoreText.SetText("{0}", score);

        if (comboContainer != null)
        {
            comboContainer.SetActive(combo > 0);
        }

        if (comboText != null && combo > 0)
        {
            comboText.SetText("{0}", combo);
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

    public void UpdatePulsePhase(int phase)
    {
        for (int i = 0; i < pulseAnimPhases.Length; i++)
        {
            if (pulseAnimPhases[i] != null)
            {
                pulseAnimPhases[i].SetActive(i == (phase - 1));
            }
        }
    }

    public void UpdateMinimapProgress(float progress)
    {
        if (minimapSlider != null)
        {
            minimapSlider.value = Mathf.Clamp01(progress);
        }
    }
    #endregion

    #region Pause System
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        AudioManager.Instance?.PlayUIClick();
        if (pauseDimImage != null) pauseDimImage.gameObject.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioManager.Instance?.PlayUIClick();
        if (pauseDimImage != null) pauseDimImage.gameObject.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void RestartGame()
    {
        AudioManager.Instance?.PlayUIClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        AudioManager.Instance?.PlayUIClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
    #endregion

    #region EndGame System
    public void ShowGameWin()
    {
        Time.timeScale = 0f;
        if (gameWinPanel != null) gameWinPanel.SetActive(true);
        DisplayEndGameStats();
    }

    public void ShowGameOver()
    {
        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        DisplayEndGameStats();
    }

    private void DisplayEndGameStats()
    {
        if (ScoreManager.Instance != null)
        {
            if (totalScoreText != null) totalScoreText.SetText("Score: {0}", ScoreManager.Instance.CurrentScore);
            if (maxComboText != null) maxComboText.SetText("Max Combo: {0}", ScoreManager.Instance.MaxCombo);
            if (totalMissText != null) totalMissText.SetText("Total Miss: {0}", ScoreManager.Instance.TotalMisses);
        }
    }
    #endregion
}