using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_HighlightFeedback : MonoBehaviour, IFeedbackSystem
{
    [SerializeField]
    private Color targetColor = Color.white, outputColor = Color.white;

    public void Play()
    {
        gameObject.GetComponent<SpriteRenderer>().material.SetColor("_Color1in", targetColor);
        gameObject.GetComponent<SpriteRenderer>().material.SetColor("_Color1out", outputColor);
    }

    public void UnPlay()
    {
        gameObject.GetComponent<SpriteRenderer>().material.SetColor("_Color1in", Color.white);
        gameObject.GetComponent<SpriteRenderer>().material.SetColor("_Color1out", Color.white);
    }
}
