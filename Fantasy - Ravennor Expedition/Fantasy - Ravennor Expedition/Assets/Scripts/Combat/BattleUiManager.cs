using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BattleUiManager : MonoBehaviour
{
    public static BattleUiManager instance;

    [SerializeField]
    private GameObject mainBattleUI;
    [SerializeField]
    private List<CanvasGroup> playerInterractionCanvas;
    [SerializeField] private Camera mainCam;

    [SerializeField]
    private List<Image> turnImages;

    [Header("Actions du joueur")]
    [SerializeField]
    private Image baseAttackImage;
    [SerializeField]
    private List<Image> spellImages;
    [SerializeField]
    private Image currentPersoIcon;
    [SerializeField]
    private Image currentHpImage;
    [SerializeField]
    private TextMeshProUGUI currentHpText, currentMaxHpText;
    [SerializeField]
    private TextMeshProUGUI currentMaanaText;
    [SerializeField]
    private List<GameObject> actionPoint, baseActionPoint;

    private int currentMaxHps;

    [Header("Parchemins")]
    [SerializeField]
    private ParcheminDialogueSystem parcheminStory;
    [SerializeField]
    private CharacterInformationUI charaInfo;

    [Header("Ecrans de fin")]
    [SerializeField]
    private GameObject looseScreen;

    [Header("Information des sorts")]
    [SerializeField]
    private GameObject spellInfo;
    [SerializeField]
    private TextMeshProUGUI spellTitle, spellDescription, spellMaanaCost, spellIncantationTime;

    [Header("Maana Spent")]
    [SerializeField] private GameObject maanaSpentParent;
    [SerializeField] private List<TextMeshProUGUI> maanaToUseText;

    private RuntimeBattleCharacter currentChara;
    private int currentIndex;

    [Header("Error management")]
    [SerializeField]
    private TextMeshProUGUI errorTxt;
    [SerializeField]
    private UnityEvent errorFeedbackEvt;

    public RuntimeBattleCharacter GetCurrentChara()
    {
        return currentChara;
    }

    private void Awake()
    {
        instance = this;
    }
    
    private void Start()
    {
        mainBattleUI.SetActive(false);
    }

    public void SetUI()
    {
        mainBattleUI.SetActive(true);
    }

    public void SetPlayerUI(bool state)
    {
        foreach(CanvasGroup canvGroup in playerInterractionCanvas)
        {
            canvGroup.interactable = state;
            canvGroup.blocksRaycasts = state;
        }
    }

    public void SetNewTurn(int turnIndex, List<RuntimeBattleCharacter> roundList)
    {
        int offset = 0;
        currentIndex = turnIndex;

        currentChara = roundList[turnIndex];

        for(int i = 0; i < turnImages.Count; i++)
        {
            Color newCol = Color.white;
            newCol.a = 0;
            
            if (turnIndex + offset < roundList.Count)
            {
                if (roundList[turnIndex].GetCurrentHps() > 0)
                {
                    turnImages[i].sprite = roundList[turnIndex + offset].GetCharacterDatas().icon;
                    turnImages[i].color = Color.white;
                }
                else
                {
                    i--;
                }
                turnIndex++;
                turnIndex = (turnIndex) % roundList.Count;
            }
        }
    }
    
    public void SetNewCharacter(RuntimeBattleCharacter newChara)
    {
        currentMaxHps = newChara.GetCharacterDatas().GetMaxHps();
        currentMaxHpText.text = currentMaxHps.ToString();
        SetCurrentHps(newChara.GetCurrentHps());
        SetCurrentMaana(newChara.GetCurrentMaana());

        currentPersoIcon.sprite = newChara.GetCharacterDatas().icon;

        baseAttackImage.color = Color.white;
        baseAttackImage.sprite = newChara.GetActions()[0].icon;

        for (int i = 0; i < spellImages.Count; i++)
        {
            Color newCol = Color.white;
            newCol.a = 0;
            if(i<newChara.GetActions().Count-1)
            {
                spellImages[i].gameObject.SetActive(true);
                spellImages[i].color = Color.white;
                spellImages[i].sprite = newChara.GetActions()[i+1].icon;
            }
            else
            {
                spellImages[i].color = newCol;
                spellImages[i].gameObject.SetActive(false);
            }
        }

        UpdatePossibleAction();
    }

    public void UpdateSpells()
    {
        Color newCol = Color.black;
        newCol.r = 0.5f;
        newCol.g = 0.5f;
        newCol.b = 0.5f;

        
        if (!BattleManager.instance.IsActionAvailable(currentChara, currentChara.GetActions()[0]))
        {
            baseAttackImage.color = newCol;
        }
        else
        {
            baseAttackImage.color = Color.white;
        }

        for (int i = 0; i < currentChara.GetActions().Count-1; i++)
        {
            if(!BattleManager.instance.IsActionAvailable(currentChara, currentChara.GetActions()[i+1]))
            {
                spellImages[i].color = newCol;
            }
            else
            {
                spellImages[i].color = Color.white;
            }
        }
    }

    public void SetCurrentHps(int value)
    {
        currentHpText.text = value.ToString();
        currentHpImage.fillAmount = 1-((float)value / (float)currentMaxHps);
    }

    public void SetCurrentMaana(int value)
    {
        currentMaanaText.text = value.ToString();
    }

    public void ChooseSpell(int index)
    {
        PlayerBattleControllerManager.instance.ChooseSpell(index);
    }

    public void LoosingScreen()
    {
        looseScreen.SetActive(true);
    }

    public void StartDialogue(ParcheminScriptable newStory)
    {
        parcheminStory.gameObject.SetActive(true);
        parcheminStory.ShowStory(newStory);
    }

    public void EndDialogue()
    {
        parcheminStory.gameObject.SetActive(false);
    }

    public void ShowSpellInformations(int index)
    {
        CharacterActionScriptable toShow = PlayerBattleControllerManager.instance.GetSpell(index);

        spellTitle.text = toShow.nom;
        spellDescription.text = toShow.description;
        spellMaanaCost.text = toShow.maanaCost.ToString();
        spellIncantationTime.text = toShow.incantationTime.ToString();

        spellInfo.SetActive(true);
    }

    public void HideSpellInformation()
    {
        spellInfo.SetActive(false);
    }

    public void ShowCharaInformation(RuntimeBattleCharacter newChara)
    {
        charaInfo.SetNewChara(newChara);
    }

    public void HideCharaInformation()
    {
        charaInfo.Hide();
    }

    public void UpdatePossibleAction()
    {
        currentChara.GetCharacterDatas().GetPossibleActions(out int possibleAction, out int possibleAttack);

        for (int i = 0; i < possibleAction; i++)
        {
            if (i < actionPoint.Count)
            {
                actionPoint[i].SetActive(true);
            }
        }
        for (int i = 0; i < possibleAttack; i++)
        {
            if (i < baseActionPoint.Count)
            {
                baseActionPoint[i].SetActive(true);
            }
        }
    }

    public bool IsAskingSpell()
    {
        return maanaSpentParent.activeSelf;
    }

    public void ShowMaanaSpentAsker(Vector2 position, int minUse, int maxUse)
    {
        int currentUse = minUse;
        for(int i = 0; i < maanaToUseText.Count; i++)
        {
            if(currentUse <= maxUse)
            {
                maanaToUseText[i].text = currentUse.ToString();
                maanaToUseText[i].transform.parent.gameObject.SetActive(true);
            }
            else
            {
                maanaToUseText[i].transform.parent.gameObject.SetActive(false);
            }
            currentUse++;
        }
        Vector2 newPos = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        maanaSpentParent.transform.position = newPos;
        maanaSpentParent.SetActive(true);
    }

    public void HideMaanaSpentAsker()
    {
        maanaSpentParent.SetActive(false);
    }

    /// <summary>
    /// Called in editor (Button)
    /// </summary>
    /// <param name="usedMaanaIndex">Index of the button</param>
    public void UseSpell(int usedMaanaIndex)
    {
        maanaSpentParent.SetActive(false);
        PlayerBattleManager.instance.UseSpell(usedMaanaIndex);
    }

    public void UseActionFeedback(bool useActionPoint)
    {
        if(useActionPoint)
        {
            for (int i = actionPoint.Count - 1; i >= 0; i--)
            {
                if (actionPoint[i].activeSelf)
                {
                    actionPoint[i].SetActive(false);
                    break;
                }
            }
        }
        else
        {
            for (int i = baseActionPoint.Count - 1; i >= 0; i--)
            {
                if (baseActionPoint[i].activeSelf)
                {
                    baseActionPoint[i].SetActive(false);
                    break;
                }
            }
        }
    }

    public void DisplayErrorMessage(string msg)
    {
        errorTxt.text = msg;
        errorFeedbackEvt.Invoke();
    }

    public void HighlightCharaByTurn(int index)
    {
        BattleManager.instance.GetAllChara()[(index + currentIndex) % BattleManager.instance.GetAllChara().Count].SetHighlight(true);
    }

    public void EndHighlightChara(int index)
    {
        BattleManager.instance.GetAllChara()[(index + currentIndex) % BattleManager.instance.GetAllChara().Count].SetHighlight(false);
    }
}
