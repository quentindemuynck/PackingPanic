using System.Collections.Generic;

[System.Serializable]
public class PlayerProgress
{
    public List<StarData> levelStars = new List<StarData>();
    public int totalCoins = 0;

    // New fields for power-ups
    public int amountOfSpeedBoosts = 0;
    public int amountOfTimeSlows = 0;

    // Get stars for a specific level
    public int GetStarsForLevel(int levelIndex)
    {
        StarData starData = levelStars.Find(data => data.levelIndex == levelIndex);
        return starData != null ? starData.stars : 0;
    }

    // Set stars for a specific level
    public void SetStarsForLevel(int levelIndex, int stars)
    {
        StarData existingData = levelStars.Find(data => data.levelIndex == levelIndex);
        if (existingData != null)
        {
            // Update the existing entry
            existingData.stars = stars;
        }
        else
        {
            // Add new entry
            levelStars.Add(new StarData(levelIndex, stars));
        }
    }

    // Add or remove power-ups
    public void AddSpeedBoost(int amount)
    {
        amountOfSpeedBoosts += amount;
    }

    public void AddTimeSlow(int amount)
    {
        amountOfTimeSlows += amount;
    }

    public bool UseSpeedBoost()
    {
        if (amountOfSpeedBoosts > 0)
        {
            amountOfSpeedBoosts--;
            return true;
        }
        return false;
    }

    public bool UseTimeSlow()
    {
        if (amountOfTimeSlows > 0)
        {
            amountOfTimeSlows--;
            return true;
        }
        return false;
    }
}

[System.Serializable]
public class StarData
{
    public int levelIndex;
    public int stars;

    public StarData(int levelIndex, int stars)
    {
        this.levelIndex = levelIndex;
        this.stars = stars;
    }
}
