﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CharacterUI : MonoBehaviour
{
    [Header("Dice Results")]
    [SerializeField] private Animator diceResultAnimator;
    [SerializeField] private List<BattleDiceUI> diceResultObjects;
    [SerializeField] private List<GameObject> diceResultArmorObjects;
    [SerializeField] private List<TextMeshProUGUI> diceResultTexts;
    [SerializeField] private TextMeshProUGUI diceResultDamageText;

    [SerializeField] private TextMeshProUGUI armorValue;

    public void ShowDiceResults(List<int> values, List<BattleDiceResult> results, int total)
    {
        for (int i = 0; i < diceResultObjects.Count; i++)
        {
            if (i < results.Count)
            {
                diceResultObjects[i].ShowDiceResults(values[i], results[i]);
            }
            else
            {
                diceResultObjects[i].HideDice();
            }
        }
        diceResultDamageText.text = total.ToString();

        diceResultAnimator.SetTrigger("ShowDices");
    }

    public void ShowArmorValue(int value)
    {
        /*armorValue.text = value.ToString();
        diceResultAnimator.SetTrigger("ShowArmor");*/
    }
}
