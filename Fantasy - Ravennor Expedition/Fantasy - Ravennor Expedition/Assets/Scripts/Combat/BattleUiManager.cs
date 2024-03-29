﻿using System.Collections;
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
    private List<Image> spellImages;
    [SerializeField]
    private List<Image> spellCooldownImages;
    [SerializeField]
    private List<TextMeshProUGUI> spellCooldownText;
    [SerializeField]
    private Image currentPersoIcon;
    [SerializeField]
    private Image currentHpImage;
    [SerializeField]
    private TextMeshProUGUI currentHpText, currentMaxHpText;
    [SerializeField]
    private TextMeshProUGUI currentMaanaText;
    [SerializeField]
    private List<GameObject> actionPoint;

    private int currentMaxHps;

    [Header("Parchemins")]
    [SerializeField]
    private ParcheminDialogueSystem parcheminStory;
    [SerializeField]
    private CharacterInformationUI charaInfo;

    [Header("Ecrans de fin")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject looseScreen;

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

    [Header("Events")]
    [SerializeField] private UnityEvent PlayOnDisplayCharacterInformation;
    [SerializeField] private UnityEvent PlayOnHideCharacterInformation;
    [SerializeField] private UnityEvent PlayOnDisplaySpellInformation;
    [SerializeField] private UnityEvent PlayOnHideSpellInformation;

    [Header("Error management")]
    [SerializeField]
    private TextMeshProUGUI errorTxt;
    [SerializeField]
    private UnityEvent errorFeedbackEvt;

    private bool isOvercharging;

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
        isOvercharging = false;

        currentMaxHps = newChara.GetCharacterDatas().GetMaxHps();
        currentMaxHpText.text = currentMaxHps.ToString();
        SetCurrentHps(newChara.GetCurrentHps());
        SetCurrentMaana(newChara.GetCurrentMaana());

        List<CharacterActionScriptable> actionList = newChara.GetActions(isOvercharging);

        currentPersoIcon.sprite = newChara.GetCharacterDatas().icon;

        for (int i = 0; i < spellImages.Count; i++)
        {
            Color newCol = Color.white;
            newCol.a = 0;
            if(i < actionList.Count)
            {
                spellImages[i].gameObject.SetActive(true);
                spellImages[i].color = Color.white;
                spellImages[i].sprite = actionList[i].icon;
            }
            else
            {
                spellImages[i].color = newCol;
                spellImages[i].gameObject.SetActive(false);
            }
        }

        UpdatePossibleAction();
    }

    public void SetOvercharge(bool nValue)
    {
        isOvercharging = nValue;

        PlayerBattleManager.instance.UpdateActionList(isOvercharging);
        UpdateSpellVisual();
    }

    public void ChangeOvercharge()
    {
        if(isOvercharging)
        {
            SetOvercharge(false);
        }
        else
        {
            SetOvercharge(true);
        }
    }

    public void UpdateSpellVisual()
    {
        List<CharacterActionScriptable> actionList = currentChara.GetActions(isOvercharging);

        for (int i = 0; i < spellImages.Count; i++)
        {
            Color newCol = Color.white;
            newCol.a = 0;
            if (i < actionList.Count)
            {
                spellImages[i].gameObject.SetActive(true);
                spellImages[i].color = Color.white;
                spellImages[i].sprite = actionList[i].icon;
            }
            else
            {
                spellImages[i].color = newCol;
                spellImages[i].gameObject.SetActive(false);
            }
        }

        UpdateSpells();
    }

    public void UpdateSpells()
    {
        List<CharacterActionScriptable> actionList = currentChara.GetActions(isOvercharging);

        for (int i = 0; i < actionList.Count; i++)
        {
            if(isOvercharging || BattleManager.instance.IsActionAvailable(currentChara, actionList[i]))
            {
                spellCooldownImages[i].gameObject.SetActive(false);
            }
            else
            {
                spellCooldownImages[i].gameObject.SetActive(true);
                spellCooldownText[i].text = "";
                int cooldown = currentChara.GetSpellCooldown(actionList[i]);
                if (cooldown != 0)
                {
                    spellCooldownText[i].text = cooldown.ToString();
                    spellCooldownImages[i].fillAmount = (float)cooldown / actionList[i].GetMaxCooldown();
                }
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

    public void WinningScreen()
    {
        winScreen.SetActive(true);
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

    public void ShowSpellInformations(int index)
    {
        PlayOnDisplaySpellInformation?.Invoke();

        CharacterActionScriptable toShow = PlayerBattleControllerManager.instance.GetSpell(index);

        spellTitle.text = toShow.nom;
        spellDescription.text = toShow.description;
        spellMaanaCost.text = toShow.maanaCost.ToString();
        spellIncantationTime.text = toShow.incantationTime.ToString();

        spellInfo.SetActive(true);
    }

    public void HideSpellInformation()
    {
        PlayOnHideSpellInformation?.Invoke();
        spellInfo.SetActive(false);
    }

    public void ShowCharaInformation(RuntimeBattleCharacter newChara)
    {
        PlayOnDisplayCharacterInformation?.Invoke();
        charaInfo.SetNewChara(newChara);
    }

    public void HideCharaInformation()
    {
        if (charaInfo.gameObject.activeSelf)
        {
            PlayOnHideCharacterInformation?.Invoke();
        }
        charaInfo.Hide();
    }

    public void UpdatePossibleAction()
    {
        int possibleAction = currentChara.GetCharacterDatas().GetPossibleActions();

        for (int i = 0; i < possibleAction; i++)
        {
            if (i < actionPoint.Count)
            {
                actionPoint[i].SetActive(true);
            }
        }
    }

    public bool IsAskingSpell()
    {
        return maanaSpentParent.activeSelf;
    }

    /// <summary>
    /// Called in editor (Button)
    /// </summary>
    public void UseSpell()
    {
        maanaSpentParent.SetActive(false);
        PlayerBattleManager.instance.UseSpell(0);
    }

    public void UseActionFeedback()
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
