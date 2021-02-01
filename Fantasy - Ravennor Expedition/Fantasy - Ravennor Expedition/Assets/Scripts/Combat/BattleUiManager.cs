using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUiManager : MonoBehaviour
{
    public static BattleUiManager instance;

    [SerializeField]
    private GameObject mainBattleUI;

    [SerializeField]
    private List<Image> turnImages;

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

    private int currentMaxHps;

    [SerializeField]
    private ParcheminDialogueSystem parcheminStory;
    [SerializeField]
    private CharacterInformationUI charaInfo;

    [SerializeField]
    private GameObject winScreen, looseScreen;

    [SerializeField]
    private GameObject spellInfo;
    [SerializeField]
    private TextMeshProUGUI spellTitle, spellDescription, spellMaanaCost;

    private RuntimeBattleCharacter currentChara;
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

    public void SetNewTurn(int turnIndex, List<RuntimeBattleCharacter> roundList)
    {
        int offset = 0;
        int currentPerso = turnIndex;

        currentChara = roundList[turnIndex];

        for(int i = 0; i < turnImages.Count; i++)
        {
            Color newCol = Color.white;
            newCol.a = 0;

            //if (turnIndex + offset < roundList.Count)
            {
                if (roundList[turnIndex + offset].GetCurrentHps() > 0)
                {
                    turnImages[i].sprite = roundList[turnIndex + offset].GetCharacterDatas().icon;
                    turnImages[i].color = Color.white;

                    turnIndex++;
                    turnIndex = turnIndex % roundList.Count;
                }
                /*else
                {
                    //offset++;
                    //i--;
                }*/
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
    }

    public void UpdateSpells()
    {
        Color newCol = Color.black;
        newCol.r = 0.5f;
        newCol.g = 0.5f;
        newCol.b = 0.5f;

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

    public void TestFonct()
    {
        Debug.Log("Yess");
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
}
