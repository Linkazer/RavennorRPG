using System;
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

    private bool shouldMove = false;
    private Vector2 target;
    private Vector3 direction;
    private float speed;
    private Action movementCallback;

    private void Start()
    {
        enabled = false;
    }

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

    public void SetMovableObject(Vector2 tTarget, float tSpeed, Action callback)
    {
        movementCallback = callback;

        speed = tSpeed;
        target = tTarget;

        Vector2 currentPos = transform.position;
        direction = (target - currentPos).normalized;

        transform.up = direction;

        shouldMove = true;
    }

    public void SetObject(Vector2 position)
    {
        enabled = true;
        isUsed = true;
        SetPosition(position);
    }

    public void SetCaster(RuntimeBattleCharacter newCaster, int turn, RuntimeSpellEffect newEffet)
    {
        effet = newEffet;
        turnLeft = turn;
        effectCaster = newCaster;
        BattleManager.characterTurnBegin += UpdateRound;
    }

    public void ResetObject()
    {
        transform.right = Vector2.right;
        if(effectCaster != null)
        {
            effet = null;
            effectCaster = null;
            BattleManager.characterTurnBegin -= UpdateRound;
        }
        isUsed = false;
        SetPosition(new Vector2(-20, -20));

        shouldMove = false;
        enabled = false;
    }

    public void UpdateRound(RuntimeBattleCharacter turnChara)
    {
        if (turnChara == effectCaster)
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

    private void Update()
    {
        if(shouldMove)
        {
            if(Vector2.Distance(transform.position, target) < 0.1f)
            {
                movementCallback?.Invoke();
                ResetObject();
            }
            transform.position += direction * speed * Time.deltaTime;
        }
    }
}
