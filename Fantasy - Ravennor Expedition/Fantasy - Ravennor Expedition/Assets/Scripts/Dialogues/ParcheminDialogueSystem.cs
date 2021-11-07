using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class ParcheminDialogueSystem : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI storyText;

    private ParcheminScriptable currentStory;

    public UnityEvent EndDialogueEvent = new UnityEvent();

    [SerializeField] private Animator bookAnimator;

    public void ShowStory(ParcheminScriptable newStory)
    {
        bookAnimator.SetTrigger("Open");
        currentStory = newStory;

        storyText.text = newStory.storyText;

        gameObject.SetActive(true);
    }

    public void GetResponse(int index)
    {
        bookAnimator.ResetTrigger("Open");
        if (currentStory.reponses[index].nextStory != null)
        {
            ShowStory(currentStory.reponses[index].nextStory);
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        if(BattleManager.instance != null)
        {
            if (currentStory.battleBegin)
            {
                BattleManager.instance.BattleBegin();
            }
            else if(currentStory.goToCamp)
            {
                Debug.Log("Pass");
                BattleManager.instance.SetWinPanel();
            }
        }

        EndDialogueEvent.Invoke();
        bookAnimator.SetTrigger("Close");
    }

    public void EndBookAnimation()
    {
        bookAnimator.ResetTrigger("Close");
        gameObject.SetActive(false);
    }
}
