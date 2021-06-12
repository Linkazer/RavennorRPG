using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_ChangeSpriteColor : MonoBehaviour, IFeedbackSystem
{
    [SerializeField]
    private SpriteRenderer targetSprite;
    [SerializeField]
    private Color targetColor;
    [SerializeField]
    private int loopNumber = 1;
    [SerializeField]
    private float colorDisplayTime = 0.5f, timeBetweenLoop = 0f;

    private bool isPlaying = false;

    public void Play()
    {
        if (!isPlaying)
        {
            isPlaying = true;
            StartCoroutine(ChangeColor());
        }
    }

    IEnumerator ChangeColor()
    {
        Color baseColor = targetSprite.color;
        
        for (int i = 0; i < loopNumber; i++)
        {
            targetSprite.color = targetColor;
            yield return new WaitForSeconds(colorDisplayTime);
            targetSprite.color = baseColor;
            yield return new WaitForSeconds(timeBetweenLoop);
        }
        isPlaying = false;
    }
}
