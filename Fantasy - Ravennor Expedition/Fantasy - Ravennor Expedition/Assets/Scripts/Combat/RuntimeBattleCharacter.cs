﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RuntimeBattleCharacter : MonoBehaviour
{
    [SerializeField]
    private PersonnageScriptables currentScriptable;

    [Header("Affichages")]
    [SerializeField]
    private SpriteRenderer characterSprite;
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private SpriteRenderer handSpriteRight, handSpriteLeft;
    [SerializeField]
    private GameObject brasParent;

    [HideInInspector]
    private int possibleAction;
    
    [HideInInspector]
    public bool CanMove => movementLeft >= 10;

    //[HideInInspector]
    public int movementLeft;

    private int currentHps;

    private int currentMaana;

    private int team;

    [HideInInspector]
    public Node currentNode { get; private set; }

    private float vulnerability, dangerosity;

    [SerializeField]
    private Image hpImage;

    [SerializeField]
    private CharacterUI uiManagement;

    private int initiative;

    private List<CharacterActionScriptable> actionsDisponibles;
    private List<int> cooldowns = new List<int>();
    private List<int> spellUtilisations = new List<int>();

    //Effets
    private List<RuntimeSpellEffect> appliedEffects = new List<RuntimeSpellEffect>();

    private List<Affliction> afflictions = new List<Affliction>();

    private List<RuntimeBattleCharacter> invocations = new List<RuntimeBattleCharacter>();

    public UnityEvent<int> damageTakenEvt = new UnityEvent<int>();
    [HideInInspector]
    public UnityEvent<int> healTakenEvt = new UnityEvent<int>(), movedEvt = new UnityEvent<int>();

    public UnityEvent failedActionEvt = new UnityEvent(), crititcalActionEvt = new UnityEvent();
    [SerializeField]
    private UnityEvent highlightEvt = new UnityEvent(), endHighlightEvt = new UnityEvent();
    [HideInInspector]
    public UnityEvent useActionEvt = new UnityEvent(), endTurnEvt = new UnityEvent(), beginTurnEvt = new UnityEvent(), deathEvt = new UnityEvent();

    private bool hasOpportunity = true;

    private void OnMouseEnter()
    {
        highlightEvt.Invoke();
    }

    private void OnMouseExit()
    {
        endHighlightEvt.Invoke();
    }

    #region Varialbes Utilities
    public PersonnageScriptables GetCharacterDatas()
    {
        return currentScriptable;
    }

    public int GetCurrentHps()
    {
        return currentHps;
    }
    public int GetCurrentMaana()
    {
        return currentMaana;
    }

    public int GetMaxHp => currentScriptable.GetMaxHps();

    public bool CanDoAction()
    {
        return (possibleAction > 0);
    }

    public void UseAction(bool isBaseAttack)
    {
        possibleAction--;
        BattleUiManager.instance.UseActionFeedback();
    }

    public void UseAllAction()
    {
        possibleAction = 0;
    }

    public void ResetOneAction()
    {
        possibleAction++;
    }

    public bool HasEnoughMaana(int maanaAmount)
    {
        return maanaAmount <= currentMaana;
    }

    public void UseSpell(CharacterActionScriptable spell)
    {
        int index = 0;
        for (index = 0; index < actionsDisponibles.Count; index++)
        {
            if (actionsDisponibles[index].IsSameSpell(spell.nom))
            {
                break;
            }
        }
        spellUtilisations[index] += 1;
    }

    public bool HasSpellUtilisationLeft(CharacterActionScriptable spell)
    {
        int index = 0;
        for (index = 0; index < actionsDisponibles.Count; index++)
        {
            if (actionsDisponibles[index].IsSameSpell(spell.nom))
            {
                if (spell.maxUtilisation > 0 && spellUtilisations[index] < spell.maxUtilisation)
                {
                    return true;
                }
                break;
            }
        }
        return false;
    }

    public bool UseMaana(int maanaSpent)
    {

        if (maanaSpent <= currentMaana)
        {
            currentMaana -= maanaSpent;

            if (BattleUiManager.instance.GetCurrentChara() == this)
            {
                BattleUiManager.instance.SetCurrentMaana(currentMaana);
            }
            return true;
        }
        return false;
    }

    public void UseMaanaWithoutrestriction(int maanaSpent)
    {
        currentMaana -= maanaSpent;

        if (currentMaana < 0)
        {
            currentMaana = 0;
        }
    }

    public List<RuntimeSpellEffect> GetAppliedEffects()
    {
        return new List<RuntimeSpellEffect>(appliedEffects);
    }
    public int GetTeam()
    {
        return team;
    }

    public int GetInitiative()
    {
        return initiative;
    }
    public List<CharacterActionScriptable> GetActions(bool isOvercharged)
    {
        if (isOvercharged)
        {
            return currentScriptable.GetOverchargedSpell();
        }
        else
        {
            return actionsDisponibles;
        }
    }

    public void AddAffliction(Affliction affToAdd)
    {
        if (affToAdd == Affliction.InstantKill)
        {
            Debug.Log("Affliction de Mort");
            TakeDamage(DamageType.Brut, currentHps);
        }
        else
        {
            afflictions.Add(affToAdd);
        }
    }

    public void RemoveAffliction(Affliction affToRemove)
    {
        afflictions.Remove(affToRemove);
    }

    public bool CheckForAffliction(Affliction toCheck)
    {
        return afflictions.Contains(toCheck);
    }

    public bool CheckToDeleteAffliction(Affliction toCheck)
    {
        int nb = 0;
        foreach(RuntimeSpellEffect eff in appliedEffects)
        {
            if (eff.effet.affliction == toCheck)
                nb++;
        }

        return nb > 1;
    }

    public int GetSpellCooldown(CharacterActionScriptable spell)
    {
        int index = 0;
        for (index = 0; index < actionsDisponibles.Count; index++)
        {
            if (actionsDisponibles[index].IsSameSpell(spell.nom))
            {
                break;
            }
        }
        return cooldowns[index];
    }

    public void SetCooldown(CharacterActionScriptable spell)
    {
        int index = 0;
        for(index = 0; index < actionsDisponibles.Count; index++)
        {
            if(actionsDisponibles[index].IsSameSpell(spell.nom))
            {
                break;
            }
        }
        cooldowns[index] = actionsDisponibles[index].GetMaxCooldown();
    }

    #endregion

    #region Set Chara
    public void UseRuntimeCharacter(PersonnageScriptables newScriptable, int newTeam, Vector2 newPosition)
    {
        name = newScriptable.nom;
        transform.position = newPosition;
        currentNode = Grid.instance.NodeFromWorldPoint(newPosition);
        UseRuntimeCharacter(newScriptable, newTeam);
    }

    public void UseRuntimeCharacter(PersonnageScriptables newScriptable, int newTeam)
    {
        team = newTeam;
        currentScriptable = Instantiate(newScriptable);

        //currentScriptable.ResetStats();

        currentHps = currentScriptable.GetMaxHps();
        currentMaana = currentScriptable.GetMaxMaana();

        movementLeft = currentScriptable.GetMovementSpeed();

        actionsDisponibles = new List<CharacterActionScriptable>(currentScriptable.knownSpells);

        for(int i = 0; i < actionsDisponibles.Count; i++)
        {
            cooldowns.Add(0);
            spellUtilisations.Add(0);
        }

        currentNode = Grid.instance.NodeFromWorldPoint(transform.position);

        initiative = currentScriptable.GetInitiativeBrut();//BattleManager.instance.NormalRoll(currentScriptable.GetInitiativeDice(), currentScriptable.GetInititativeBonus(), DiceType.D6);

        characterSprite.sprite = currentScriptable.spritePerso;
        handSpriteRight.sprite = currentScriptable.spriteBras;
        handSpriteLeft.sprite = currentScriptable.spriteBras;

        brasParent.transform.localPosition = new Vector3(brasParent.transform.localPosition.x, currentScriptable.brasPosition, 0);
    }
    #endregion

    public void NewTurn()
    {
        if (currentHps > 0)
        {
            for(int i = 0; i < cooldowns.Count; i++)
            {
                if (cooldowns[i] > 0)
                {
                    cooldowns[i]--;
                }
            }

            possibleAction = currentScriptable.GetPossibleActions();

            hasOpportunity = true;

            if (CheckForAffliction(Affliction.Immobilisation))
            {
                movementLeft = 0;
            }
            else
            {
                movementLeft = currentScriptable.GetMovementSpeed();
            }

            UpdateEffects();

            beginTurnEvt.Invoke();
        }
    }

    #region Utilities
    public float GetPercentHp()
    {
        return (float)currentHps / (float)currentScriptable.GetMaxHps();
    }

    public int GetMaxMovement()
    {
        return currentScriptable.GetMovementSpeed();
    }

    public void AddVulnerability(float toAdd)
    {
        vulnerability += toAdd;
    }

    public void AddDangerosity(float toAdd)
    {
        dangerosity += toAdd;
    }

    public float GetVulnerability()
    {
        return vulnerability;
    }

    public float GetDangerosity()
    {
        return dangerosity;
    }

    public void ResetVulnerabilityDangerosity()
    {
        vulnerability = 0;
        dangerosity = 0;
    }

    public void ModifyCurrentNode(Node newNode)
    {
        //currentNode.hasCharacterOn = false;
        currentNode = newNode;
        //currentNode.hasCharacterOn = true;
        currentNode.chara = this;
    }

    public void Die()
    {
        gameObject.SetActive(false);
    }

    public void SetHighlight(bool isHighlight)
    {
        if(isHighlight)
        {
            highlightEvt.Invoke();
        }
        else
        {
            endHighlightEvt.Invoke();
        }
    }
    #endregion

    #region Action Interraction
    public void UseActionAnim()
    {
        BattleManager.instance.UseCurrentAction();
    }

    public int TakeDamage(DamageType typeOfDamage, int damageDealt)
    {
        int damageAmount;

        int brutDamage = 0;

        int physicalDamage = damageDealt;

        if (physicalDamage > 0 && currentScriptable.GetArmor() > 0)
        {
            BattleDiary.instance.AddText("L'armure de " + name + " réduit les dégâts de " + currentScriptable.GetArmor().ToString() + ".");
            if (currentScriptable.GetArmor() <= physicalDamage)
            {
                physicalDamage -= currentScriptable.GetArmor();
            }
            else
            {
                physicalDamage = 0;
            }
        }

        damageAmount = physicalDamage + brutDamage;

        string damageText = damageAmount + " de dégâts";
        if (damageAmount > 0)
        {
            if (RavenorGameManager.instance != null && team == 0 && RavenorGameManager.instance.GetDifficulty() != 1)
            {
                damageAmount = Mathf.RoundToInt(damageAmount * RavenorGameManager.instance.GetDifficulty());
                if (damageAmount <= 0)
                {
                    damageAmount = 1;
                }
                if (RavenorGameManager.instance.GetDifficulty() < 1)
                {
                    damageText += "(réduit à " + damageAmount + ") ";
                }
                else
                {
                    damageText += "(augmenté à " + damageAmount + ") ";
                }
            }


            damageTakenEvt.Invoke(damageAmount);
            ResolveEffect(EffectTrigger.DamageTaken);

            uiManagement.ShowDirectHitResult(damageAmount, false);

            BattleDiary.instance.AddText(name + " a pris " + damageText + " points de dégâts.");
            currentHps -= damageAmount;
            hpImage.fillAmount = (float)currentHps / (float)currentScriptable.GetMaxHps();

            if (BattleUiManager.instance.GetCurrentChara() == this)
            {
                BattleUiManager.instance.SetCurrentHps(currentHps);
            }

            if (currentHps <= 0)
            {
                BattleManager.instance.KillCharacter(this);
                return damageAmount + currentHps;
            }
            return damageAmount;
        }

        return 0;
    }

    public void TakeHeal(int healAmount)
    {
        if (healAmount > 0)
        {
            if (currentHps + healAmount > currentScriptable.GetMaxHps())
            {
                healAmount = currentScriptable.GetMaxHps() - currentHps;
            }

            uiManagement.ShowDirectHitResult(healAmount, true);

            healTakenEvt.Invoke(healAmount);
            ResolveEffect(EffectTrigger.Heal);

            Debug.Log(this + " healed of " + healAmount);
            BattleDiary.instance.AddText(name + " est soigné de " + healAmount + ".");

            currentHps += healAmount;

            hpImage.fillAmount = (float)currentHps / (float)currentScriptable.GetMaxHps();

            if (BattleUiManager.instance.GetCurrentChara() == this)
            {
                BattleUiManager.instance.SetCurrentHps(currentHps);
            }
        }
    }

    public void AddEffect(RuntimeSpellEffect runEffect)
    {
        if (ContainsEffect(runEffect.effet))
        {
            RemoveEffect(runEffect.effet);
        }

        appliedEffects.Add(runEffect);
        runEffect.currentStack++;

        if (!CheckForAffliction(runEffect.effet.affliction) && runEffect.effet.affliction != Affliction.None)
        {
            AddAffliction(runEffect.effet.affliction);
        }
    }

    public void ApplyEffect(SpellEffect wantedEffect)
    {
        currentScriptable.StatBonus(wantedEffect.RealValue(), wantedEffect.type, wantedEffect.dicesBonus, true);
    }

    public void UpdateEffects()
    {
        for(int i = 0; i < appliedEffects.Count; i++)
        {
            ResolveEffect(EffectTrigger.BeginTurn);

            if (appliedEffects[i].currentCooldown >= 0)
            {

                appliedEffects[i].currentCooldown--;
                if (appliedEffects[i].currentCooldown <= 0)
                {
                    ResolveEffect(EffectTrigger.End);
                    RemoveEffect(i);
                    i--;
                }
            }
        }
    }

    public void RemoveEffect(SpellEffectCommon effectToRemove)
    {
        int index = 0;
        foreach(RuntimeSpellEffect eff in appliedEffects)
        {
            if (eff.effet.nom == effectToRemove.nom)
            {
                break;
            }
            index++;
        }

        if (index < appliedEffects.Count)
        {
            RemoveEffect(index);
        }
    }

    public void RemoveEffect(int index)
    {
        foreach (SpellEffect eff in appliedEffects[index].effet.effects)
        {
            currentScriptable.StatBonus(-eff.RealValue(), eff.type, eff.dicesBonus, false);
        }

        if(appliedEffects[index].effet.affliction != Affliction.None && CheckToDeleteAffliction(appliedEffects[index].effet.affliction))
        {
            RemoveAffliction(appliedEffects[index].effet.affliction);
        }

        appliedEffects.RemoveAt(index);
    }

    public void ResolveEffect(EffectTrigger triggerWanted)
    {
        for (int i = 0; i < appliedEffects.Count; i++)
        {
            BattleManager.instance.ResolveEffect(appliedEffects[i].effet, transform.position, transform.position, triggerWanted, appliedEffects[i].currentStack);
        }
    }

    public void ResolveEffect(EffectTrigger triggerWanted, Vector2 targetPosition)
    {
        for (int i = 0; i < appliedEffects.Count; i++)
        {
            BattleManager.instance.ResolveEffect(appliedEffects[i].effet, transform.position, targetPosition, triggerWanted, appliedEffects[i].currentStack);
        }
    }

    public bool ContainsEffect(SpellEffectCommon effectToCheck)
    {
        foreach(RuntimeSpellEffect eff in appliedEffects)
        {
            if(eff.effet.nom == effectToCheck.nom)
            {
                return true;
            }
        }
        return false;
    }

    public void AddInvocation(RuntimeBattleCharacter toAdd)
    {
        invocations.Add(toAdd);
    }

    public List<RuntimeBattleCharacter> GetInvocations()
    {
        return invocations;
    }

    public bool CheckForInvocations(PersonnageScriptables newInvoc)
    {
        foreach (RuntimeBattleCharacter chara in invocations)
        {
            if (chara.GetCharacterDatas().name == newInvoc.name)
            {
                return true;
            }
        }
        return false;
    }
    
    public void AttackOfOpportunity(RuntimeBattleCharacter opportunityTarget)
    {
        bool canUseSpell = true;
        if ((actionsDisponibles[0].attackType != AttackType.Magical && CheckForAffliction(Affliction.Atrophie)) || (actionsDisponibles[0].attackType == AttackType.Magical && CheckForAffliction(Affliction.Silence)))
        {
            canUseSpell = false;
        }

        if (hasOpportunity && canUseSpell)
        {
            BattleManager.instance.LaunchAction(actionsDisponibles[0], 0, this, opportunityTarget.currentNode.worldPosition, true);
            hasOpportunity = false;
        }
    }

    public void Teleport(Vector2 newPosition)
    {
        transform.position = newPosition;
        currentNode = Grid.instance.NodeFromWorldPoint(newPosition);
        Grid.instance.CreateGrid();
    }
    #endregion

    #region UI
    public void DisplayDice(List<int> values, List<BattleDiceResult> results, int total)
    {
        int usedArmor = 0;
        for(int i = 0; i < results.Count; i++)
        {
            if(results[i] == BattleDiceResult.Hit && usedArmor < currentScriptable.GetArmor())
            {
                usedArmor++;
                results[i] = BattleDiceResult.Reduce;
            }
        }

        uiManagement.ShowDiceResults(values, results, total);
    }

    #endregion

    #region Animations
    public void SetSpriteDirection(bool direction)
    {
        characterSprite.flipX = direction;
    }

    public void SetAnimation(string animName)
    {
        anim.Play(animName);
    }

    public AnimatorClipInfo GetCurrentAnimation()
    {
        AnimatorClipInfo[] infos = anim.GetCurrentAnimatorClipInfo(0);
        if (infos.Length > 0)
        {
            return infos[0];
        }
        return new AnimatorClipInfo();
    }
    #endregion
}
