using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUIManager : MonoBehaviour
{
    public void Play()
    {
        RavenorGameManager.instance.LoadBattle();
    }
}
