using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("MainMenu")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;

    [Header("CutScene")]
    [SerializeField] private bool isCutSceneScene = false;
    [SerializeField] private GameObject pressSpacePromptImage;

    [Header("Credit")]
    [SerializeField] private GameObject creditPanel;

    [Header("GamePlay")]

    [SerializeField] private GameObject gameplayHUD;
    [SerializeField] private GameObject hitCircleSprite;
    [SerializeField] private GameObject comboContainer;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;


    [SerializeField] private TextMeshProUGUI hitFeedbackText;
    [SerializeField] private TextMeshProUGUI missFeedbackText;


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

    [Header("MiniMap")]
    [SerializeField] private Image spriteMinimap;
    [SerializeField] private RectTransform flagContainer;
    [SerializeField] private Slider minimapSlider;

    [Header("MiniMap Icons")]
    [SerializeField] private GameObject normalWaveIconPrefab; // ไอคอนเวฟปกติ
    [SerializeField] private GameObject eliteWaveIconPrefab;  // ไอคอนเวฟที่มี Elite
    [SerializeField] private GameObject doorIconPrefab;       // ไอคอนประตูตอนจบ

    private bool isPaused = false;

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

            if (pressSpacePromptImage != null)
            {
                pressSpacePromptImage.SetActive(true);
                StartCoroutine(BlinkPromptRoutine());
            }
            return;
        }

        if (gameplayHUD != null)
        {
            Time.timeScale = 1f;
            UpdatePillsUI(3);
            UpdateScoreUI(0, 0);
            UpdatePulsePhase(0);


            if (hitFeedbackText != null) hitFeedbackText.text = "";
            if (missFeedbackText != null) missFeedbackText.text = "";

            UpdateMinimapProgress(0f);

            AudioManager.Instance?.PlayBGMInGame(); // เข้าโหมดเล่นจริงแล้ว สลับเพลงเป็น InGame
        }
        else
        {
            // ไม่ใช่ทั้ง CutScene และ Gameplay -> แปลว่าเป็นซีน Main Menu
            AudioManager.Instance?.PlayBGMMainMenu();
        }
    }

    private void Update()
    {
        if (isCutSceneScene && Input.GetKeyDown(KeyCode.Space))
        {
            AudioManager.Instance?.PlayUIClick();
            StartGame();
            return;
        }

        if (gameplayHUD != null && Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    private IEnumerator BlinkPromptRoutine()
    {
        if (pressSpacePromptImage == null) yield break;

        CanvasGroup canvasGroup = pressSpacePromptImage.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = pressSpacePromptImage.AddComponent<CanvasGroup>();
        }

        while (true)
        {
            float elapsed = 0f;
            while (elapsed < 0.6f)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0.2f, elapsed / 0.6f);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < 0.6f)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0.2f, 1f, elapsed / 0.6f);
                yield return null;
            }
        }
    }

    #region Minimap Flag Spawner & Slider Logic
    public void SpawnMinimapFlags(List<WaveData> waves)
    {
        if (flagContainer == null) return;

        foreach (Transform child in flagContainer)
        {
            Destroy(child.gameObject);
        }

        int totalPoints = waves.Count + 1; // จำนวนเวฟ + ประตูทางออก 1 บาน
        float width = flagContainer.rect.width;

        for (int i = 0; i < totalPoints; i++)
        {
            GameObject prefabToSpawn = normalWaveIconPrefab;

            if (i < waves.Count)
            {
                if (waves[i].spawnEliteMonsterAtEnd)
                {
                    prefabToSpawn = eliteWaveIconPrefab;
                }
            }
            else
            {
                prefabToSpawn = doorIconPrefab;
            }

            if (prefabToSpawn != null)
            {
                GameObject flagObj = Instantiate(prefabToSpawn, flagContainer);
                RectTransform rect = flagObj.GetComponent<RectTransform>();

                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0f, 0.5f);
                    rect.anchorMax = new Vector2(0f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);

                    float t = (float)i / (totalPoints - 1);
                    rect.anchoredPosition = new Vector2(t * width, 0f);
                }
            }
        }
    }

    public void UpdateMinimapByWave(int waveIndex, int totalWavesAndSecret)
    {
        float progress = (float)waveIndex / (totalWavesAndSecret - 1);
        UpdateMinimapProgress(progress);
    }

    public void UpdateMinimapProgress(float progress)
    {
        if (minimapSlider != null)
        {
            minimapSlider.value = Mathf.Clamp01(progress);
        }
    }
    #endregion

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

    public void HideHitZone()
    {
        if (hitCircleSprite != null)
        {
            hitCircleSprite.SetActive(false);
        }
    }

    public void ShowFeedback(string message, Color color)
    {

        bool isHit = message.Contains("Hit");

        if (isHit)
        {
            if (hitFeedbackText != null) hitFeedbackText.text = message;
            if (missFeedbackText != null) missFeedbackText.text = ""; // ปิดฝั่ง Miss
        }
        else
        {
            if (missFeedbackText != null) missFeedbackText.text = message;
            if (hitFeedbackText != null) hitFeedbackText.text = ""; // ปิดฝั่ง Hit
        }

        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }

        feedbackCoroutine = StartCoroutine(HideFeedbackRoutine());
    }

    private IEnumerator HideFeedbackRoutine()
    {
        yield return waitFeedback;


        if (hitFeedbackText != null) hitFeedbackText.text = "";
        if (missFeedbackText != null) missFeedbackText.text = "";
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
        AudioManager.Instance?.StopRageStage3Loop();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        AudioManager.Instance?.PlayUIClick();
        AudioManager.Instance?.StopRageStage3Loop();
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

            if (totalScoreText != null) totalScoreText.SetText("{0}", ScoreManager.Instance.CurrentScore);
            if (maxComboText != null) maxComboText.SetText("{0}", ScoreManager.Instance.MaxCombo);
            if (totalMissText != null) totalMissText.SetText("{0}", ScoreManager.Instance.TotalMisses);
        }
    }

    public void GoToNextStage()
    {
        AudioManager.Instance?.PlayUIClick();
        Time.timeScale = 1f;


        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextSceneIndex);
    }
    #endregion
}