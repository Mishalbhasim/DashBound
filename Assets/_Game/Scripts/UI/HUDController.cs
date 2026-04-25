using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Score & Fruits")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI fruitCountText;

    [Header("Health")]
    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartEmpty;

    [Header("Lives")]
    [SerializeField] private TextMeshProUGUI livesText;

    [Header("Power-Up")]
    [SerializeField] private Image powerUpIcon;
    [SerializeField] private GameObject powerUpPanel;

    [Header("Boss Bar")]
    [SerializeField] private GameObject bossHealthPanel;
    [SerializeField] private Image bossHealthFill;
    [SerializeField] private TextMeshProUGUI bossNameText;

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverFruitsText;

    [Header("Level Complete Panel")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TextMeshProUGUI levelCompleteScoreText;
    [SerializeField] private TextMeshProUGUI levelCompleteFruitsText;

    
    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (bossHealthPanel != null) bossHealthPanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            UpdateLives(GameManager.Instance.Lives);
            UpdateScore(0);
        }

        
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    
    private void OnGameStateChanged(GameState state)
    {
        if (state == GameState.GameOver)
        {
            
            if (gameOverPanel != null && !gameOverPanel.activeSelf)
            {
                int score = GameManager.Instance != null ? GameManager.Instance.LevelScore : 0;
                int fruits = GameManager.Instance != null ? GameManager.Instance.LevelFruits : 0;
                ShowGameOver(score, fruits);
            }
        }

        if (state == GameState.LevelComplete)
        {
            if (levelCompletePanel != null && !levelCompletePanel.activeSelf)
            {
                int score = GameManager.Instance != null ? GameManager.Instance.LevelScore : 0;
                int fruits = GameManager.Instance != null ? GameManager.Instance.LevelFruits : 0;
                ShowLevelComplete(score, fruits);
            }
        }
    }

    
    private void OnEnable()
    {
        UIEvents.OnScoreChanged += UpdateScore;
        UIEvents.OnFruitsChanged += UpdateFruits;
        UIEvents.OnLivesChanged += UpdateLives;
        UIEvents.OnHealthChanged += UpdateHealth;
        UIEvents.OnTimerChanged += UpdateTimer;
       
        UIEvents.OnLevelComplete += ShowLevelComplete;
        UIEvents.OnGameOver += ShowGameOver;

        BossBase.OnBossHealthChanged += UpdateBossHealth;
    }

    private void OnDisable()
    {
        UIEvents.OnScoreChanged -= UpdateScore;
        UIEvents.OnFruitsChanged -= UpdateFruits;
        UIEvents.OnLivesChanged -= UpdateLives;
        UIEvents.OnHealthChanged -= UpdateHealth;
        UIEvents.OnTimerChanged -= UpdateTimer;
        
        UIEvents.OnLevelComplete -= ShowLevelComplete;
        UIEvents.OnGameOver -= ShowGameOver;

        BossBase.OnBossHealthChanged -= UpdateBossHealth;
    }

   
    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"{score:000000}";
    }

    private void UpdateFruits(int count)
    {
        if (fruitCountText != null)
            fruitCountText.text = $"x{count}";
    }

    private void UpdateLives(int lives)
    {
        if (livesText != null)
            livesText.text = $"x{lives}";
    }

    private void UpdateHealth(float pct)
    {
        if (heartImages == null || heartImages.Length == 0) return;

        int fullHearts = Mathf.RoundToInt(pct * heartImages.Length);
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
                heartImages[i].sprite = i < fullHearts ? heartFull : heartEmpty;
        }
    }

    private void UpdateTimer(float timeRemaining)
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void UpdateBossHealth(float pct)
    {
        if (bossHealthPanel != null) bossHealthPanel.SetActive(true);
        if (bossHealthFill != null) bossHealthFill.fillAmount = pct;
    }

    
    //receives level score + fruits from event
    private void ShowGameOver(int score, int fruits)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverScoreText != null)
            gameOverScoreText.text = $"Score\n{score:000000}";

        if (gameOverFruitsText != null)
            gameOverFruitsText.text = $"Fruits\n{fruits}";
    }

    private void ShowLevelComplete(int score, int fruits)
    {
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);

        if (levelCompleteScoreText != null)
            levelCompleteScoreText.text = $"Score\n{score:000000}";

        if (levelCompleteFruitsText != null)
            levelCompleteFruitsText.text = $"Fruits\n{fruits}";
    }
}