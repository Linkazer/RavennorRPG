using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CampManager : MonoBehaviour
{
    [SerializeField]
    private GameObject characterSheet;

    [SerializeField]
    private List<Image> characterSprites;

    [SerializeField]
    private Color nonSelectedColor;

    [Header("Character Sheet")]
    [SerializeField] private TextMeshProUGUI maxHps;
    [SerializeField] private TextMeshProUGUI maxMaana;
    [SerializeField] private TextMeshProUGUI hitDice;
    [SerializeField] private TextMeshProUGUI physicalPower;
    [SerializeField] private TextMeshProUGUI magicalPower;
    [SerializeField] private TextMeshProUGUI defense;
    [SerializeField] private TextMeshProUGUI movement;
    [SerializeField] private TextMeshProUGUI armurePhy;
    [SerializeField] private TextMeshProUGUI armureMag;
    [SerializeField] private TextMeshProUGUI chanceCrit;
    [SerializeField] private TextMeshProUGUI multCrit;

    [SerializeField]
    private Image characterPortrait;
    [SerializeField]
    private TextMeshProUGUI characterName, characterDescription;

    [SerializeField]
    private List<PersonnageScriptables> persos = new List<PersonnageScriptables>();
    private PersonnageScriptables currentChara;

    [Header("Spell Management")]
    [SerializeField]
    private List<Image> knownSpells = new List<Image>();
    [SerializeField]
    private GameObject spellInfo;
    [SerializeField]
    private TextMeshProUGUI spellNom, spellDescription, spellMaana, spellIncantation, spellPortee;

    [Header("Level Up")]
    [SerializeField] private GameObject levelUpParent;

    [SerializeField] private Image levelUpcharacterPortrait;
    [SerializeField] private TextMeshProUGUI levelUpCharacterName;

    [SerializeField] private TextMeshProUGUI levelUpStats;
    [SerializeField] private Image levelUpSpellsIcon;
    [SerializeField] private TextMeshProUGUI levelUpSpells;
    [SerializeField] private Image levelUpCapacitiesIcon;
    [SerializeField] private TextMeshProUGUI levelUpCapacities;

    [SerializeField] private ParcheminDialogueSystem dialogSysteme;

    private LevelUpCapacity capacityUpShowed;
    private CharacterActionScriptable spellUpShowed;

    [SerializeField]
    private GameObject capacityInfo;
    [SerializeField]
    private TextMeshProUGUI capacityNom, capacityDescription;

    private void Start()
    {
        foreach (PersonnageScriptables perso in RavenorGameManager.instance.playerPersos)
        {
            if (RavenorGameManager.instance.GetBattle().GetComponent<RoomManager>().characterInCamp.Contains(perso.nom))
            {
                persos.Add(perso);
            }
        }

        for(int i = 0; i < characterSprites.Count; i++)
        {
            if (!GetCharactersId().Contains(i))
            {
                characterSprites[i].gameObject.SetActive(false);
            }
        }

        if (RavenorGameManager.instance.dialogueToDisplay != null)
        {
            dialogSysteme.ShowStory(RavenorGameManager.instance.dialogueToDisplay);
            RavenorGameManager.instance.dialogueToDisplay = null;

            dialogSysteme.EndDialogueEvent.AddListener(EndDialogue);
        }
        else
        {
            CheckForNewLevelUp();
        }

        LoadingScreenManager.instance.HideScreen();
    }

    private void CheckForNewLevelUp()
    {
        int newLevelUp = RavenorGameManager.instance.GetNextLevelUp();
        if (newLevelUp >= 0)
        {
            LevelUpCharacter(newLevelUp);
        }
    }

    public void GoToNextScene()
    {
        RavenorGameManager.instance.LoadBattle();
    }

    #region Level Up
    private void LevelUpCharacter(int characterIndex)
    {
        LevelTable table = persos[characterIndex].levelUpTable.GetLevelTable(persos[characterIndex].GetLevel);

        string statDisplay = "";

        for(int i = 0; i < table.stats.Count; i++)
        {
            persos[characterIndex].LevelUpStat(table.stats[i].value, table.stats[i].stat);
            statDisplay += table.stats[i].stat.ToString() + " + " + table.stats[i].value.ToString() + "\n";
        }

        levelUpStats.text = statDisplay;

        for(int i = 0; i < table.possibleSpells.Count; i++)
        {
            persos[characterIndex].knownSpells.Add(table.possibleSpells[i]);
        }

        for(int i = 0; i < table.capacities.Count; i++)
        {
            persos[characterIndex].passifs.Add(table.capacities[i].passif);
        }

        persos[characterIndex].UpLevel();

        levelUpParent.SetActive(true);
    }

    public void ShowCapacityLevelUp()
    {
        capacityNom.text = capacityUpShowed.nom;
        capacityDescription.text = capacityUpShowed.description;

        capacityInfo.SetActive(true);
    }

    public void HideCapacity()
    {
        capacityInfo.SetActive(false);
    }

    public void ShowSpellLevelUp()
    {
        SpellInfo(spellUpShowed);
    }

    public void QuitLevelUp()
    {
        levelUpParent.SetActive(false);
        CheckForNewLevelUp();
    }
    #endregion

    #region Feedbacks
    public void Outline(Image matToChange)
    {
        //matToChange.material.color = Color.white;
    }

    public void HideOutline(Image matToChange)
    {
        //matToChange.material.color = Color.black;
    }
    #endregion

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

        hitDice.text = currentChara.GetHitDice().ToString() + "D6 + " + currentChara.GetHitBonus().ToString();
        physicalPower.text = currentChara.GetPhysicalDamage().ToString();
        magicalPower.text = currentChara.GetMagicalDamage().ToString();

        defense.text = currentChara.GetDefense().ToString();
        movement.text = currentChara.GetMovementSpeed().ToString();
        armurePhy.text = currentChara.GetPhysicalArmor().ToString();
        armureMag.text = currentChara.GetMagicalArmor().ToString();
        chanceCrit.text = currentChara.GetCriticalChanceBonus().ToString();
        multCrit.text = currentChara.GetCriticalDamageMultiplier().ToString();

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
        foreach(PersonnageScriptables perso in persos)
        {
            switch(perso.nom)
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
                case "Lila":
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
                return "Lila";
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
            case "Lila":
                return 5;
        }
        return -1;
    }

    public void EndDialogue()
    {
        CheckForNewLevelUp();
    }
}
