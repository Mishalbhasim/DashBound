using UnityEngine;
using DG.Tweening;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Menu Intro")]
    public Transform title;
    public Transform[] buttons;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;

    [Header("Level Select Intro")]
    public Transform levelSelectTitle;
    public Transform backButton;
    public Transform[] levelButtons;

    private void Start()
    {
        Time.timeScale = 1f;
        PlayMainMenuIntro();
    }

    void PlayMainMenuIntro()
    {
        title.localScale = Vector3.zero;
        title.DOScale(1f, 0.6f).SetEase(Ease.OutBack);

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].localScale = Vector3.zero;
            buttons[i].DOScale(1f, 0.5f)
                .SetEase(Ease.OutBack)
                .SetDelay(0.2f + i * 0.15f);
        }
    }

    void PlayLevelSelectIntro()
    {
        levelSelectTitle.localScale = Vector3.zero;
        levelSelectTitle.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].localScale = Vector3.zero;
            levelButtons[i].DOScale(1f, 0.35f)
                .SetEase(Ease.OutBack)
                .SetDelay(0.1f + i * 0.03f);
        }

        backButton.localScale = Vector3.zero;
        backButton.DOScale(1f, 0.4f)
            .SetEase(Ease.OutBack)
            .SetDelay(0.2f);
    }

    public void PlayGame()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadLevel(1);
        else
            Debug.LogError("LevelManager.Instance is NULL");
    }

    public void OpenLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
        PlayLevelSelectIntro();
    }

    public void CloseLevelSelect()
    {
        levelSelectPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        PlayMainMenuIntro();
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    public void LoadLevelByIndex(int levelIndex)
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadLevel(levelIndex);
        else
            Debug.LogError("LevelManager.Instance is NULL");
    }
}