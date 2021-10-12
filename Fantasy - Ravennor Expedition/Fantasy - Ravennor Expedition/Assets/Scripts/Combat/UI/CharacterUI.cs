using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private Transform feedbackUiHandler;
    [SerializeField] private CharacterDiceHitUI dice;
    [SerializeField] private CharacterDirectHitUI directHit;

    public void ShowDiceResults(List<int> values, List<BattleDiceResult> results, int total)
    {
        GameObject toDisplay = Instantiate(dice.gameObject, feedbackUiHandler);
        toDisplay.GetComponent<CharacterDiceHitUI>().ShowDiceResults(values, results, total);
        StartCoroutine(DisplayObject(toDisplay));
    }

    public void ShowDirectHitResult(int amount, bool isHeal)
    {
        GameObject toDisplay = Instantiate(directHit.gameObject, feedbackUiHandler);
        toDisplay.GetComponent<CharacterDirectHitUI>().ShowResult(amount, isHeal);
        StartCoroutine(DisplayObject(toDisplay));
    }

    IEnumerator DisplayObject(GameObject toDisplay)
    {
        yield return new WaitForSeconds(3f);
        Destroy(toDisplay);
    }
}
