using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CharacterDiceHitUI : MonoBehaviour
{
    [SerializeField] private Animator diceResultAnimator;

    [Header("Dice Results")]
    [SerializeField] private List<BattleDiceUI> diceResultObjects;
    [SerializeField] private TextMeshProUGUI diceResultDamageText;

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
}
