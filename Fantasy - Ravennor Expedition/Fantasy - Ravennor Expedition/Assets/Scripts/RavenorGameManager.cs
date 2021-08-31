using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RavenorGameManager : MonoBehaviour
{
    public static RavenorGameManager instance;

    [SerializeField]
    private GameObject nextBattle;
    private RoomManager nextRoom;

    //public List<PersonnageScriptables> playerPersos;

    public ParcheminScriptable dialogueToDisplay;

    [SerializeField]
    private float difficultyMultiplier = 1;

    [Header("Musics")]
    [SerializeField] private AudioClip menuClip;
    [SerializeField] private AudioClip battleClip;
    [SerializeField] private AudioClip campClip;

    public static AudioClip BattleClip => instance.battleClip;

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
        }
    }

    public static void SetNextBattle(GameObject newBattle)
    {
        instance.SetLocalNextBattle(newBattle);
    }

    public static void AddUnlockLevel(string levelToAdd)
    {
        SaveManager.data.AddLevel(levelToAdd);
        SaveManager.SaveUnlockedLevels();
    }


    private void OnEnable()
    {
        nextRoom = nextBattle.GetComponent<RoomManager>();
    }

    public void SetLocalNextBattle(GameObject newBattle)
    {
        nextBattle = newBattle;
        nextRoom = nextBattle.GetComponent<RoomManager>();
    }

    public GameObject GetBattle()
    {
        return nextBattle;
    }

    public RoomManager GetCurrentBattle()
    {
        return nextRoom;
    }

    #region Loading
    public void LoadMainMenu()
    {
        LoadScene(0);
        SoundSyst.ChangeMainMusic(menuClip);
    }

    public void LoadBattle()
    {
        LoadScene(1);
    }

    /*public void LoadCamp()
    {
        LoadScene(2);
        if (nextRoom.backgroundMusic != null)
        {
            SoundSyst.ChangeMainMusic(nextRoom.backgroundMusic);
        }
        else
        {
            SoundSyst.ChangeMainMusic(campClip);
        }
    }*/

    internal void LoadTuto()
    {
        LoadScene(3);
    }

    void LoadScene(int sceneNb)
    {
        if (LoadingScreenManager.instance != null)
        {
            LoadingScreenManager.instance.ShowScreen();
        }
        StartCoroutine(LoadAsyncScene(sceneNb));
    }

    IEnumerator LoadAsyncScene(int index)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(index);
        float loadTime = 0;

        while (!asyncLoad.isDone || loadTime < 3)
        {
            loadTime += Time.fixedDeltaTime;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
    }
    #endregion

    public void CloseGame()
    {
        Application.Quit();
    }

    public float GetDifficulty()
    {
        return difficultyMultiplier;
    }
}
