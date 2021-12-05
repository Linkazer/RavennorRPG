using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [Header("Level Choice")]
    [SerializeField] private GameObject levelMenu;
    [SerializeField] private TextMeshProUGUI levelName;
    [SerializeField] private TextMeshProUGUI levelDescription;
    [SerializeField] private TextMeshProUGUI levelWinCondition;
    [SerializeField] private TextMeshProUGUI levelLoseCondition;
    [SerializeField] private List<LevelCharacterButton> characterButtons;
    [SerializeField] private GameObject levelSelectedDisplay;
    [SerializeField] private List<LevelSelectionHandler> selectableLevelButton;

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

    private List<PersonnageScriptables> persos = new List<PersonnageScriptables>();
    private PersonnageScriptables currentChara;

    [Header("Spell Management")]
    [SerializeField]
    private List<Image> knownSpells = new List<Image>();
    [SerializeField]
    private GameObject spellInfo;
    [SerializeField]
    private TextMeshProUGUI spellNom, spellDescription, spellMaana, spellIncantation, spellPortee;

    private void Start()
    {
        for(int i = 0; i < selectableLevelButton.Count; i++)
        {
            if (SaveManager.DoesLevelExist(selectableLevelButton[i].GetLevelID))
            {
                selectableLevelButton[i].EnableButton();
            }
            else
            {
                selectableLevelButton[i].DisableButton();
            }
        }

        LoadingScreenManager.instance.HideScreen();
    }

    public void OpenLevelMenu()
    {
        levelMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void CloseLevelMenu()
    {
        levelMenu.SetActive(false);
        mainMenu.SetActive(true);
    }


    public void Play()
    {
        RavenorGameManager.instance.LoadBattle();
    }

    public void PlayTuto()
    {
        RavenorGameManager.instance.LoadTuto();
    }

    #region Level Selection
    public void SelectLevel(StoryLevelInformation newBattle)
    {
        RavenorGameManager.SetNextBattle(newBattle.levelPrefab);
        DisplayLevelInformation(newBattle);

    }

    private void DisplayLevelInformation(StoryLevelInformation chosenLevel)
    {
        levelName.text = chosenLevel.nom;
        levelDescription.text = chosenLevel.description;
        levelWinCondition.text = chosenLevel.winCondition;
        levelLoseCondition.text = chosenLevel.looseCondition;

        persos = chosenLevel.charactersInLevel;

        for (int i = 0; i < characterButtons.Count; i++)
        {
            if(i < chosenLevel.charactersInLevel.Count)
            {
                characterButtons[i].Enable(chosenLevel.charactersInLevel[i]);
            }
            else
            {
                characterButtons[i].Disable();
            }
        }
        levelSelectedDisplay.SetActive(true);
    }
    #endregion

    #region CharacterSheet
    public void OpenCharacterSheet(int index)
    {
        CloseCharacterSheet();

        currentChara = persos[index];

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
        power.text = currentChara.GetPower().ToString();

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

}
