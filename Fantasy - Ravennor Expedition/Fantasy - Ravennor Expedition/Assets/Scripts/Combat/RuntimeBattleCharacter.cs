using System.Collections;
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
    private int possibleAction, possibleBaseAttack;
    
    [HideInInspector]
    public bool hasMoved;

    //[HideInInspector]
    public int movementLeft;

    private int currentHps;

    private int currentMaana;

    private int team;

    [HideInInspector]
    public Node currentNode;

    private float vulnerability, dangerosity;

    [SerializeField]
    private Image hpImage;

    private int initiative;

    private List<CharacterActionScriptable> actionsDisponibles;
    private List<int> cooldowns = new List<int>();
    private List<int> spellUtilisations = new List<int>();

    //Effets
    private List<RuntimeSpellEffect> appliedEffects = new List<RuntimeSpellEffect>();

    private List<Affliction> afflictions = new List<Affliction>();

    private List<RuntimeBattleCharacter> invocations = new List<RuntimeBattleCharacter>();

    [HideInInspector]
    public UnityEvent<int> damageTakenEvt = new UnityEvent<int>(), healTakenEvt = new UnityEvent<int>(), movedEvt = new UnityEvent<int>();
    [HideInInspector]
    public UnityEvent useActionEvt = new UnityEvent(), endTurnEvt = new UnityEvent(), beginTurnEvt = new UnityEvent(), deathEvt = new UnityEvent();

    private bool hasOpportunity = true;

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

    public bool CanDoAction(bool isBaseAttack)
    {
        if(isBaseAttack)
        {
            return (possibleBaseAttack > 0 || possibleAction > 0);
        }
        return (possibleAction > 0);
    }

    public void UseAction(bool isBaseAttack)
    {
        if (isBaseAttack)
        {
            if (possibleBaseAttack > 0)
            {
                possibleBaseAttack--;
            }
            else
            {
                possibleAction--;
            }
        }
        else
        {
            possibleAction--;
        }
    }

    public void UseAllAction()
    {
        possibleBaseAttack = 0;
        possibleAction = 0;
    }

    public bool HasEnoughMaana(int maanaAmount)
    {
        return maanaAmount <= currentMaana;
    }

    public void UseSpell(CharacterActionScriptable spell)
    {
        int index = 0;
        foreach (CharacterActionScriptable s in actionsDisponibles)
        {
            if (s.nom == spell.nom)
            {
                break;
            }
            index++;
        }
        Debug.Log(index);
        Debug.Log(spellUtilisations.Count);
        spellUtilisations[index] += 1;
    }

    public bool HasSpellUtilisationLeft(CharacterActionScriptable spell)
    {
        int index = 0;
        foreach (CharacterActionScriptable s in actionsDisponibles)
        {
            if (s.nom == spell.nom)
            {
                if(spell.maxUtilisation > 0 && spellUtilisations[index] < spell.maxUtilisation)
                {
                    return true;
                }
                break;
            }
            index++;
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
    public List<CharacterActionScriptable> GetActions()
    {
        return actionsDisponibles;
    }

    public void AddAffliction(Affliction affToAdd)
    {
        if (affToAdd == Affliction.InstantKill)
        {
            Debug.Log("Affliction de Mort");
            TakeDamage(currentHps,DamageType.Brut);
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
        foreach (CharacterActionScriptable s in actionsDisponibles)
        {
            if (s.nom == spell.nom)
            {
                break;
            }
            index++;
        }
        return cooldowns[index];
    }

    public void SetCooldown(CharacterActionScriptable spell)
    {
        int index = 0;
        foreach(CharacterActionScriptable s in actionsDisponibles)
        {
            if(s.nom == spell.nom)
            {
                break;
            }
            index++;
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

        actionsDisponibles= new List<CharacterActionScriptable>(currentScriptable.sortsDisponibles);

        for(int i = 0; i < actionsDisponibles.Count; i++)
        {
            cooldowns.Add(0);
            spellUtilisations.Add(0);
        }

        currentNode = Grid.instance.NodeFromWorldPoint(transform.position);

        initiative = BattleManager.instance.NormalRoll(currentScriptable.GetInitiative(), 0, DiceType.D6);

        characterSprite.sprite = currentScriptable.spritePerso;
        handSpriteRight.sprite = currentScriptable.spriteDeMains;
        handSpriteLeft.sprite = currentScriptable.spriteDeMains;

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

            hasMoved = false;

            currentScriptable.GetPossibleActions(out possibleAction, out possibleBaseAttack);

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

    public float GetMaxMovement()
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
    #endregion

    #region Action Interraction
    public int TakeDamage(int damageAmount, DamageType typeOfDamage)
    {
        int damageResistance = 0;
        switch(typeOfDamage)
        {
            case DamageType.Physical:
                damageResistance = currentScriptable.GetPhysicalArmor();
                break;
            case DamageType.Magical:
                damageResistance = currentScriptable.GetMagicalArmor();
                break;
        }

        damageAmount -= damageResistance;

        if (damageAmount > 0)
        {
            damageTakenEvt.Invoke(damageAmount);
            ResolveEffect(EffectTrigger.DamageTaken);

            Debug.Log(this + " took " + damageAmount);
            BattleDiary.instance.AddText(name + " a pris " + damageAmount.ToString() + " de dégâts");
            currentHps -= damageAmount;
            hpImage.fillAmount = (float)currentHps / (float)currentScriptable.GetMaxHps();

            SetAnimation("DamageTaken");

            if (BattleUiManager.instance.GetCurrentChara() == this)
            {
                BattleUiManager.instance.SetCurrentHps(currentHps);
            }

            if (currentHps <= 0)
            {
                BattleManager.instance.KillCharacter(this);
                return 0;
            }
            return damageAmount;
        }

        return 0;
    }

    public int TakeDamage(DamageType typeOfDamage, List<Dice> damageDices, int bonusAmount)
    {
        int damageAmount;

        int magicalDamage = 0;
        int physicalDamage = 0;
        int brutDamage = 0;

        string damageFeedback = "";
        int dmg = 0;

        foreach(Dice d in damageDices)
        {
            if (d.numberOfDice > 0)
            {
                if(dmg != 0)
                {
                    damageFeedback += "+ ";
                }
                dmg = GameDices.RollDice(d.numberOfDice, d.wantedDice);
                damageFeedback += dmg.ToString() + "(" + d.numberOfDice + d.wantedDice + ") ";
                switch (d.wantedDamage)
                {
                    case DamageType.Physical:
                        physicalDamage += dmg;
                        break;
                    case DamageType.Magical:
                        magicalDamage += dmg;
                        break;
                    case DamageType.Brut:
                        brutDamage += dmg;
                        break;
                }
            }
        }

        if (bonusAmount != 0)
        {
            if(damageFeedback != "")
            {
                if (bonusAmount > 0)
                {
                    damageFeedback += "+ ";
                }
                else
                {
                    damageFeedback += "- ";
                }
            }
            damageFeedback += Mathf.Abs(bonusAmount).ToString() + " ";

            switch (typeOfDamage)
            {
                case DamageType.Physical:
                    physicalDamage += bonusAmount;
                    break;
                case DamageType.Magical:
                    magicalDamage += bonusAmount;
                    break;
                case DamageType.Brut:
                    brutDamage += bonusAmount;
                    break;
            }
        }

        if(currentScriptable.GetMagicalArmor()<=magicalDamage)
        {
            magicalDamage -= currentScriptable.GetMagicalArmor();
        }
        else
        {
            magicalDamage = 0;
        }

        if(currentScriptable.GetPhysicalArmor()<=physicalDamage)
        {
            physicalDamage -= currentScriptable.GetPhysicalArmor();
        }
        else
        {
            physicalDamage = 0;
        }

        damageAmount = physicalDamage + magicalDamage + brutDamage;

        string damageText = damageAmount + " de dégâts";
        if ((damageAmount - bonusAmount) > 0 && damageAmount > 0)
        {
            if (RavenorGameManager.instance != null && team == 1 && RavenorGameManager.instance.GetDifficulty() != 1)
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

            Debug.Log(name + " a pris " + damageText + " (" + damageFeedback + ")");
            BattleDiary.instance.AddText(name + " a pris " + damageText + " (" + damageFeedback + ")");
            currentHps -= damageAmount;
            hpImage.fillAmount = (float)currentHps / (float)currentScriptable.GetMaxHps();

            SetAnimation("DamageTaken");

            if (BattleUiManager.instance.GetCurrentChara() == this)
            {
                BattleUiManager.instance.SetCurrentHps(currentHps);
            }

            if (currentHps <= 0)
            {
                BattleManager.instance.KillCharacter(this);
                return 0;
            }
            return damageAmount;
        }

        //Debug.Log(this + " took " + damageFeedback);

        return 0;
    }

    public void TakeHeal(int healAmount)
    {
        if (healAmount > 0)
        {
            healTakenEvt.Invoke(healAmount);
            ResolveEffect(EffectTrigger.Heal);

            Debug.Log(this + " healed of " + healAmount);
            BattleDiary.instance.AddText(name + " est soigné de " + healAmount);

            currentHps += healAmount;

            currentHps = Mathf.Clamp(currentHps, 0, currentScriptable.GetMaxHps());

            hpImage.fillAmount = (float)currentHps / (float)currentScriptable.GetMaxHps();

            if (BattleUiManager.instance.GetCurrentChara() == this)
            {
                BattleUiManager.instance.SetCurrentHps(currentHps);
            }
        }
    }

    public void TakeHeal(List<Dice> healedDice, int bonusAmount)
    {
        if (healedDice.Count > 0)
        {
            int healAmount = 0, newAmount = 0;
            string toPrint = name + " est soigné de ";
            foreach (Dice d in healedDice)
            {
                if (d.numberOfDice > 0)
                {
                    if (newAmount != 0)
                    {
                        toPrint += "+ ";
                    }
                    newAmount = GameDices.RollDice(d.numberOfDice, d.wantedDice);
                    toPrint += newAmount.ToString() + "(" + d.numberOfDice + d.wantedDice + ") ";

                    healAmount += newAmount;
                }
            }

            healTakenEvt.Invoke(healAmount+bonusAmount);
            ResolveEffect(EffectTrigger.Heal);

            Debug.Log(this + " healed of " + (healAmount + bonusAmount));
            BattleDiary.instance.AddText(toPrint);

            currentHps += healAmount + bonusAmount;

            Mathf.Clamp(currentHps, 0, currentScriptable.GetMaxHps());

            hpImage.fillAmount = (float)currentHps / (float)currentScriptable.GetMaxHps();

            if (BattleUiManager.instance.GetCurrentChara() == this)
            {
                BattleUiManager.instance.SetCurrentHps(currentHps);
            }
        }
    }

    public void AddEffect(RuntimeSpellEffect runEffect)
    {
        if (appliedEffects.Contains(runEffect))
        {
            foreach (RuntimeSpellEffect eff in appliedEffects)
            {
                if (eff.effet.nom == runEffect.effet.nom && eff.currentCooldown < runEffect.currentCooldown)
                {
                    eff.currentCooldown = runEffect.currentCooldown;
                    if(eff.currentStack < eff.effet.maxStack)
                    {
                        eff.currentStack++;
                    }
                }
            }
        }
        else
        {
            appliedEffects.Add(runEffect);
            runEffect.currentStack++;

            /*foreach (SpellEffect eff in runEffect.effet.effects)
            {
                currentScriptable.StatBonus(eff.value, eff.type, eff.dicesBonus, true);
            }*/

            if(!CheckForAffliction(runEffect.effet.affliction) && runEffect.effet.affliction != Affliction.None)
            {
                AddAffliction(runEffect.effet.affliction);
            }
        }
    }

    public void ApplyEffect(SpellEffect wantedEffect)
    {
        currentScriptable.StatBonus(wantedEffect.value, wantedEffect.type, wantedEffect.dicesBonus, true);
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

    public void RemoveEffect(SpellEffectScriptables effectToRemove)
    {
        int index = 0;
        foreach(RuntimeSpellEffect eff in appliedEffects)
        {
            if (eff.effet.nom == effectToRemove.effet.nom)
            {
                break;
            }
            index++;
        }

        if (index < appliedEffects.Count)
        {
            Debug.Log("Effect remove");
            RemoveEffect(index);
        }
    }

    public void RemoveEffect(int index)
    {
        foreach (SpellEffect eff in appliedEffects[index].effet.effects)
        {
            currentScriptable.StatBonus(-eff.value, eff.type, eff.dicesBonus, false);
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
            BattleManager.instance.ResolveEffect(appliedEffects[i].effet, transform.position, triggerWanted, appliedEffects[i].currentStack);
        }
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
    
    public void AttackOfOpportunity(Vector2 position)
    {
        bool canUseSpell = true;
        if ((actionsDisponibles[0].attackType != AttackType.PuissMagique && CheckForAffliction(Affliction.Atrophie)) || (actionsDisponibles[0].attackType == AttackType.PuissMagique && CheckForAffliction(Affliction.Silence)))
        {
            canUseSpell = false;
        }

        if (hasOpportunity && canUseSpell)
        {
            BattleManager.instance.LaunchAction(actionsDisponibles[0], this, position, true);
            hasOpportunity = false;
        }
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
    #endregion
}
