using System.Collections;
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
    [SerializeField] private List<GameObject> diceResultObjects;
    [SerializeField] private List<GameObject> diceResultArmorObjects;
    [SerializeField] private List<TextMeshProUGUI> diceResultTexts;
    [SerializeField] private TextMeshProUGUI diceResultDamageText;

    public void ShowDiceResults(List<int> values, List<bool> results, int total)
    {
        for (int i = 0; i < diceResultObjects.Count; i++)
        {
            if (i < results.Count)
            {
                if (results[i])
                {
                    diceResultArmorObjects[i].SetActive(true);
                    diceResultTexts[i].text = "";
                }
                else
                {
                    diceResultArmorObjects[i].SetActive(false);
                    diceResultTexts[i].text = values[i].ToString();
                }

                diceResultObjects[i].SetActive(true);
            }
            else
            {
                diceResultObjects[i].SetActive(false);
            }
        }
        diceResultDamageText.text = total.ToString();

        diceResultAnimator.SetTrigger("ShowDices");
    }
}
