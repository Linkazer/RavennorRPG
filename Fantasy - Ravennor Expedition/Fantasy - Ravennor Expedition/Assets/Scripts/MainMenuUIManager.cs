using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUIManager : MonoBehaviour
{
    private void Start()
    {
        LoadingScreenManager.instance.HideScreen();
    }

    public void Play()
    {
        RavenorGameManager.instance.LoadBattle();
    }

    public void PlayTuto()
    {
        RavenorGameManager.instance.LoadTuto();
    }
}
