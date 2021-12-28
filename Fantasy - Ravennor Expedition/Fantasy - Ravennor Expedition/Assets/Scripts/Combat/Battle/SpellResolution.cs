using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellResolution : MonoBehaviour
{
    public void ResolveSpell(CharacterActionDirect wantedAction, int maanaSpent, RuntimeBattleCharacter caster, RuntimeBattleCharacter target, bool isEffectSpell)
    {
        int applyEffect = 0;
        if (wantedAction.hasPowerEffect)
        {
            if (wantedAction.damageType == DamageType.Heal)
            {
                DoHeal(wantedAction, maanaSpent, caster, target);
            }
            else
            {
                applyEffect = DoDamage(wantedAction, maanaSpent, caster, target);
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
                    ApplyEffects(eff, maanaSpent, caster, target);
                }
            }
        }
    }

    public void DoHeal(CharacterActionDirect wantedAction, int maanaSpent, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        int baseHeal = wantedAction.GetBaseDamage(maanaSpent) + caster.GetCharacterDatas().GetSoinApplique() + caster.GetCharacterDatas().GetPower();

        target.TakeHeal(baseHeal);
    }

    public int DoDamage(CharacterActionDirect wantedAction, int maanaSpent, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        if (!audioSource.isPlaying)
        {
            SoundSyst.PlaySound(diceClip[UnityEngine.Random.Range(0, diceClip.Count)], audioSource);
        }

        return target.TakeDamage(wantedAction.damageType, DoesHit(wantedAction, maanaSpent, caster, target));
    }

    public int DoesHit(CharacterActionDirect wantedAction, int maanaSpent, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        int targetDefenseScore = 0;
        int dealtDamage = 0;

        List<int> diceValues = new List<int>();
        List<BattleDiceResult> diceResult = new List<BattleDiceResult>();

        int neededDices = wantedAction.GetDices(maanaSpent);

        #region Scaling
        /*switch (wantedAction.scaleOrigin)
        {
            case ScalePossibility.EffectStack:
                foreach (RuntimeSpellEffect eff in target.GetAppliedEffects())
                {
                    if (eff.effet.nom == wantedAction.wantedScaleEffect.effet.nom)
                    {
                        neededDices.Add(wantedAction.scalingDices);
                        neededDices[neededDices.Count - 1].numberOfDice = Mathf.RoundToInt(eff.currentStack * wantedAction.diceByScale);
                    }
                }
                break;
            case ScalePossibility.HpLostPercent:
                neededDices.Add(wantedAction.scalingDices);
                neededDices[neededDices.Count - 1].numberOfDice = Mathf.RoundToInt((1 / target.GetPercentHp()) * 100 * wantedAction.diceByScale);
                break;
            case ScalePossibility.Distance:
                neededDices.Add(wantedAction.scalingDices);
                neededDices[neededDices.Count - 1].numberOfDice = Mathf.RoundToInt(Pathfinding.instance.GetDistance(caster.currentNode, target.currentNode) / 10 * wantedAction.diceByScale);
                break;
        }*/
        #endregion


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


        if ((neededDices <= 0 && wantedAction.GetBaseDamage(maanaSpent) > 0) || dealtDamage > 0)
        {
            dealtDamage += caster.GetCharacterDatas().GetPower() + wantedAction.GetBaseDamage(maanaSpent);
        }

        if (dealtDamage < 0)
        {
            dealtDamage = 0;
        }

        target.DisplayDice(diceValues, diceResult, dealtDamage);

        return dealtDamage;
    }

    public void ApplyEffects(SpellEffectScriptables wantedEffect, int maanaSpent, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        if (wantedEffect.effet.wantedEffectToTrigger == null || target.ContainsEffect(wantedEffect.effet.wantedEffectToTrigger.effet))
        {
            BattleDiary.instance.AddText(target.name + " est affecté par " + wantedEffect.effet.nom + ".");

            RuntimeSpellEffect runEffet = new RuntimeSpellEffect(
            wantedEffect.effet,
            maanaSpent,
            wantedEffect.duree,
            caster
            );

            runEffet.ApplyEffect(caster, target);

            foreach (SpellEffectScriptables eff in wantedEffect.bonusToCancel)
            {
                target.RemoveEffect(eff.effet);
            }

            ResolveEffect(runEffet.effet, target, EffectTrigger.Apply);
        }
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
                        LaunchAction(effAct.spellToUse, effAct.maanaSpent, effAct.caster, targetPosition, true);
                    }
                    else
                    {
                        LaunchAction(effAct.spellToUse, effAct.maanaSpent, effAct.caster, casterPosition, true);
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
                LaunchAction(effAct.spellToUse, effAct.maanaSpent, effAct.caster, target.transform.position, true);
            }
        }
    }

    private void InvokeAlly(RuntimeBattleCharacter caster, CharacterActionInvocation spell, Vector2 wantedPosition)
    {
        PersonnageScriptables toInvoke = CharacterToInvoke(spell.invocations);

        if (!caster.CheckForInvocations(toInvoke) && toInvoke != null)
        {
            Node nodeWanted = Grid.instance.NodeFromWorldPoint(wantedPosition);
            if (nodeWanted.usableNode && !nodeWanted.HasCharacterOn)
            {
                playerTeam.Add(toInvoke);
                SetCharacter(toInvoke, wantedPosition);

                caster.AddInvocation(roundList[roundList.Count - 1]);

                if (roundList[roundList.Count - 1].GetInitiative() > currentCharacterTurn.GetInitiative())
                {
                    currentIndexTurn++;
                }

                SortInitiativeList(initiatives, roundList, 0, initiatives.Count - 1);

                Grid.instance.CreateGrid();
            }
        }
        EndCurrentAction();
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

    private void TeleportationSpell(RuntimeBattleCharacter caster, CharacterActionTeleportation spell, int maanaSpent, Vector2 wantedPosition)
    {
        Vector2 targetPosition = wantedPosition;
        for (int i = 0; i < spell.positionsToTeleport.Count; i++)
        {
            Vector2 possiblePosition = GetTargetPosWithFacingPosition(caster.currentNode.worldPosition, wantedPosition, spell.positionsToTeleport[i]);
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
                ApplyEffects(eff, maanaSpent, caster, caster);
            }
            StartCoroutine(TeleportationSpellWaiter(spell, maanaSpent, caster, wantedPosition, targetPosition));
        }
        else
        {
            BattleUiManager.instance.DisplayErrorMessage("Aucun espace disponible pour attérir/se téléporter");
            CancelCurrentAction();
        }
    }

    private IEnumerator TeleportationSpellWaiter(CharacterActionTeleportation spell, int maanaSpent, RuntimeBattleCharacter characterToTeleport, Vector2 teleportPosition, Vector2 spellTargetPosition)
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
            UseAction(spell.jumpEffect, maanaSpent, characterToTeleport, characterToTeleport.currentNode.worldPosition, true);
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
            UseAction(spell.landEffect, maanaSpent, characterToTeleport, spellTargetPosition, true);
        }

        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(characterToTeleport.GetCurrentAnimation().clip.length);
        EndCurrentAction();
    }

    public static Vector2 GetTargetPosWithFacingPosition(Vector2 casterPos, Vector2 targetPos, Vector2 spellDirection)
    {
        Vector2 direction = Vector2.one;

        direction = new Vector2(casterPos.x, casterPos.y);
        direction = new Vector2(Grid.instance.NodeFromWorldPoint(targetPos).gridX, Grid.instance.NodeFromWorldPoint(targetPos).gridY) - direction;

        if (direction.y == 0 && direction.x == 0)
        {
            targetPos += new Vector2(spellDirection.x * 0.16f, spellDirection.y * 0.16f);
        }
        else if (direction.y > 0 && (Mathf.Abs(direction.y) > Mathf.Abs(direction.x) || direction.x == direction.y))
        {
            targetPos += new Vector2(spellDirection.x * 0.16f, spellDirection.y * 0.16f);
        }
        else if (direction.x < 0 && (Mathf.Abs(direction.x) > Mathf.Abs(direction.y) || direction.x == -direction.y))
        {
            targetPos += new Vector2(-spellDirection.y * 0.16f, spellDirection.x * 0.16f);
        }
        else if (direction.y < 0 && (Mathf.Abs(direction.y) > Mathf.Abs(direction.x) || direction.x == direction.y))
        {
            targetPos += new Vector2(-spellDirection.x * 0.16f, -spellDirection.y * 0.16f);
        }
        else
        {
            targetPos += new Vector2(spellDirection.y * 0.16f, -spellDirection.x * 0.16f);
        }

        return targetPos;
    }
}
