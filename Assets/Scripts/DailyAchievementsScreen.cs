using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

[Serializable]
public class Achievement
{
    public string title;
    public string description;
    public string playerPrefKey;
    public string comparison;
    public string requiredValue;
    public bool isAccumulative;
    public bool isEventBased = true;
    public int targetCount = 1;
    public float progress = 0;
    public bool IsCompleted;
}

[Serializable]
public class AchievementSaveCollection
{
    public List<Achievement> allAchievements = new List<Achievement>();
    public string lastResetDate;
    public List<int> dailyIndexes = new List<int>();
}

public class DailyAchievementsScreen : MonoBehaviour
{
    public TMP_Text[] achievementTexts;
    public TMP_Text[] descriptionTexts;
    public GameObject[] checkmarks;

    private string savePath => Path.Combine(Application.persistentDataPath, "dailyAchievements.json");
    private readonly List<string> gradeOrder = new() { "F", "C", "B", "A", "S", "SS" };

    private AchievementSaveCollection saveData = new();
    private List<Achievement> dailyAchievements = new();

    private void OnEnable()
    {
        LoadAchievements();
        DisplayAchievements();
    }

    private void OnDestroy() => SaveAchievements();
    private void OnApplicationQuit() => SaveAchievements();

    private void LoadAchievements()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            saveData = JsonUtility.FromJson<AchievementSaveCollection>(json);
        }
        else
        {
            saveData = new AchievementSaveCollection
            {
                allAchievements = GetDefaultAchievements(),
                lastResetDate = "",
                dailyIndexes = new List<int>()
            };
        }

        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        if (saveData.lastResetDate != today || saveData.dailyIndexes.Count == 0)
        {
            PickNewDailyAchievements();
            saveData.lastResetDate = today;
        }

        dailyAchievements.Clear();
        foreach (int i in saveData.dailyIndexes)
        {
            if (i >= 0 && i < saveData.allAchievements.Count)
                dailyAchievements.Add(saveData.allAchievements[i]);
        }

        SaveAchievements();
    }

    private void PickNewDailyAchievements()
    {
        saveData.dailyIndexes.Clear();

        List<int> availableIndexes = new();
        for (int i = 0; i < saveData.allAchievements.Count; i++)
        {
            if (!saveData.allAchievements[i].IsCompleted)
                availableIndexes.Add(i);
        }

        // Shuffle available unfinished achievements
        for (int i = availableIndexes.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (availableIndexes[i], availableIndexes[j]) = (availableIndexes[j], availableIndexes[i]);
        }

        // Pick up to 3
        int count = Mathf.Min(3, availableIndexes.Count);
        for (int i = 0; i < count; i++)
            saveData.dailyIndexes.Add(availableIndexes[i]);
    }

    public void EvaluateAchievements()
    {
        foreach (var achievement in dailyAchievements)
        {
            float currentValue = GetStatValue(achievement.playerPrefKey);
            float requiredValue = GetStatValue(achievement.requiredValue);
            bool conditionMet = false;

            if (achievement.playerPrefKey == "LetterGrade" && !float.TryParse(achievement.requiredValue, out _))
            {
                string currentGrade = PlayerPrefs.GetString("LetterGrade", "F");
                string requiredGrade = achievement.requiredValue;
                conditionMet = IsGradeSufficient(currentGrade, requiredGrade);
            }
            else
            {
                conditionMet = Compare(currentValue, achievement.comparison, requiredValue);
            }

            if (achievement.isAccumulative)
            {
                if (achievement.isEventBased)
                {
                    if (conditionMet && achievement.progress < achievement.targetCount)
                        achievement.progress += 1;
                }
                else
                {
                    if (currentValue > 0)
                        achievement.progress = Mathf.Min(achievement.progress + currentValue, achievement.targetCount);
                }

                achievement.IsCompleted = achievement.targetCount > 0 && achievement.progress >= achievement.targetCount;
            }
            else
            {
                achievement.progress = conditionMet ? 1 : 0;
                achievement.IsCompleted = conditionMet;
            }
        }

        SaveAchievements();
    }

    public void DisplayAchievements()
    {
        for (int i = 0; i < achievementTexts.Length && i < dailyAchievements.Count; i++)
        {
            var ach = dailyAchievements[i];
            string displayProgress = "";
            bool completed = ach.IsCompleted;

            if (ach.targetCount == 1)
            {
                float currentValue = GetStatValue(ach.playerPrefKey);
                float requiredValue = GetStatValue(ach.requiredValue);
                bool conditionMet = Compare(currentValue, ach.comparison, requiredValue);
                completed = conditionMet;

                if (!(Mathf.Approximately(currentValue, 0) && Mathf.Approximately(requiredValue, 0)) &&
                    !(Mathf.Approximately(currentValue, 0) && requiredValue == 1))
                {
                    displayProgress = conditionMet
                        ? "\n<color=green>? Completed</color>"
                        : $"\nProgress: {currentValue}/{requiredValue}";
                }
                else
                {
                    completed = false;
                }
            }
            else if (ach.targetCount > 1)
            {
                if (!(Mathf.Approximately(ach.progress, 0) && (ach.targetCount <= 1 || ach.targetCount == 0)))
                {
                    displayProgress = completed
                        ? "\n<color=green>? Completed</color>"
                        : $"\nProgress: {ach.progress}/{ach.targetCount}";
                }

                if (Mathf.Approximately(ach.progress, 0) && ach.targetCount == 0)
                {
                    completed = false;
                    displayProgress = "";
                }
            }

            achievementTexts[i].text = ach.title;
            descriptionTexts[i].text = ach.description + displayProgress;
            checkmarks[i].SetActive(completed);
        }
    }

    private float GetStatValue(string keyOrNumber)
    {
        if (float.TryParse(keyOrNumber, out float num)) return num;

        if (PlayerPrefs.HasKey(keyOrNumber))
        {
            if (float.TryParse(PlayerPrefs.GetString(keyOrNumber, ""), out float parsed)) return parsed;
            return PlayerPrefs.GetFloat(keyOrNumber, PlayerPrefs.GetInt(keyOrNumber, 0));
        }

        return 0;
    }

    private bool Compare(float a, string comparison, float b)
    {
        return comparison switch
        {
            ">" => a > b,
            ">=" => a >= b,
            "<" => a < b,
            "<=" => a <= b,
            "=" => Mathf.Approximately(a, b),
            _ => false,
        };
    }

    private bool IsGradeSufficient(string actualGrade, string requiredGrade)
    {
        int actualIndex = gradeOrder.IndexOf(actualGrade.ToUpper());
        int requiredIndex = gradeOrder.IndexOf(requiredGrade.ToUpper());
        return actualIndex >= requiredIndex;
    }

    private void SaveAchievements()
    {
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
    }

    private List<Achievement> GetDefaultAchievements() => new()
    {
        new Achievement { title = "Scorer", description = "Score more than 100000 points", playerPrefKey = "SongScore", comparison = ">", requiredValue = "100000" },
        new Achievement { title = "Combo Master", description = "Get a Full Combo", playerPrefKey = "BestCombo", comparison = "=", requiredValue = "TotalMaxCombo" },
        new Achievement { title = "Hit Machine", description = "Get more than 500 total hits", playerPrefKey = "TotalHits", comparison = ">", requiredValue = "500", isAccumulative = true, isEventBased = false, targetCount = 500 },
        new Achievement { title = "Combo King", description = "Best Combo over 300", playerPrefKey = "BestCombo", comparison = ">", requiredValue = "300" },
        new Achievement { title = "Collector", description = "Collect all notes in a song", playerPrefKey = "TotalCollected", comparison = "=", requiredValue = "MaxCollected" },
        new Achievement { title = "Enemy Slayer", description = "Kill all enemies in a song", playerPrefKey = "EnemiesKilled", comparison = "=", requiredValue = "TotalEnemies" },
        new Achievement { title = "Straight A's", description = "Get an A grade or better in 2 songs", playerPrefKey = "LetterGrade", comparison = ">=", requiredValue = "A", isAccumulative = true, isEventBased = true, targetCount = 2 },
        new Achievement { title = "Certified Note Millionaire", description = "Accumulate a total score of 1,000,000 across all songs", playerPrefKey = "SongScore", comparison = ">=", requiredValue = "1000000", isAccumulative = true, isEventBased = false, targetCount = 1000000},
        new Achievement { title = "Rhythm God", description = "Get an SS grade", playerPrefKey = "LetterGrade", comparison = "=", requiredValue = "SS" },
    };

#if UNITY_EDITOR
    [ContextMenu("Delete Achievement Save")]
    private void DeleteAchievementsJson()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Deleted daily achievements file at: " + savePath);
        }
    }
#endif
}
