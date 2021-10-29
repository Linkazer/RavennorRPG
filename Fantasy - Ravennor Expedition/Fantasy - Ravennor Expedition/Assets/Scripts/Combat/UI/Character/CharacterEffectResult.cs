using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterEffectResult : MonoBehaviour
{
    [SerializeField] private Animator diceResultAnimator;
    [SerializeField] private Image effectImage;
    [SerializeField] private Color addColor;
    [SerializeField] private Color removeColor;

    public void ShowResult(Sprite effectSprite, bool doesAdd)
    {
        diceResultAnimator.SetTrigger("ShowDices");
        effectImage.sprite = effectSprite;
        if(doesAdd)
        {
            effectImage.color = addColor;
        }
        else
        {
            effectImage.color = removeColor;
        }
    }
}
