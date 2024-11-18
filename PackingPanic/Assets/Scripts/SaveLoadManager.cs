using System.IO;
using UnityEngine;

public static class SaveLoadManager
{
    private static string savePath = Application.persistentDataPath + "/playerProgress.json";


    public static void SaveProgress(PlayerProgress progress)
    {
        string json = JsonUtility.ToJson(progress, true);
        File.WriteAllText(savePath, json);
        Debug.Log(savePath);
    }

    public static PlayerProgress LoadProgress()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<PlayerProgress>(json);
        }

        // Return a new instance if no file exists
        PlayerProgress playerProgress = new PlayerProgress();
        playerProgress.totalCoins = 1500;
        return playerProgress;
    }
}
