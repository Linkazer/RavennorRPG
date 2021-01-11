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
    private GameObject effectResume;

    [SerializeField]
    private TextMeshProUGUI effectDetail;

    private List<string> effectDescriptions;

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

        effectDescriptions = new List<string>();
        int i = 0;
        foreach(RuntimeSpellEffect runEff in chara.GetAppliedEffects())
        {
            effectDescriptions.Add(runEff.effet.description);
            effectsOnChara[i].sprite = runEff.effet.spr;
            effectsOnChara[i].gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        HideEffect();
        foreach(Image i in effectsOnChara)
        {
            i.gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }

    public void ShowEffect(int index)
    {
        effectDetail.text = effectDescriptions[index];
        effectResume.SetActive(true);
    }

    public void HideEffect()
    {
        effectResume.SetActive(false);
    }
}
