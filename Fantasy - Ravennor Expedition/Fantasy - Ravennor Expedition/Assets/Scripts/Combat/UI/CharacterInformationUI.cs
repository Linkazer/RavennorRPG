using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterInformationUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nomChara;

    [SerializeField]
    private Image spriteChara;

    [SerializeField]
    private Transform effectGrid;
    [SerializeField]
    private List<Image> effectsOnChara;
    [SerializeField]
    private List<TextMeshProUGUI> effectTimes;
    [SerializeField]
    private TextMeshProUGUI health, movespeed, defense, melee, distance, magical;

    [SerializeField]
    private GameObject effectResume;

    [SerializeField]
    private TextMeshProUGUI effectName, effectDetail;

    private List<string> effectDescriptions, effectNames;

    [ContextMenu("Set Images Effects")]
    void SetEffectImage()
    {
        effectsOnChara = new List<Image>();
        foreach (Transform child in effectGrid)
        {
            effectsOnChara.Add(child.GetComponent<Image>());
        }
    }

    private void Start()
    {
        Hide();
    }

    public void SetNewChara(RuntimeBattleCharacter chara)
    {
        if(gameObject.activeSelf)
        {
            Hide();
        }

        gameObject.SetActive(true);

        PersonnageScriptables p = chara.GetCharacterDatas();
        nomChara.text = p.nom;
        spriteChara.sprite = p.spritePerso;

        health.text = chara.GetCurrentHps() + "/" + p.GetMaxHps();
        movespeed.text = p.GetMovementSpeed().ToString();
        defense.text = p.GetBrutDefense().ToString();
        melee.text = p.GetTouchDices(1).ToString() + "D6";
        distance.text = p.GetTouchDices(2).ToString() + "D6";
        magical.text = p.GetTouchDices(3).ToString() + "D6";

        effectDescriptions = new List<string>();
        effectNames = new List<string>();
        int i = 0;
        foreach(RuntimeSpellEffect runEff in chara.GetAppliedEffects())
        {
            effectNames.Add(runEff.effet.nom);
            effectDescriptions.Add(runEff.effet.description);
            effectsOnChara[i].sprite = runEff.effet.spr;
            effectsOnChara[i].transform.parent.gameObject.SetActive(true);

            effectTimes[i].text = runEff.currentCooldown>=0 ? runEff.currentCooldown.ToString() : "";
        }
    }

    public void Hide()
    {
        HideEffect();
        foreach(Image i in effectsOnChara)
        {
            i.transform.parent.gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }

    public void ShowEffect(int index)
    {
        effectName.text = effectNames[index];
        effectDetail.text = effectDescriptions[index];
        effectResume.SetActive(true);
    }

    public void HideEffect()
    {
        effectResume.SetActive(false);
    }
}
