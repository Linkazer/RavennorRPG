using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectionHandler : MonoBehaviour
{
    [Header("Informations")]
    [SerializeField] private StoryLevelInformation level;

    [Header("UI Gestion")]
    [SerializeField] private MainMenuUIManager manager;
    [SerializeField] private TextMeshProUGUI levelName;
    [SerializeField] private Button button;

    public void SelectLevel()
    {
        manager.SelectLevel(level);
    }

    public void EnableButton()
    {
        button.interactable = true;
        levelName.text = level.nom;
    }

    public void DisableButton()
    {
        button.interactable = false;
        levelName.text = "???";
    }

    public string GetLevelID => level.ID;
}
