
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class GameSaveData
{
    
    public int totalScore = 0;
    public int highScore = 0;
    public int highestLevel = 1;
    public int lives = 3;
    public bool[] levelUnlocked = new bool[16]; 

    
    public int[] leaderboardScores = new int[5];
    public string[] leaderboardNames = new string[5];

    
    public float masterVolume = 1f;
    public float musicVolume = 0.7f;
    public float sfxVolume = 1f;
}

public static class SaveSystem
{
    private static readonly string SAVE_PATH =
        Path.Combine(Application.persistentDataPath, "save.json");

    private static GameSaveData _cache;

    
    public static GameSaveData Load()
    {
        if (_cache != null) return _cache;

        if (File.Exists(SAVE_PATH))
        {
            try
            {
                string json = File.ReadAllText(SAVE_PATH);
                _cache = JsonUtility.FromJson<GameSaveData>(json);
                Debug.Log("[SaveSystem] Loaded from: " + SAVE_PATH);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SaveSystem] Load failed, creating new save. " + e.Message);
                _cache = CreateNewSave();
            }
        }
        else
        {
            _cache = CreateNewSave();
        }

        return _cache;
    }

   
    public static void Save(GameSaveData data)
    {
        try
        {
            _cache = data;
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(SAVE_PATH, json);
            Debug.Log("[SaveSystem] Saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError("[SaveSystem] Save failed: " + e.Message);
        }
    }

    
    public static void DeleteSave()
    {
        _cache = null;
        if (File.Exists(SAVE_PATH))
        {
            File.Delete(SAVE_PATH);
            Debug.Log("[SaveSystem] Save deleted");
        }
    }

    
    public static void SubmitScore(int score, string playerName = "Player")
    {
        GameSaveData data = Load();

        
        for (int i = 0; i < data.leaderboardScores.Length; i++)
        {
            if (score > data.leaderboardScores[i])
            {
                
                for (int j = data.leaderboardScores.Length - 1; j > i; j--)
                {
                    data.leaderboardScores[j] = data.leaderboardScores[j - 1];
                    data.leaderboardNames[j] = data.leaderboardNames[j - 1];
                }
                data.leaderboardScores[i] = score;
                data.leaderboardNames[i] = playerName;
                break;
            }
        }

        Save(data);
    }

   
    private static GameSaveData CreateNewSave()
    {
        var data = new GameSaveData();
        data.levelUnlocked = new bool[16];
        data.levelUnlocked[1] = true; 
        data.leaderboardScores = new int[5];
        data.leaderboardNames = new string[] { "---", "---", "---", "---", "---" };
        return data;
    }

    public static bool IsLevelUnlocked(int levelIndex)
    {
        var data = Load();
        if (levelIndex < 1 || levelIndex >= data.levelUnlocked.Length) return false;
        return data.levelUnlocked[levelIndex];
    }

    public static void UnlockLevel(int levelIndex)
    {
        var data = Load();
        if (levelIndex >= 1 && levelIndex < data.levelUnlocked.Length)
        {
            data.levelUnlocked[levelIndex] = true;
            Save(data);
        }
    }
}