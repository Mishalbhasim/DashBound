using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : Singleton<LevelManager>
{
    [Header("Configuration")]
    [SerializeField] private LevelData[] levelDatabase;
    [SerializeField] private float transitionDuration = 0.8f;

    public LevelData CurrentLevelData { get; private set; }
    public int CurrentLevelIndex { get; private set; } = 1;

    // Checkpoint data
    private Vector3 checkpointPosition;
    private bool hasCheckpoint = false;
    private string checkpointScene = "";

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Keep checkpoint only if same scene (player was hurt)
        // Clear it if loading a different scene (next level)
        if (checkpointScene != scene.name)
        {
            hasCheckpoint = false;
            checkpointScene = "";
        }

        for (int i = 0; i < levelDatabase.Length; i++)
        {
            if (levelDatabase[i].sceneName == scene.name)
            {
                CurrentLevelIndex = i + 1;
                CurrentLevelData = levelDatabase[i];

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ResetFruits();
                    GameManager.Instance.StartLevelTimer(CurrentLevelData.timeLimit);
                    GameManager.Instance.SetGameState(GameState.Playing);
                }

                if (AudioManager.Instance != null)
                {
                    if (CurrentLevelData.isBossLevel && CurrentLevelData.bossMusic != null)
                        AudioManager.Instance.PlayMusic(CurrentLevelData.bossMusic);
                    else
                        AudioManager.Instance.PlayMusic(CurrentLevelData.backgroundMusic);
                }

                break;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────
    public void SetCheckpoint(Vector3 position)
    {
        checkpointPosition = position;
        hasCheckpoint = true;
        checkpointScene = SceneManager.GetActiveScene().name;
        Debug.Log($"[LevelManager] Checkpoint set at {position} in {checkpointScene}");
    }

    // ── ADDED: clears checkpoint — called on restart and game over ────
    public void ClearCheckpoint()
    {
        hasCheckpoint = false;
        checkpointScene = "";
        Debug.Log("[LevelManager] Checkpoint cleared");
    }

    public Vector3 GetSpawnPosition()
    {
        if (hasCheckpoint)
        {
            Debug.Log($"[LevelManager] Spawning at checkpoint: {checkpointPosition}");
            return checkpointPosition;
        }

        GameObject spawn = GameObject.FindWithTag("SpawnPoint");
        if (spawn != null)
        {
            Debug.Log($"[LevelManager] Spawning at SpawnPoint: {spawn.transform.position}");
            return spawn.transform.position;
        }

        Debug.LogWarning("[LevelManager] No SpawnPoint found!");
        return Vector3.zero;
    }

    // ─────────────────────────────────────────────────────────────────
    public void RestartLevel()
    {
        // ── ADDED: clear checkpoint on restart so player starts fresh ─
        ClearCheckpoint();
        StartCoroutine(LoadWithTransition(SceneManager.GetActiveScene().name));
    }

    public void LoadNextLevel()
    {
        int next = CurrentLevelIndex + 1;
        if (next > levelDatabase.Length)
        {
            GameManager.Instance?.SetGameState(GameState.LevelComplete);
            return;
        }
        LoadLevel(next);
    }

    public void LoadLevel(int index)
    {
        if (index < 1 || index > levelDatabase.Length)
        {
            Debug.LogWarning($"[LevelManager] Invalid level index: {index}");
            return;
        }
        StartCoroutine(LoadWithTransition(levelDatabase[index - 1].sceneName));
    }

    public void LoadMainMenu()
    {
        ClearCheckpoint();   // ── ADDED: also clear on main menu
        StartCoroutine(LoadWithTransition("MainMenu"));
    }

    private IEnumerator LoadWithTransition(string sceneName)
    {
        GameManager.Instance?.SetGameState(GameState.Loading);

        if (ObjectPool.Instance != null)
            ObjectPool.Instance.ReturnAll();

        if (UITransitionController.Instance != null)
        {
            UITransitionController.Instance.FadeOut(transitionDuration);
            yield return new WaitForSecondsRealtime(transitionDuration);
        }

        SceneManager.LoadScene(sceneName);
    }
}