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
    [SerializeField] private RectTransform feedbackUIRectTransform;
    [SerializeField] private CharacterDiceHitUI dice;
    [SerializeField] private CharacterDirectHitUI directHit;
    [SerializeField] private CharacterEffectResult effectHit;

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

    public void ShowEffect(Sprite spriteEffect, bool doesAdd)
    {
        GameObject toDisplay = Instantiate(effectHit.gameObject, feedbackUiHandler);
        toDisplay.GetComponent<CharacterEffectResult>().ShowResult(spriteEffect, doesAdd);
        if (gameObject.activeSelf)
        {
            StartCoroutine(DisplayObject(toDisplay));
        }
    }
    IEnumerator DisplayObject(GameObject toDisplay)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(feedbackUIRectTransform);
        yield return new WaitForSeconds(3f);
        Destroy(toDisplay);
    }
}
