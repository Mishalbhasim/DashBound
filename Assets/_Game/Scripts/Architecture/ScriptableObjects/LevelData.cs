using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "PixelAdventure/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public int levelIndex;            // 1-15
    public string levelName;
    public string sceneName;          // Must match scene name exactly

    [Header("Gameplay")]
    public int totalFruits;           // Total collectibles in level
    public float timeLimit;           // 0 = no limit
    public bool isBossLevel;          // Levels 5, 10, 15

    [Header("Progression")]
    public int fruitsRequiredToUnlock; // Fruits needed from prev level to unlock this
    public LevelData nextLevel;

    [Header("Visual")]
    public Sprite levelThumbnail;
    public Color backgroundTint = Color.white;

    [Header("Audio")]
    public AudioClip backgroundMusic;
    public AudioClip bossMusic;
}