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

    public GameObject GetNextBattle()
    {
        return nextBattle;
    }

    public void LoadBattle()
    {
        SceneManager.LoadScene(1);
    }

    public void LoadCamp()
    {
        SceneManager.LoadScene(2);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

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
}
