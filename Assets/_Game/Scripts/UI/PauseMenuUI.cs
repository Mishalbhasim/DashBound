using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Slider musicSlider, sfxSlider;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        bool paused = GameManager.Instance.CurrentState == GameState.Paused;
        if (paused)
        {
            GameManager.Instance.SetGameState(GameState.Playing);
            pausePanel.SetActive(false);
        }
        else
        {
            GameManager.Instance.SetGameState(GameState.Paused);
            pausePanel.SetActive(true);
        }
    }

    public void OnMusicSliderChanged(float v) => AudioManager.Instance.SetMusicVolume(v);
    public void OnSFXSliderChanged(float v) => AudioManager.Instance.SetSFXVolume(v);
    public void OnResumeClicked() => TogglePause();

    public void OnRestartClicked()
    {
        GameManager.Instance.SetGameState(GameState.Playing);
        pausePanel.SetActive(false);
        LevelManager.Instance.RestartLevel();
    }

    public void OnMainMenuClicked()
    {
        GameManager.Instance.SetGameState(GameState.Playing);
        pausePanel.SetActive(false);
        LevelManager.Instance.LoadMainMenu();
    }

    public void OnQuitClicked() => Application.Quit();
}