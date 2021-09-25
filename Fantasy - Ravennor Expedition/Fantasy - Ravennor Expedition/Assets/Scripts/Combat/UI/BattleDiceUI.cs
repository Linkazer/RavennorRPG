using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum BattleDiceResult
{
    Hit,
    Block,
    Reduce
}

public class BattleDiceUI : MonoBehaviour
{
    [SerializeField] private GameObject diceResultObjects;
    [SerializeField] private GameObject diceResultArmorObjects;
    [SerializeField] private GameObject diceResultReductionbjects;
    [SerializeField] private TextMeshProUGUI diceResultTexts;

    [SerializeField] private TextMeshProUGUI armorValue;

    public void ShowDiceResults(int value, BattleDiceResult result)
    {
        ResetDice();
        diceResultObjects.SetActive(true);

        switch(result)
        {
            case BattleDiceResult.Hit:
                diceResultTexts.text = value.ToString();
                break;
            case BattleDiceResult.Block:
                diceResultArmorObjects.SetActive(true);
                break;
            case BattleDiceResult.Reduce:
                diceResultReductionbjects.SetActive(true);
                diceResultTexts.text = value.ToString();
                break;
        }
    }

    public void HideDice()
    {
        diceResultObjects.SetActive(false);
        ResetDice();
    }

    private void ResetDice()
    {
        diceResultArmorObjects.SetActive(false);
        diceResultReductionbjects.SetActive(false);
        diceResultTexts.text = "";
    }

    public void ShowArmorValue(int value)
    {
        /*armorValue.text = value.ToString();
        diceResultAnimator.SetTrigger("ShowArmor");*/
    }
}
