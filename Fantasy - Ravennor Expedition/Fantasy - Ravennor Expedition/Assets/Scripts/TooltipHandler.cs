using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipHandler : MonoBehaviour
{
    public void Show(string text)
    {
        UITooltipGestion.Show(text, gameObject);
    }

    public void Hide()
    {
        UITooltipGestion.Hide(gameObject);
    }

    private void OnDisable()
    {
        UITooltipGestion.Hide(gameObject);
    }
}
