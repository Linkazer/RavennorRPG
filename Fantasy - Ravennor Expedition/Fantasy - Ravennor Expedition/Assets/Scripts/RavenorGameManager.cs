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
}
