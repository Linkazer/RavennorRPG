using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public List<string> unlockedLevelIds = new List<string>();

    public SaveData()
    {
        unlockedLevelIds = new List<string>();
        unlockedLevelIds.Add("Hist1_Lvl1");
    }

    public SaveData(string firstLevel)
    {
        unlockedLevelIds = new List<string>();
        unlockedLevelIds.Add(firstLevel);
    }

    public void AddLevel(string levelId)
    {
        if(!unlockedLevelIds.Contains(levelId))
        {
            unlockedLevelIds.Add(levelId);
        }
    }
}
