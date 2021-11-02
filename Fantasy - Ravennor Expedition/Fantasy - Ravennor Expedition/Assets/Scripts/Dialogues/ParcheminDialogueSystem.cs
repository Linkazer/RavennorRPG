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
    [SerializeField]
    private List<TextMeshProUGUI> reponsesTexts;

    private ParcheminScriptable currentStory;

    public UnityEvent EndDialogueEvent = new UnityEvent();

    public void ShowStory(ParcheminScriptable newStory)
    {
        currentStory = newStory;

        storyText.text = newStory.storyText;

        for(int i = 0; i < reponsesTexts.Count; i++)
        {
            if(i<newStory.reponses.Count)
            {
                reponsesTexts[i].gameObject.SetActive(true);
                reponsesTexts[i].text = newStory.reponses[i].text;
            }
            else
            {
                reponsesTexts[i].gameObject.SetActive(false);
            }
        }

        gameObject.SetActive(true);
    }

    public void GetResponse(int index)
    {
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
                BattleUiManager.instance.EndDialogue();
                BattleManager.instance.BattleBegin();
            }
            else if(currentStory.goToCamp)
            {
                Debug.Log("Pass");
                BattleManager.instance.SetWinPanel();
            }
        }

        EndDialogueEvent.Invoke();
        gameObject.SetActive(false);
    }
}
