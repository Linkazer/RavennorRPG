using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CampManager : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField]
    private List<Image> characterSprites;
    [SerializeField]
    private List<RectTransform> characterTransforms;

    [SerializeField]
    private Color nonSelectedColor;

    [Header("Character Sheet")]
    [SerializeField]
    private GameObject characterSheet;

    [SerializeField] private TextMeshProUGUI maxHps;
    [SerializeField] private TextMeshProUGUI maxMaana;
    [SerializeField] private TextMeshProUGUI accuracy;
    [SerializeField] private TextMeshProUGUI power;
    [SerializeField] private TextMeshProUGUI defense;
    [SerializeField] private TextMeshProUGUI movement;
    [SerializeField] private TextMeshProUGUI armor;

    [SerializeField]
    private Image characterPortrait;
    [SerializeField]
    private TextMeshProUGUI characterName, characterDescription;

    [SerializeField]
    private List<PersonnageScriptables> persos = new List<PersonnageScriptables>();
    private List<CampDisplayableCharacter> displayableCharas = new List<CampDisplayableCharacter>();
    private PersonnageScriptables currentChara;

    [Header("Spell Management")]
    [SerializeField]
    private List<Image> knownSpells = new List<Image>();
    [SerializeField]
    private GameObject spellInfo;
    [SerializeField]
    private TextMeshProUGUI spellNom, spellDescription, spellMaana, spellIncantation, spellPortee;

    [Header("Dialogue")]
    [SerializeField] private ParcheminDialogueSystem dialogSysteme;

    private LevelUpCapacity capacityUpShowed;
    private CharacterActionScriptable spellUpShowed;

    [Header("Capacities")]
    [SerializeField]
    private GameObject capacityInfo;
    [SerializeField]
    private TextMeshProUGUI capacityNom, capacityDescription;

    private void Start()
    {
        List<CampDisplayableCharacter> displays = RavenorGameManager.instance.GetCurrentBattle().characterInCamp;
        for (int i = 0; i < displays.Count; i++)
        {
            if (displays[i].Scriptable != null)
            {
                persos.Add(displays[i].Scriptable);
            }
            displayableCharas.Add(displays[i]);
        }

        background.sprite = RavenorGameManager.instance.GetCurrentBattle().backgroundCamp;

        for (int i = 0; i < displayableCharas.Count; i++)
        {
            Image displayImage = characterSprites[GetCharaIndexByName(displayableCharas[i].ID)];
            displayImage.gameObject.SetActive(true);
            displayImage.sprite = displayableCharas[i].Sprite;
            characterTransforms[i].localPosition = displayableCharas[i].Position;
            characterTransforms[i].localScale = displayableCharas[i].Scale;
        }

        if (RavenorGameManager.instance.dialogueToDisplay != null)
        {
            dialogSysteme.ShowStory(RavenorGameManager.instance.dialogueToDisplay);
            RavenorGameManager.instance.dialogueToDisplay = null;

            dialogSysteme.EndDialogueEvent.AddListener(EndDialogue);
        }

        LoadingScreenManager.instance.HideScreen();
    }

    public void GoToNextScene()
    {
        RavenorGameManager.instance.LoadBattle();
    }

    #region CharacterSheet
    public void OpenCharacterSheet(int index)
    {
        CloseCharacterSheet();

        int charaIndex = 0;
        for (int i = 0; i < persos.Count; i++)
        {
            if(persos[i].nom == GetCharaNameByIndex(index))
            {
                charaIndex = i;
                break;
            }
        }

        currentChara = persos[charaIndex];
        characterSprites[index].color = Color.white;
        for (int i = 0; i < characterSprites.Count; i++)
        {
            if (i != index)
            {
                characterSprites[i].color = nonSelectedColor;
            }
        }

        SetCharacterSheet();
    }

    public void SetCharacterSheet()
    {
        characterPortrait.sprite = currentChara.icon;
        characterName.text = currentChara.nom;

        maxHps.text = currentChara.GetMaxHps().ToString();
        maxMaana.text = currentChara.GetMaxMaana().ToString();

        characterDescription.text = currentChara.description;

        accuracy.text = currentChara.GetAccuracy().ToString();
        power.text = currentChara.GetPhysicalDamage().ToString();

        defense.text = currentChara.GetDefense().ToString();
        movement.text = currentChara.GetMovementSpeed().ToString();
        armor.text = currentChara.GetArmor().ToString();

        for (int i = 0; i < knownSpells.Count; i++)
        {
            if (currentChara.knownSpells.Count > i && currentChara.knownSpells[i] != null)
            {
                knownSpells[i].sprite = currentChara.knownSpells[i].icon;
                knownSpells[i].transform.parent.gameObject.SetActive(true);
            }
            else
            {
                knownSpells[i].transform.parent.gameObject.SetActive(false);
            }
        }

        characterSheet.SetActive(true);
    }
    #endregion

    #region Spell Gestion
    public void CloseCharacterSheet()
    {
        characterSheet.SetActive(false);
        ClosePannel();
        currentChara = null;
    }

    public void ShowUseSpellInfo(int index)
    {
        if (currentChara != null && currentChara.knownSpells.Count > index && currentChara.knownSpells[index] != null)
        {
            SpellInfo(currentChara.knownSpells[index]);
        }
    }

    public void SpellInfo(CharacterActionScriptable toShow)
    {
        spellNom.text = toShow.nom;
        spellDescription.text = toShow.description;
        spellMaana.text = toShow.maanaCost.ToString();
        spellIncantation.text = toShow.incantationTime.ToString();
        spellPortee.text = toShow.range.ToString();

        spellInfo.SetActive(true);
    }

    public void HideSpellInfo()
    {
        spellInfo.SetActive(false);
    }
    #endregion

    public void ClosePannel()
    {
        for (int i = 0; i < characterSprites.Count; i++)
        {
            characterSprites[i].color = Color.white;
        }
    }

    private List<int> GetCharactersId()
    {
        List<int> toReturn = new List<int>();
        for(int i = 0; i < displayableCharas.Count; i++)
        {
            switch (displayableCharas[i].ID)
            {
                case "Eliza":
                    toReturn.Add(0);
                    break;
                case "Nor":
                    toReturn.Add(1);
                    break;
                case "Okun":
                    toReturn.Add(2);
                    break;
                case "Shedun":
                    toReturn.Add(3);
                    break;
                case "Vanyaenn":
                    toReturn.Add(4);
                    break;
                case "Free 1":
                    toReturn.Add(5);
                    break;
            }
        }
        return toReturn;
    }

    private string GetCharaNameByIndex(int index)
    {
        switch (index)
        {
            case 0:
                return "Eliza";
            case 1:
                return "Nor";
            case 2:
                return "Okun";
            case 3:
                return "Shedun";
            case 4:
                return "Vanyaenn";
            case 5:
                return "Free 1";
        }
        return "";
    }

    private int GetCharaIndexByName(string wantedName)
    {
        switch (wantedName)
        {
            case "Eliza":
                return 0;
            case "Nor":
                return 1;
            case "Okun":
                return 2;
            case "Shedun":
                return 3;
            case "Vanyaenn":
                return 4;
            case "Free 1":
                return 5;
        }
        return -1;
    }

    public void EndDialogue()
    {
        
    }
}
