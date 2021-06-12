using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_PlaySprite : MonoBehaviour, IFeedbackSystem
{
    public void Play()
    {
        gameObject.SetActive(true);
    }

    private void Disable()
    {
        gameObject.SetActive(false);
    }
}
