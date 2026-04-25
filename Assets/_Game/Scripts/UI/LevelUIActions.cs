using UnityEngine;

public class LevelUIActions : MonoBehaviour
{
    public void OnNextLevelClicked()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadNextLevel();
        else
            Debug.LogError("LevelManager.Instance is null");
    }

    public void OnRestartClicked()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.RestartLevel();
        else
            Debug.LogError("LevelManager.Instance is null");
    }

    public void OnMainMenuClicked()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadMainMenu();
        else
            Debug.LogError("LevelManager.Instance is null");
    }
}