using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    #region Variables
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject cutScenePanel;
    [SerializeField] private GameObject gameplayHUD;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameWinPanel;

    [Header("Pause Background")]
    [SerializeField] private Image pauseDimImage;

    [Header("Pills")]
    [SerializeField] private Image[] pillImages;
    private static readonly Color PillDisabledColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);

    [Header("In-Game Texts")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private float feedbackDuration = 0.8f;
    private Coroutine feedbackCoroutine;

    [Header("Minimap Slider")]
    [SerializeField] private Slider minimapSlider;

    [Header("End Game Texts")]
    [SerializeField] private TextMeshProUGUI winScoreText;
    [SerializeField] private TextMeshProUGUI winMaxComboText;
    [SerializeField] private TextMeshProUGUI winMissText;

    [SerializeField] private TextMeshProUGUI loseScoreText;
    [SerializeField] private TextMeshProUGUI loseMaxComboText;
    [SerializeField] private TextMeshProUGUI loseMissText;

    private bool isPaused = false;
    private bool isInCutScene = false;
    #endregion

    #region Initialization
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (mainMenuPanel != null && mainMenuPanel.activeSelf)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }

        UpdatePillsUI(3);
        UpdateScoreUI(0, 0);
        UpdateMinimapProgress(0f);
    }

    private void Update()
    {
        if (isInCutScene && Input.GetKeyDown(KeyCode.Space))
        {
            EndCutScene();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }
    #endregion

    #region Main Menu & CutScene
    public void OnStartButtonClicked()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        StartCutScene();
    }

    public void OnExitButtonClicked()
    {
        Debug.Log("Exit Game");
        Application.Quit();
    }

    public void StartCutScene()
    {
        isInCutScene = true;
        Time.timeScale = 0f;
        if (cutScenePanel != null) cutScenePanel.SetActive(true);
    }

    public void EndCutScene()
    {
        isInCutScene = false;
        Time.timeScale = 1f;
        if (cutScenePanel != null) cutScenePanel.SetActive(false);
        if (gameplayHUD != null) gameplayHUD.SetActive(true);
    }
    #endregion

    #region Pause Game
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseDimImage != null) pauseDimImage.gameObject.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseDimImage != null) pauseDimImage.gameObject.SetActive(false);
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
    #endregion

    #region HUD Updates (Pills, Score, Minimap, Feedback)
    public void UpdatePillsUI(int currentPills)
    {
        for (int i = 0; i < pillImages.Length; i++)
        {
            pillImages[i].color = (i < currentPills) ? Color.white : PillDisabledColor;
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
                comboText.SetText("COMBO {0}", combo);
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

        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(HideFeedbackRoutine());
    }

    private IEnumerator HideFeedbackRoutine()
    {
        yield return new WaitForSecondsRealtime(feedbackDuration);
        if (feedbackText != null) feedbackText.text = "";
    }

    public void UpdateMinimapProgress(float progress)
    {
        if (minimapSlider != null)
        {
            minimapSlider.value = Mathf.Clamp01(progress);
        }
    }
    #endregion

    #region End Game Panels
    public void ShowGameWin()
    {
        if (gameWinPanel != null) gameWinPanel.SetActive(true);

        if (ScoreManager.Instance != null)
        {
            if (winScoreText != null) winScoreText.SetText("Score: {0}", ScoreManager.Instance.CurrentScore);
            if (winMaxComboText != null) winMaxComboText.SetText("Max Combo: {0}", ScoreManager.Instance.MaxCombo);
            if (winMissText != null) winMissText.SetText("Total Miss: {0}", ScoreManager.Instance.TotalMisses);
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (ScoreManager.Instance != null)
        {
            if (loseScoreText != null) loseScoreText.SetText("Score: {0}", ScoreManager.Instance.CurrentScore);
            if (loseMaxComboText != null) loseMaxComboText.SetText("Max Combo: {0}", ScoreManager.Instance.MaxCombo);
            if (loseMissText != null) loseMissText.SetText("Total Miss: {0}", ScoreManager.Instance.TotalMisses);
        }
    }
    #endregion
}