using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RavenorGameManager : MonoBehaviour
{
    public static RavenorGameManager instance;

    /* Contient :
     * - La liste des personnages dans le groupe
     * - Le niveau actuel (Update quand on clique sur le prochain niveau)
     * - Les changements de scènes
     * 
     */

    [SerializeField]
    private GameObject nextBattle;

    public List<PersonnageScriptables> playerPersos;

    [SerializeField]
    private List<int> characterToLevelUp = new List<int>();

    public ParcheminScriptable dialogueToDisplay;

    [SerializeField]
    private float difficultyMultiplier = 1;


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

    public void SetNextBattle(GameObject newBattle)
    {
        nextBattle = newBattle;
    }

    public GameObject GetBattle()
    {
        return nextBattle;
    }

    #region Loading
    public void LoadMainMenu()
    {
        LoadScene(0);
        //SceneManager.LoadScene(0);
    }

    public void LoadBattle()
    {
        LoadScene(1);
        //SceneManager.LoadScene(1);
    }

    public void LoadCamp()
    {
        LoadScene(2);
        //SceneManager.LoadScene(2);
    }

    internal void LoadTuto()
    {
        LoadScene(3);
        //SceneManager.LoadScene(3);
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

    public void SetLevelUp()
    {
        for(int i = 0; i < playerPersos.Count; i++)
        {
            characterToLevelUp.Add(i);
        }
    }

    public int GetNextLevelUp()
    {
        if(characterToLevelUp.Count>0)
        {
            int toReturn = characterToLevelUp[0];
            characterToLevelUp.RemoveAt(0);
            return toReturn;
        }
        return -1;
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public float GetDifficulty()
    {
        return difficultyMultiplier;
    }
}
