using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelCharacterButton : MonoBehaviour
{
    [SerializeField] private Image charaDisplay;
    [SerializeField] private TextMeshProUGUI charaName;

    public void Enable(PersonnageScriptables newPerso)
    {
        charaDisplay.sprite = newPerso.spritePerso;
        charaName.text = newPerso.nom;

        gameObject.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}
