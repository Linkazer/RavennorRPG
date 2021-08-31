using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    public static SaveData data;

    private static string dataPath;

    [SerializeField] private SaveDataScriptable cheatData;

    [SerializeField] private string firstLevel;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadUnlockedLevel();
            dataPath = Application.persistentDataPath + "/RavenorLevels.json";
        }
    }

    public static void SaveUnlockedLevels()
    {
        string jsonData = JsonUtility.ToJson(data, true);
        File.WriteAllText(dataPath, jsonData);
    }

    public static void LoadUnlockedLevel()
    {
        if (instance.cheatData != null)
        {
            data = instance.cheatData.data;
        }
        else
        {
            if (!File.Exists(Application.persistentDataPath + "/RavenorLevels.json"))
            {
                data = new SaveData(instance.firstLevel);
            }
            else
            {
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(Application.persistentDataPath + "/RavenorLevels.json"));
            }
        }
    }

    public static bool DoesLevelExist(string levelName)
    {
        if(data != null && data.unlockedLevelIds.Contains(levelName))
        {
            return true;
        }
        return false;
    }
}
