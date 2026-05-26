
using UnityEngine;
using System;

public enum GameState { MainMenu, Playing, Paused, LevelComplete, GameOver, Loading }

public class GameManager : Singleton<GameManager>
{
    // State
    public GameState CurrentState { get; private set; }
    public static event Action<GameState> OnGameStateChanged;

    // Progress
    public int CurrentLevel { get; private set; } = 1;
    public int TotalScore { get; private set; } = 0;
    public int HighScore { get; private set; } = 0;
    public int Lives { get; private set; } = 3;
    public int FruitsCollected { get; private set; } = 0;
    public int LevelScore { get; private set; } = 0;
    public int LevelFruits { get; private set; } = 0;

    [Header("Level Data")]
    [SerializeField] private LevelData[] allLevels;

    [Header("Settings")]
    [SerializeField] private int startingLives = 3;

    // Timer
    private float currentTime = 0f;
    private bool timerRunning = false;

    // Save data reference
    private GameSaveData saveData;

    
    protected override void Awake()
    {
        base.Awake();
        LoadProgress();
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing || !timerRunning) return;

        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(currentTime, 0f);
        UIEvents.OnTimerChanged?.Invoke(currentTime);

        if (currentTime <= 0f)
        {
            timerRunning = false;
            TriggerGameOver();
        }
    }

    //STATE MACHINE
    public void SetGameState(GameState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                AudioManager.Instance?.PlaySFX("GameOver");
                UIEvents.OnGameOver?.Invoke(LevelScore, LevelFruits);
                LevelManager.Instance?.ClearCheckpoint();
                // Save high score if beaten
                if (LevelScore > HighScore)
                {
                    HighScore = LevelScore;
                    saveData.highScore = HighScore;
                    SaveSystem.Save(saveData);
                }
                SaveSystem.SubmitScore(TotalScore);
                break;

            case GameState.LevelComplete:
                Time.timeScale = 0f;
                AudioManager.Instance?.PlaySFX("LevelComplete");
                UIEvents.OnLevelComplete?.Invoke(LevelScore, LevelFruits);
                break;

            case GameState.Loading:
                Time.timeScale = 1f;
                break;

            case GameState.MainMenu:
                Time.timeScale = 1f;
                break;
        }

        OnGameStateChanged?.Invoke(newState);
    }

    //TIMER
    public void StartLevelTimer(float timeLimit)
    {
        if (timeLimit <= 0f) { timerRunning = false; return; }
        currentTime = timeLimit;
        timerRunning = true;
        UIEvents.OnTimerChanged?.Invoke(currentTime);
    }

    public void StopTimer() => timerRunning = false;
    public float GetCurrentTime() => currentTime;

    //SCORE
    public void AddScore(int points)
    {
        if (points <= 0) return;
        TotalScore += points;
        LevelScore += points;
        UIEvents.OnScoreChanged?.Invoke(LevelScore);
    }

    //FRUITS
    public void CollectFruit()
    {
        FruitsCollected++;
        LevelFruits++;
        AddScore(100);
        UIEvents.OnFruitsChanged?.Invoke(FruitsCollected);
    }

    public void ResetFruits()
    {
        FruitsCollected = 0;
        LevelScore = 0;
        LevelFruits = 0;
        UIEvents.OnFruitsChanged?.Invoke(0);
        UIEvents.OnScoreChanged?.Invoke(0);
    }

    //LIVES
    public void LoseLife()
    {
        Lives--;
        UIEvents.OnLivesChanged?.Invoke(Lives);
        if (Lives <= 0)
            SetGameState(GameState.GameOver);
        else
            LevelManager.Instance?.RestartLevel();
    }

    public void TriggerGameOver()
    {
        StopTimer();
        if (CurrentState == GameState.GameOver)
            CurrentState = GameState.Playing;
        SetGameState(GameState.GameOver);
    }

    public void GainLife()
    {
        Lives = Mathf.Min(Lives + 1, 99);
        UIEvents.OnLivesChanged?.Invoke(Lives);
    }

    //LEVEL PROGRESSION
    public void LevelComplete()
    {
        StopTimer();
        SaveProgress();
        SetGameState(GameState.LevelComplete);
    }

    public LevelData GetLevelData(int index)
    {
        if (index < 1 || index > allLevels.Length) return null;
        return allLevels[index - 1];
    }

    public bool IsLevelUnlocked(int levelIndex)
        => SaveSystem.IsLevelUnlocked(levelIndex);

    //SAVE / LOAD
    private void SaveProgress()
    {
        saveData.totalScore = TotalScore;
        saveData.lives = Lives;
        saveData.highScore = Mathf.Max(saveData.highScore, LevelScore);

        // Unlock next level
        int nextLevel = LevelManager.Instance != null
            ? LevelManager.Instance.CurrentLevelIndex + 1
            : CurrentLevel + 1;

        SaveSystem.UnlockLevel(nextLevel);
        saveData.highestLevel = Mathf.Max(saveData.highestLevel, nextLevel - 1);

        SaveSystem.Save(saveData);
    }

    private void LoadProgress()
    {
        saveData = SaveSystem.Load();
        TotalScore = saveData.totalScore;
        HighScore = saveData.highScore;
        Lives = saveData.lives > 0 ? saveData.lives : startingLives;
    }

    public void ResetGame()
    {
        TotalScore = 0;
        Lives = startingLives;
        FruitsCollected = 0;
        LevelScore = 0;
        LevelFruits = 0;
        CurrentLevel = 1;
        StopTimer();
        SaveSystem.DeleteSave();
        saveData = SaveSystem.Load(); // fresh save
        UIEvents.OnLivesChanged?.Invoke(Lives);
        UIEvents.OnScoreChanged?.Invoke(0);
        UIEvents.OnFruitsChanged?.Invoke(0);
    }
}

//UI Event Bus
public static class UIEvents
{
    public static Action<int> OnScoreChanged;
    public static Action<int> OnLivesChanged;
    public static Action<int> OnFruitsChanged;
    public static Action<float> OnHealthChanged;
    public static Action<float> OnTimerChanged;
    public static Action<PowerUpData> OnPowerUpCollected;
    public static Action<int> OnAmmoChanged;
    public static Action<int, int> OnLevelComplete;   // score, fruits
    public static Action<int, int> OnGameOver;        // score, fruits

    //Clear all subscribers — call on scene unload to prevent leaks
    public static void ClearAll()
    {
        OnScoreChanged = null;
        OnLivesChanged = null;
        OnFruitsChanged = null;
        OnHealthChanged = null;
        OnTimerChanged = null;
        OnPowerUpCollected = null;
        OnAmmoChanged = null;
        OnLevelComplete = null;
        OnGameOver = null;
    }
}