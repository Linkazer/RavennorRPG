using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CampManager : MonoBehaviour
{
    [SerializeField]
    private GameObject characterSheet, characterSpells;

    [SerializeField]
    private List<Image> characterSprites;

    [SerializeField]
    private Color nonSelectedColor;

    [Header("Character Sheet")]
    [SerializeField]
    private List<TextMeshProUGUI> mainStats; //Force; Constitution; Agilité; Intel; Perception; Flux Magique
    [SerializeField]
    private TextMeshProUGUI maxHps, maxMaana, meleeTouch, meleeDamage, distTouch, distDamage, magTouch, magDamage, defense, deplacement, initiative, armurePhy, armureMag, chanceCrit, multCrit;

    [SerializeField]
    private Image characterPortrait;
    [SerializeField]
    private TextMeshProUGUI characterName;

    private List<PersonnageScriptables> persos;
    private PersonnageScriptables currentChara;

    [Header("Spell Management")]
    [SerializeField]
    private GameObject spellManagementParent;
    [SerializeField]
    private List<Image> knownSpells = new List<Image>(), usedSpells = new List<Image>();
    [SerializeField]
    private GameObject spellInfo;
    [SerializeField]
    private TextMeshProUGUI spellNom, spellDescription, spellMaana, spellIncantation, spellPortee;

    [Header("Level Up")]
    [SerializeField]
    private GameObject levelUpParent;
    //Chara info
    [SerializeField]
    private Image lvlPortrait;
    [SerializeField]
    private TextMeshProUGUI lvlName;
    //Stats
    [SerializeField]
    private GameObject levelUpStatParent;
    [SerializeField]
    private List<TextMeshProUGUI> lvlMainStats; //Force; Constitution; Agilité; Intel; Perception; Flux Magique
    [SerializeField]
    private TextMeshProUGUI lvlMaxHps, lvlMaxMaana, lvlMeleeTouch, lvlMeleeDamage, lvlDistTouch, lvlDistDamage, lvlMagTouch, lvlMagDamage, lvlDefense, lvlDeplacement, lvlInitiative, lvlArmurePhy, lvlArmureMag, lvlChanceCrit, lvlMultCrit;
    private int statToUp = -1;
    //Spells
    [SerializeField]
    private GameObject levelUpSpellParent;
    [SerializeField]
    private List<Image> imgUnlockSpells = new List<Image>(), lvlKnownSpells = new List<Image>();
    [SerializeField]
    private Image imgWantedNewSpell;
    private CharacterActionScriptable wantedNewSpell;
    private List<CharacterActionScriptable> unlockableSpells;

    [SerializeField]
    private ParcheminDialogueSystem dialogSysteme;

    private void Start()
    {
        persos = RavenorGameManager.instance.playerPersos;

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
    }

    private void CheckForNewLevelUp()
    {
        int newLevelUp = RavenorGameManager.instance.GetNextLevelUp();
        if (newLevelUp >= 0)
        {
            OpenLevelUpTable(newLevelUp);
        }
    }

    public void GoToNextScene()
    {
        RavenorGameManager.instance.LoadBattle();
    }

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

        currentChara = persos[index];
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

        List<int> mainStatsValues = currentChara.GetAllMainStats();

        for (int i = 0; i < mainStats.Count; i++)
        {
            mainStats[i].text = mainStatsValues[i].ToString();
        }

        //Melee
        meleeTouch.text = currentChara.GetTouchDices(1).ToString() + "D6 + " + currentChara.GetBrutToucheMelee();
        string dicePrint = "";
        foreach (Dice d in currentChara.GetBonusDice(EffectType.PhysicalMeleDamage))
        {
            dicePrint += d.numberOfDice.ToString() + d.wantedDice + " + ";
        }
        dicePrint += currentChara.GetPhysicalDamageMelee().ToString();
        meleeDamage.text = dicePrint;

        //Distance
        distTouch.text = currentChara.GetTouchDices(2).ToString() + "D6 + " + currentChara.GetBrutToucheDistance();
        dicePrint = "";
        foreach (Dice d in currentChara.GetBonusDice(EffectType.PhysicalMeleDamage))
        {
            dicePrint += d.numberOfDice.ToString() + d.wantedDice + " + ";
        }
        dicePrint += currentChara.GetPhysicalDamageDistance().ToString();
        distDamage.text = dicePrint;

        //Magical
        magTouch.text = currentChara.GetTouchDices(3).ToString() + "D6 + " + currentChara.GetBrutToucheMagical();
        dicePrint = "";
        foreach (Dice d in currentChara.GetBonusDice(EffectType.MagicalDamage))
        {
            dicePrint += d.numberOfDice.ToString() + d.wantedDice + " + ";
        }
        dicePrint += currentChara.GetMagicalDamage().ToString();
        magDamage.text = dicePrint;

        defense.text = currentChara.GetBrutDefense().ToString();
        deplacement.text = currentChara.GetMovementSpeed().ToString();
        initiative.text = currentChara.GetInitiative().ToString() + "D6";
        armurePhy.text = currentChara.GetPhysicalArmor().ToString();
        armureMag.text = currentChara.GetMagicalArmor().ToString();
        chanceCrit.text = currentChara.GetCriticalChanceBonus().ToString();
        multCrit.text = currentChara.GetCriticalDamageMultiplier().ToString();

        //Level Up
        if (currentChara.GetStatPoint() > 0)
        {

        }

        characterSheet.SetActive(true);
    }
    #endregion

    #region Spell Gestion
    public void AddSpell(CharacterActionScriptable newSpell)
    {
        if (!currentChara.learnedSpells.Contains(newSpell))
        {
            currentChara.learnedSpells.Add(newSpell);
        }
    }

    public void CloseCharacterSheet()
    {
        characterSheet.SetActive(false);
        CloseSpellManager();
        ClosePannel();
        currentChara = null;
    }

    public void OpenCharacterSpells()
    {
        if (currentChara != null)
        {
            for (int i = 0; i < knownSpells.Count; i++)
            {
                if (currentChara.learnedSpells.Count > i && currentChara.learnedSpells[i] != null)
                {
                    knownSpells[i].sprite = currentChara.learnedSpells[i].icon;
                    knownSpells[i].gameObject.SetActive(true);
                }
                else
                {
                    knownSpells[i].gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < usedSpells.Count; i++)
            {
                if (currentChara.sortsDisponibles.Count > i+1 && currentChara.sortsDisponibles[i+1] != null)
                {
                    usedSpells[i].sprite = currentChara.sortsDisponibles[i+1].icon;
                    usedSpells[i].gameObject.SetActive(true);
                }
                else
                {
                    usedSpells[i].gameObject.SetActive(false);
                }
            }

            spellManagementParent.SetActive(true);
        }
    }

    public void AddSpell(int index)
    {
        if (currentChara != null && index < currentChara.learnedSpells.Count && currentChara.sortsDisponibles.Count < currentChara.GetMemory() && currentChara.learnedSpells[index] != null && !currentChara.sortsDisponibles.Contains(currentChara.learnedSpells[index]))
        {
            currentChara.sortsDisponibles.Add(currentChara.learnedSpells[index]);
            OpenCharacterSpells();
        }
    }

    public void RemoveSpell(int spellIndex)
    {
        spellIndex++;
        if (currentChara != null && currentChara.sortsDisponibles.Count > spellIndex && currentChara.sortsDisponibles[spellIndex] != null)
        {
            currentChara.sortsDisponibles.RemoveAt(spellIndex);
            OpenCharacterSpells();
        }
    }

    public void ShowSpellInfo(int index)
    {
        if (currentChara != null && currentChara.learnedSpells.Count > index && currentChara.learnedSpells[index] != null)
        {
            SpellInfo(currentChara.learnedSpells[index]);
        }
    }

    public void ShowUseSpellInfo(int index)
    {
        index++;
        if (currentChara != null && currentChara.sortsDisponibles.Count > index && currentChara.sortsDisponibles[index] != null)
        {
            SpellInfo(currentChara.sortsDisponibles[index]);
        }
    }

    public void ShowUnlockableSpellInfo(int index)
    {
        index++;
        if (currentChara != null && currentChara.sortsDisponibles.Count > index && currentChara.sortsDisponibles[index] != null)
        {
            SpellInfo(currentChara.sortsDisponibles[index]);
        }
    }

    public void SpellInfo(CharacterActionScriptable toShow)
    {
        spellNom.text = toShow.nom;
        spellDescription.text = toShow.description;
        spellMaana.text = toShow.maanaCost.ToString();
        spellIncantation.text = toShow.incantationTime.ToString();
        spellPortee.text = toShow.range.y.ToString();

        spellInfo.SetActive(true);
    }

    public void HideSpellInfo()
    {
        spellInfo.SetActive(false);
    }

    public void CloseSpellManager()
    {
        spellManagementParent.SetActive(false);
    }
    #endregion

    #region Level Up
    public void OpenLevelUpTable(int index)
    {
        currentChara = persos[index];

        unlockableSpells = new List<CharacterActionScriptable>();

        foreach(LevelTable table in currentChara.levelUpTable.GetUsableTables(currentChara.level))
        {
            foreach(CharacterActionScriptable act in table.possibleSpells)
            {
                if(!currentChara.learnedSpells.Contains(act))
                {
                    unlockableSpells.Add(act);
                }
            }
        }

        lvlPortrait.sprite = currentChara.portrait;
        lvlName.text = currentChara.nom;

        statToUp = -1;

        wantedNewSpell = null;

        SetLevepUpSheet();

        #region Spell Unlock
        Color hidingColor = Color.black;
        hidingColor.a = 0;
        for(int i = 0; i < imgUnlockSpells.Count; i++)
        {
            if(i < unlockableSpells.Count)
            {
                imgUnlockSpells[i].sprite = unlockableSpells[i].icon;
                imgUnlockSpells[i].color = Color.white;
            }
            else
            {
                imgUnlockSpells[i].color = hidingColor;
            }
        }

        for (int i = 0; i < lvlKnownSpells.Count; i++)
        {
            if (i < currentChara.learnedSpells.Count)
            {
                lvlKnownSpells[i].sprite = currentChara.learnedSpells[i].icon;
                lvlKnownSpells[i].color = Color.white;
            }
            else
            {
                lvlKnownSpells[i].color = hidingColor;
            }
        }
        #endregion

        levelUpParent.SetActive(true);
    }

    public void SetLevepUpSheet()
    {
        lvlMaxHps.text = currentChara.GetMaxHps().ToString();
        lvlMaxMaana.text = currentChara.GetMaxMaana().ToString();

        List<int> mainStatsValues = currentChara.GetAllMainStats();

        for (int i = 0; i < mainStats.Count; i++)
        {
            lvlMainStats[i].text = mainStatsValues[i].ToString();
        }

        //Melee
        lvlMeleeTouch.text = currentChara.GetTouchDices(1).ToString() + "D6 + " + currentChara.GetBrutToucheMelee();
        string dicePrint = "";
        foreach (Dice d in currentChara.GetBonusDice(EffectType.PhysicalMeleDamage))
        {
            dicePrint += d.numberOfDice.ToString() + d.wantedDice + " + ";
        }
        dicePrint += currentChara.GetPhysicalDamageMelee().ToString();
        lvlMeleeDamage.text = dicePrint;

        //Distance
        lvlDistTouch.text = currentChara.GetTouchDices(2).ToString() + "D6 + " + currentChara.GetBrutToucheDistance();
        dicePrint = "";
        foreach (Dice d in currentChara.GetBonusDice(EffectType.PhysicalMeleDamage))
        {
            dicePrint += d.numberOfDice.ToString() + d.wantedDice + " + ";
        }
        dicePrint += currentChara.GetPhysicalDamageDistance().ToString();
        lvlDistDamage.text = dicePrint;

        //Magical
        lvlMagTouch.text = currentChara.GetTouchDices(3).ToString() + "D6 + " + currentChara.GetBrutToucheMagical();
        dicePrint = "";
        foreach (Dice d in currentChara.GetBonusDice(EffectType.MagicalDamage))
        {
            dicePrint += d.numberOfDice.ToString() + d.wantedDice + " + ";
        }
        dicePrint += currentChara.GetMagicalDamage().ToString();
        lvlMagDamage.text = dicePrint;

        lvlDefense.text = currentChara.GetBrutDefense().ToString();
        lvlDeplacement.text = currentChara.GetMovementSpeed().ToString();
        lvlInitiative.text = currentChara.GetInitiative().ToString() + "D6";
        lvlArmurePhy.text = currentChara.GetPhysicalArmor().ToString();
        lvlArmureMag.text = currentChara.GetMagicalArmor().ToString();
        lvlChanceCrit.text = currentChara.GetCriticalChanceBonus().ToString();
        lvlMultCrit.text = currentChara.GetCriticalDamageMultiplier().ToString();

        levelUpStatParent.SetActive(true);
    }

    public void AddStat(int index)
    {
        if(statToUp != -1)
        {
            currentChara.RemoveUpStat(statToUp);
        }
        statToUp = index;
        currentChara.LevelUpStat(statToUp);
        SetLevepUpSheet();
    }

    public void ChooseUnlockedSpell(int index)
    {
        wantedNewSpell = unlockableSpells[index];
        imgWantedNewSpell.sprite = wantedNewSpell.icon;
    }

    public void UnlockNewSpell()
    {
        currentChara.learnedSpells.Add(wantedNewSpell);
    }

    public void ValidateNewStats()
    {
        levelUpStatParent.SetActive(false);
        if (currentChara.levelUpTable.levelUnlockSpell.Contains(currentChara.level))
        {
            levelUpSpellParent.SetActive(true);
        }
        else
        {
            CheckForNewLevelUp();
        }
    }

    public void ValidateNewSpell()
    {
        UnlockNewSpell();
        levelUpParent.SetActive(false);
        CheckForNewLevelUp();
    }
    #endregion
    public void ClosePannel()
    {
        for (int i = 0; i < characterSprites.Count; i++)
        {
            characterSprites[i].color = Color.white;
        }
    }

    public void OpenInventory()
    {

    }

    public void EndDialogue()
    {
        CheckForNewLevelUp();
    }
}
