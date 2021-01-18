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
    private List<TextMeshProUGUI> secondaryStats; //Mélée Touche; Mélée dégât; Dist touche; Dist dégâts; Mag touche; Mag dégât; Défense; Déplacement; Initiative; Armure Phy; Armure Mag; Chance crit; Mult Crit
    [SerializeField]
    private TextMeshProUGUI meleeTouch, meleeDamage, distTouch, distDamage, magTouch, magDamage, defense, deplacement, initiative, armurePhy, armureMag, chanceCrit, multCrit;

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

    private void Start()
    {
        persos = RavenorGameManager.instance.playerPersos;
    }

    public void GoToNextScene()
    {
        RavenorGameManager.instance.LoadBattle();
    }

    public void Outline(Image matToChange)
    {
        //matToChange.material.color = Color.white;
    }

    public void HideOutline(Image matToChange)
    {
        //matToChange.material.color = Color.black;
    }

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

    public void AddStat(int index)
    {
        currentChara.LevelUpStat(index);
        SetCharacterSheet();
    }

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
            spellNom.text = currentChara.learnedSpells[index].nom;
            spellDescription.text = currentChara.learnedSpells[index].description;
            spellMaana.text = currentChara.learnedSpells[index].maanaCost.ToString();
            spellIncantation.text = currentChara.learnedSpells[index].incantationTime.ToString();
            spellPortee.text = currentChara.learnedSpells[index].range.y.ToString();

            spellInfo.SetActive(true);
        }
    }

    public void ShowUseSpellInfo(int index)
    {
        index++;
        if (currentChara != null && currentChara.sortsDisponibles.Count > index && currentChara.sortsDisponibles[index] != null)
        {
            spellNom.text = currentChara.sortsDisponibles[index].nom;
            spellDescription.text = currentChara.sortsDisponibles[index].description;
            spellMaana.text = currentChara.sortsDisponibles[index].maanaCost.ToString();
            spellIncantation.text = currentChara.sortsDisponibles[index].incantationTime.ToString();
            spellPortee.text = currentChara.sortsDisponibles[index].range.y.ToString();

            spellInfo.SetActive(true);
        }
    }

    public void HideSpellInfo()
    {
        spellInfo.SetActive(false);
    }

    public void CloseSpellManager()
    {
        spellManagementParent.SetActive(false);
    }

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
}
