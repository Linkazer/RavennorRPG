using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CharacterDirectHitUI : MonoBehaviour
{
    [SerializeField] private Animator diceResultAnimator;
    [SerializeField] private TextMeshProUGUI resultDamageText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private Color healColor;
    [SerializeField] private Color damageColor;

    public void ShowResult(int amount, bool isHeal)
    {
        diceResultAnimator.SetTrigger("ShowDices");
        resultDamageText.text = amount.ToString();
        if(isHeal)
        {
            typeText.text = "+ ";
            resultDamageText.color = healColor;
            typeText.color = healColor;
        }
        else
        {
            typeText.text = "- ";
            resultDamageText.color = damageColor;
            typeText.color = damageColor;
        }
    }
}
