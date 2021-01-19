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

    [HideInInspector]
    public bool actionAvailable;
    
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

    //Effets
    private List<RuntimeSpellEffect> appliedEffects = new List<RuntimeSpellEffect>();

    private List<Affliction> afflictions = new List<Affliction>();

    private List<RuntimeBattleCharacter> invocations = new List<RuntimeBattleCharacter>();

    [HideInInspector]
    public UnityEvent<int> damageTakenEvt = new UnityEvent<int>(), healTakenEvt = new UnityEvent<int>(), movedEvt = new UnityEvent<int>();
    [HideInInspector]
    public UnityEvent useActionEvt = new UnityEvent(), endTurnEvt = new UnityEvent(), beginTurnEvt = new UnityEvent(), deathEvt = new UnityEvent();

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

    public bool HasEnoughMaana(int maanaAmount)
    {
        return maanaAmount <= currentMaana;
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

        currentNode = Grid.instance.NodeFromWorldPoint(transform.position);

        initiative = BattleManager.instance.NormalRoll(currentScriptable.GetInitiative(), 0, DiceType.D6);

        characterSprite.sprite = currentScriptable.spritePerso;
    }
    #endregion

    public void NewTurn()
    {
        if (currentHps > 0)
        {
            hasMoved = false;

            actionAvailable = true; //A modifier plus tard

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

            Debug.Log(this + " took " + damageAmount);
            BattleDiary.instance.AddText(name + " a pris " + damageAmount.ToString() + " de dégâts");
            currentHps -= damageAmount;
            hpImage.fillAmount = (float)currentHps / (float)currentScriptable.GetMaxHps();

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

        magicalDamage -= currentScriptable.GetMagicalArmor();
        if(magicalDamage < 0)
        {
            magicalDamage = 0;
        }

        physicalDamage -= currentScriptable.GetPhysicalArmor();
        if(physicalDamage < 0)
        {
            physicalDamage = 0;
        }

        damageAmount = physicalDamage + magicalDamage + brutDamage;

        if (damageAmount > 0)
        {
            damageTakenEvt.Invoke(damageAmount);

            Debug.Log(name + " a pris " + damageFeedback + " de dégâts");
            BattleDiary.instance.AddText(name + " a pris " + damageFeedback + " de dégâts");
            currentHps -= damageAmount;
            hpImage.fillAmount = (float)currentHps / (float)currentScriptable.GetMaxHps();

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
                }
            }
        }
        else
        {
            appliedEffects.Add(runEffect);

            foreach (SpellEffect eff in runEffect.effet.effects)
            {
                currentScriptable.StatBonus(eff.value, eff.type, eff.dicesBonus, true);
            }

            if(!CheckForAffliction(runEffect.effet.affliction) && runEffect.effet.affliction != Affliction.None)
            {
                AddAffliction(runEffect.effet.affliction);
            }
        }
    }

    public void UpdateEffects()
    {
        for(int i = 0; i < appliedEffects.Count; i++)
        {
            BattleManager.instance.ApplyTimeEffect(appliedEffects[i].effet, this);

            appliedEffects[i].currentCooldown--;
            if(appliedEffects[i].currentCooldown <= 0)
            {
                RemoveEffect(i);
                i--;
            }
            //Appel des effets OnTime
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
