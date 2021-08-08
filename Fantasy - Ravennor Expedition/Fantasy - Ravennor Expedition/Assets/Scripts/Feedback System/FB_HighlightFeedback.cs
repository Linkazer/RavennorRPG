using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_HighlightFeedback : MonoBehaviour, IFeedbackSystem
{
    [SerializeField]
    private Color targetColor = Color.white, outputColor = Color.white;

    [SerializeField] private SpriteRenderer renderer;

    private void Start()
    {
        renderer = gameObject.GetComponent<SpriteRenderer>();
    }

    public void Play()
    {
        renderer.material.SetColor("_Color1in", targetColor);
        renderer.material.SetColor("_Color1out", outputColor);
    }

    public void UnPlay()
    {
        renderer.material.SetColor("_Color1in", Color.white);
        renderer.material.SetColor("_Color1out", Color.white);
    }
}
