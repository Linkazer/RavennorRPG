using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellObject : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer sprite, caseSprite;
    [SerializeField]
    private Animation spellAnimation;

    private RuntimeBattleCharacter effectCaster;

    public bool isUsed;

    private int turnLeft;

    [SerializeField]
    private AffichageSprites spriteAffichage;

    private RuntimeSpellEffect effet;

    [SerializeField]
    private AudioSource audioSource;

    public void SetSprite(Sprite newSpr, Sprite caseFeedback, int offset)
    {
        sprite.sprite = newSpr;
        caseSprite.sprite = caseFeedback;
        spriteAffichage.offset = offset;
    }

    public void SetSound(AudioClip newSound)
    {
        audioSource.clip = newSound;
        audioSource.Play();
    }

    public void SetPosition(Vector2 newPos)
    {
        transform.position = newPos;
    }

    public void AddPositionMovement(Vector3 newPos)
    {
        transform.position += newPos;
    }

    public void SetObject(Vector2 position)
    {
        isUsed = true;
        SetPosition(position);
    }

    public void SetCaster(RuntimeBattleCharacter newCaster, int turn, RuntimeSpellEffect newEffet)
    {
        effet = newEffet;
        turnLeft = turn;
        effectCaster = newCaster;
        BattleManager.TurnBeginEvent.AddListener(UpdateRound);
    }

    public void ResetObject()
    {
        transform.right = Vector2.right;
        if(effectCaster != null)
        {
            effet = null;
            effectCaster = null;
            BattleManager.TurnBeginEvent.RemoveListener(UpdateRound);
        }
        isUsed = false;
        SetPosition(new Vector2(-20, -20));
    }

    public void UpdateRound()
    {
        if (BattleManager.instance.GetCurrentTurnChara() == effectCaster)
        {
            if (Grid.instance.NodeFromWorldPoint(transform.position).HasCharacterOn)
            {
                RuntimeBattleCharacter chara = Grid.instance.NodeFromWorldPoint(transform.position).chara;

                BattleManager.instance.ResolveEffect(effet.effet, transform.position, transform.position, EffectTrigger.BeginTurn, 1);

            }

            turnLeft--;
            if (turnLeft <= 0)
            {
                ResetObject();
            }
        }

    }
}
