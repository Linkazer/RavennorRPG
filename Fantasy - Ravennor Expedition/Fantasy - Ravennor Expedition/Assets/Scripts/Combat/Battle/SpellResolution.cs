using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;

public class SpellResolution : MonoBehaviour
{
    private static SpellResolution instance;

    [Header("Sons")]
    [SerializeField] private AudioSource attackAudioSource;
    [SerializeField] private List<RVN_AudioSound> diceClip;

    private void Awake()
    {
        instance = this;
    }

    public static void ResolveSpell(CharacterActionDirect wantedAction, RuntimeBattleCharacter caster, RuntimeBattleCharacter target, bool isEffectSpell)
    {
        int applyEffect = 0;
        if (wantedAction.hasPowerEffect)
        {
            if (wantedAction.damageType == DamageType.Heal)
            {
                instance.DoHeal(wantedAction, caster, target);
            }
            else
            {
                applyEffect = instance.DoDamage(wantedAction, caster, target);
                caster.TakeHeal(Mathf.CeilToInt(applyEffect * wantedAction.lifeStealPercent));
            }
        }
        else
        {
            applyEffect = 1;
        }

        if (applyEffect > 0)
        {
            if (!isEffectSpell)
            {
                caster.ResolveEffect(EffectTrigger.DamageDealSelf);
                caster.ResolveEffect(EffectTrigger.DamageDealTarget, target.currentNode.worldPosition);
            }
            if (wantedAction.wantedEffectOnTarget.Count > 0)
            {
                foreach (SpellEffectScriptables eff in wantedAction.wantedEffectOnTarget)
                {
                    ApplyEffects(eff, caster, target);
                }
            }
        }
    }

    public void DoHeal(CharacterActionDirect wantedAction, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        int baseHeal = wantedAction.GetBaseDamage() + caster.GetCharacterDatas().GetSoinApplique() + caster.GetCharacterDatas().GetPower();

        target.TakeHeal(baseHeal);
    }

    public int DoDamage(CharacterActionDirect wantedAction, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        if (!attackAudioSource.isPlaying)
        {
            SoundSyst.PlaySound(diceClip[UnityEngine.Random.Range(0, diceClip.Count)], attackAudioSource);
        }

        return target.TakeDamage(wantedAction.damageType, DoesHit(wantedAction, caster, target));
    }

    public int DoesHit(CharacterActionDirect wantedAction, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        int targetDefenseScore = 0;
        int dealtDamage = 0;

        List<int> diceValues = new List<int>();
        List<BattleDiceResult> diceResult = new List<BattleDiceResult>();

        int neededDices = wantedAction.GetDices();

        targetDefenseScore = target.GetCharacterDatas().GetDefense();

        int resultAtt = 0;
        for (int i = 0; i < neededDices; i++)
        {
            resultAtt = GameDices.RollD6() + caster.GetCharacterDatas().GetAccuracy();

            if (resultAtt <= targetDefenseScore)
            {
                diceResult.Add(BattleDiceResult.Block);
            }
            else
            {
                diceResult.Add(BattleDiceResult.Hit);
            }

            diceValues.Add(resultAtt);
            if (resultAtt > targetDefenseScore)
            {
                dealtDamage++;
            }
        }


        if ((neededDices <= 0 && wantedAction.GetBaseDamage() > 0) || dealtDamage > 0)
        {
            dealtDamage += caster.GetCharacterDatas().GetPower() + wantedAction.GetBaseDamage();
        }

        if (dealtDamage < 0)
        {
            dealtDamage = 0;
        }

        target.DisplayDice(diceValues, diceResult, dealtDamage);

        return dealtDamage;
    }

    public static void ApplyEffects(SpellEffectScriptables wantedEffect, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        if (wantedEffect.effet.wantedEffectToTrigger == null || target.ContainsEffect(wantedEffect.effet.wantedEffectToTrigger.effet))
        {
            BattleDiary.instance.AddText(target.name + " est affecté par " + wantedEffect.effet.nom + ".");

            RuntimeSpellEffect runEffet = new RuntimeSpellEffect(
            wantedEffect.effet,
            wantedEffect.duree,
            caster
            );

            runEffet.ApplyEffect(caster, target);

            foreach (SpellEffectScriptables eff in wantedEffect.bonusToCancel)
            {
                target.RemoveEffect(eff.effet);
            }

            instance.ResolveEffect(runEffet.effet, target, EffectTrigger.Apply);
        }
    }

    public static void AskResolveEffect(SpellEffectCommon effect, Vector2 casterPosition, Vector2 targetPosition, EffectTrigger triggerWanted, int stack)
    {
        instance.ResolveEffect(effect, casterPosition, targetPosition, triggerWanted, stack);
    }

    public static void AskResolveEffect(SpellEffectCommon effect, RuntimeBattleCharacter target, EffectTrigger triggerWanted)
    {
        instance.ResolveEffect(effect, target, triggerWanted);
    }

    public void ResolveEffect(SpellEffectCommon effect, Vector2 casterPosition, Vector2 targetPosition, EffectTrigger triggerWanted, int stack)
    {
        for (int i = 0; i < stack; i++)
        {
            if (Grid.instance.NodeFromWorldPoint(casterPosition).HasCharacterOn)
            {
                RuntimeBattleCharacter target = Grid.instance.NodeFromWorldPoint(casterPosition).chara;

                foreach (SpellEffect eff in effect.effects)
                {
                    if (eff.trigger == triggerWanted) //Rajouter la prise en compte des Targets possibles
                    {
                        target.ApplyEffect(eff);
                    }
                }
            }

            foreach (SpellEffectAction effAct in effect.actionEffect)
            {
                if (effAct.trigger == triggerWanted)
                {
                    if (triggerWanted == EffectTrigger.DamageDealTarget)
                    {
                        BattleActionsManager.LaunchAction(effAct.spellToUse, effAct.caster, targetPosition, true);
                    }
                    else
                    {
                        BattleActionsManager.LaunchAction(effAct.spellToUse, effAct.caster, casterPosition, true);
                    }
                }
            }
        }
    }

    public void ResolveEffect(SpellEffectCommon effect, RuntimeBattleCharacter target, EffectTrigger triggerWanted)
    {
        foreach (SpellEffect eff in effect.effects)
        {
            if (eff.trigger == triggerWanted) //Rajouter la prise en compte des Targets possibles
            {
                target.ApplyEffect(eff);
            }
        }

        foreach (SpellEffectAction effAct in effect.actionEffect)
        {
            if (effAct.trigger == triggerWanted)
            {
                Debug.Log("Launch Action : " + effAct.caster);
                BattleActionsManager.LaunchAction(effAct.spellToUse, effAct.caster, target.transform.position, true);
            }
        }
    }

    private void InvokeAlly(RuntimeBattleCharacter caster, CharacterActionInvocation spell, Vector2 wantedPosition)
    {
        /*PersonnageScriptables toInvoke = CharacterToInvoke(spell.invocations);

        if (!caster.CheckForInvocations(toInvoke) && toInvoke != null)
        {
            Node nodeWanted = Grid.instance.NodeFromWorldPoint(wantedPosition);
            if (nodeWanted.usableNode && !nodeWanted.HasCharacterOn)
            {
                BattleManager.SetNewCharacter(toInvoke, wantedPosition, 0);

                caster.AddInvocation(roundList[roundList.Count - 1]);

                if (roundList[roundList.Count - 1].GetInitiative() > currentCharacterTurn.GetInitiative())
                {
                    currentIndexTurn++;
                }

                SortInitiativeList(initiatives, roundList, 0, initiatives.Count - 1);

                Grid.instance.CreateGrid();
            }
        }
        EndCurrentAction();*/
    }

    private PersonnageScriptables CharacterToInvoke(List<PersonnageScriptables> possibleInvoc)
    {
        for (int i = possibleInvoc.Count - 1; i >= 0; i--)
        {
            if (possibleInvoc[i] != null && i < 1)
            {
                return possibleInvoc[i];
            }
        }

        return null;
    }

    private void TeleportationSpell(RuntimeBattleCharacter caster, CharacterActionTeleportation spell, Vector2 wantedPosition)
    {
        Vector2 targetPosition = wantedPosition;
        for (int i = 0; i < spell.positionsToTeleport.Count; i++)
        {
            Vector2 possiblePosition = Grid.GetTargetPosWithFacingPosition(caster.currentNode.worldPosition, wantedPosition, spell.positionsToTeleport[i]);
            if (Grid.instance.NodeFromWorldPoint(possiblePosition).walkable)
            {
                wantedPosition = possiblePosition;
                break;
            }
        }

        Node nodeWanted = Grid.instance.NodeFromWorldPoint(wantedPosition);
        if (!nodeWanted.HasCharacterOn && nodeWanted.walkable)
        {
            foreach (SpellEffectScriptables eff in spell.wantedEffectOnCaster)
            {
                ApplyEffects(eff, caster, caster);
            }
            StartCoroutine(TeleportationSpellWaiter(spell, caster, wantedPosition, targetPosition));
        }
        else
        {
            BattleUiManager.instance.DisplayErrorMessage("Aucun espace disponible pour attérir/se téléporter");
            //CancelCurrentAction();
            // VOIR POUR FAIRE AUTREMENT CODE REVIEW
        }
    }

    private IEnumerator TeleportationSpellWaiter(CharacterActionTeleportation spell, RuntimeBattleCharacter characterToTeleport, Vector2 teleportPosition, Vector2 spellTargetPosition)
    {
        if (spell.isJump)
        {
            characterToTeleport.SetAnimation("JumpBegin");
        }
        else
        {
            characterToTeleport.SetAnimation("TeleportBegin");
        }

        if (spell.jumpEffect != null)
        {
            BattleActionsManager.AskUseAction(spell.jumpEffect, characterToTeleport, characterToTeleport.currentNode.worldPosition, true);
        }

        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(characterToTeleport.GetCurrentAnimation().clip.length);

        //Application des effets

        characterToTeleport.Teleport(teleportPosition);

        if (spell.isJump)
        {
            characterToTeleport.SetAnimation("JumpEnd");
        }
        else
        {
            characterToTeleport.SetAnimation("TeleportEnd");
        }
        if (spell.landEffect != null)
        {
            BattleActionsManager.AskUseAction(spell.landEffect, characterToTeleport, spellTargetPosition, true);
        }

        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(characterToTeleport.GetCurrentAnimation().clip.length);
        BattleActionsManager.EndCurrentAction(0f);
    }
}
