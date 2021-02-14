using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void ShowScreen()
    {
        gameObject.SetActive(true);
    }

    public void HideScreen()
    {
        gameObject.SetActive(false);
    }
}
