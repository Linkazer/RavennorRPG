using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;

public struct LaunchActionData
{
    public LaunchActionData(CharacterActionScriptable _wantedAction, int _maanaSpent, RuntimeBattleCharacter _caster, Vector2 _positionWanted, bool _effectAction)
    {
        wantedAction = _wantedAction;
        caster = _caster;
        maanaSpent = _maanaSpent;
        positionWanted = _positionWanted;
        effectAction = _effectAction;
    }

    public CharacterActionScriptable wantedAction;
    public RuntimeBattleCharacter caster;
    public int maanaSpent;
    public Vector2 positionWanted;
    public bool effectAction;
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    [SerializeField]
    private List<RuntimeBattleCharacter> roundList = new List<RuntimeBattleCharacter>();
    private int currentIndexTurn;
    private int currentTurn;

    [SerializeField]
    private List<PersonnageScriptables> playerTeam = new List<PersonnageScriptables>();

    [SerializeField]
    private List<RuntimeBattleCharacter> usableRuntimeCharacter = new List<RuntimeBattleCharacter>();
    [SerializeField]
    private List<RuntimeBattleCharacter> charaTeamOne = new List<RuntimeBattleCharacter>(), charaTeamTwo = new List<RuntimeBattleCharacter>();
    private RuntimeBattleCharacter currentCharacterTurn;

    public static Action<RuntimeBattleCharacter> characterTurnBegin;

    [SerializeField]
    private List<int> initiatives = new List<int>();

    private CharacterActionScriptable currentWantedAction;
    private RuntimeBattleCharacter currentCaster;
    private int currentMaanaSpent = 0;

    public GameObject level;
    private RoomManager roomManager;

    public bool battleState = false;

    [SerializeField]
    private BattleDiary diary;

    [SerializeField]
    private RuntimeBattleCharacter passiveCheater;

    private LaunchActionData actData = default;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<RVN_AudioSound> diceClip;

    public static int TurnNumber => instance.currentTurn;

    #region Set Up
    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        GameDices.SetRandomInit();

        if (RavenorGameManager.instance != null)
        {
            level = Instantiate(RavenorGameManager.instance.GetBattle());
            playerTeam = new List<PersonnageScriptables>();

            roomManager = level.GetComponent<RoomManager>();

            for(int i = 0; i < roomManager.characterInLevel.Count; i++)
            {
                playerTeam.Add(Instantiate(roomManager.characterInLevel[i]));
            }
        }

        roomManager = level.GetComponent<RoomManager>();

        roomManager.SetRoomManager();

        for (int i = 0; i < playerTeam.Count; i++)
        {
            if (i < roomManager.playerStartPositions.Count)
            {
                SetCharacter(playerTeam[i], roomManager.playerStartPositions[i]);
            }
        }

        roomManager.OpenRoom(0);

        foreach (RuntimeBattleCharacter r in usableRuntimeCharacter)
        {
            r.transform.position = new Vector2(-10, -10);
        }

        SortInitiativeList(initiatives, roundList, 0, initiatives.Count - 1);

        LoadingScreenManager.instance.HideScreen();

        if (roomManager.startDialogue != null)
        {
            BattleUiManager.instance.StartDialogue(roomManager.startDialogue);
        }
        else
        {
            BattleBegin();
        }

    }

    private void OnDestroy()
    {
        characterTurnBegin = null;
    }

    public void BattleBegin()
    {
        SoundSyst.ChangeMainMusic(RavenorGameManager.BattleClip);

        BattleUiManager.instance.SetUI();

        Grid.instance.CreateGrid();

        passiveCheater.SetRuntimeCharacterData(passiveCheater.GetCharacterDatas(), 10);

        NewCharacterRound(roundList[0]);
    }

    public void SpawnNewAllyCharacter(PersonnageScriptables newPerso, Vector2 position)
    {
        playerTeam.Add(newPerso);
        SpawnNewCharacter(newPerso, position);
    }

    public void SpawnNewCharacter(PersonnageScriptables newPerso, Vector2 position)
    {
        SetCharacter(newPerso, position);
        SortInitiativeList(initiatives, roundList, 0, initiatives.Count - 1);
    }

    int persoindexdebug;
    private void SetCharacter(PersonnageScriptables newPerso, Vector2 position)
    {
        int team = 0;
        if (playerTeam.Contains(newPerso))
        {
            charaTeamOne.Add(usableRuntimeCharacter[0]);
        }
        else
        {
            charaTeamTwo.Add(usableRuntimeCharacter[0]);
            team = 1;
        }
        persoindexdebug++;
        usableRuntimeCharacter[0].UseRuntimeCharacter(newPerso, team, position, persoindexdebug);
        roundList.Add(usableRuntimeCharacter[0]);
        usableRuntimeCharacter.RemoveAt(0);

        foreach(SpellEffectScriptables eff in roundList[roundList.Count - 1].GetCharacterDatas().passifs)
        {
            ApplyEffects(eff, 0, roundList[roundList.Count - 1], roundList[roundList.Count - 1]);
        }

        initiatives.Add(roundList[roundList.Count - 1].GetInitiative());
    }

    public void KillCharacter(RuntimeBattleCharacter toKill)
    {
        toKill.ResolveEffects(EffectTrigger.Die);

        if (toKill.GetCurrentHps() <= 0)
        {
            toKill.deathEvt?.Invoke();
            diary.AddText(toKill.name + " succombe.");

            toKill.SetAnimation("DeathAnim", toKill.Die);
            toKill.currentNode.chara = null;

            if (currentCharacterTurn == toKill && toKill.GetTeam() == 0)
            {
                EndTurn();
            }
            else
            {
                TimerSyst.CreateTimer(0.5f, () => CheckForBattleEnd());
            }
        }
    }

    public bool CheckForBattleEnd()
    {
        int deadCharacters = 0;
        for (int i = 0; i < charaTeamOne.Count; i++)
        {
            if (charaTeamOne[i].GetCurrentHps() <= 0)
            {
                deadCharacters++;
                if (deadCharacters >= roomManager.numberCharacterDeathLose)
                {
                    EndBattle(false);
                    return true;
                }
            }
        }

        return roomManager.CheckEndTurn();
    }

    public void EndBattle(bool doesWin)
    {
        PlayerBattleManager.instance.ActivatePlayerBattleController(false);
        battleState = true;

        foreach(RuntimeBattleCharacter runChara in charaTeamOne)
        {
            int boucleLength = runChara.GetAppliedEffects().Count;
            for (int i = 0; i < boucleLength; i++)
            {
                runChara.RemoveEffect(0);
            }
        }

        if(doesWin)
        {
            if (roomManager.nextLvl != null)
            {
                RavenorGameManager.instance.SetLocalNextBattle(roomManager.nextLvl);
                RavenorGameManager.AddUnlockLevel(RavenorGameManager.instance.GetCurrentBattle().levelInformation.ID);
            }

            if (roomManager.endDialogue != null)
            {
                BattleUiManager.instance.StartDialogue(roomManager.endDialogue);
            }
            else
            {
                SetWinPanel();
            }
        }
        else
        {
            BattleUiManager.instance.LoosingScreen();
            //ExitBattle();
        }
    }

    public void LoadBattle()
    {
        RavenorGameManager.instance.LoadBattle();
    }

    public void SetWinPanel()
    {
        //BattleUiManager.instance.WinningScreen();
        ExitBattle();
    }

    public void ExitBattle()
    {
        RavenorGameManager.instance.LoadMainMenu();
    }

    #endregion

    #region Turn Management
    public void NewCharacterRound(RuntimeBattleCharacter character)
    {
        currentCharacterTurn = character;

        for(int i = 0; i < roundList.Count; i++)
        {
            roundList[i].ResetVulnerabilityDangerosity();
            for (int j = 0; j < roundList.Count; j++)
            {
                if(j!=i)
                {
                    if(Pathfinding.instance.GetDistance(roundList[j].currentNode, roundList[i].currentNode) < 20)
                    {
                        if(roundList[j].GetTeam() == roundList[i].GetTeam())
                        {
                            roundList[i].AddDangerosity(5);
                        }
                        else
                        {
                            roundList[i].AddVulnerability(5);
                        }
                    }
                    else if (Pathfinding.instance.GetDistance(roundList[j].currentNode, roundList[i].currentNode) < 50)
                    {
                        if (roundList[j].GetTeam() == roundList[i].GetTeam())
                        {
                            roundList[i].AddDangerosity(2);
                        }
                        else
                        {
                            roundList[i].AddVulnerability(2);
                        }
                    }
                }
            }
        }

        characterTurnBegin?.Invoke(character);

        BattleUiManager.instance.SetNewTurn(currentIndexTurn, roundList);

        character.NewTurn();

        if (character.GetCurrentHps() > 0 && !character.CheckForAffliction(Affliction.Paralysie))
        {
            //Pathfinding.instance.SearchPath(Grid.instance.NodeFromWorldPoint(character.transform.position), null, false);

            if (character.GetTeam() == 0)
            {
                //Debug.Log("Player Turn: " + character);
                PlayerBattleManager.instance.NewPlayerTurn(character);
            }
            else
            {
                //Debug.Log("AI Turn : " + character);

                PlayerBattleManager.instance.ActivatePlayerBattleController(false);

                AiBattleManager.instance.BeginNewTurn(character);
            }
        }
        else
        {
            currentIndexTurn = (currentIndexTurn + 1) % roundList.Count;
            PlayerBattleManager.instance.ActivatePlayerBattleController(false);
            NewCharacterRound(roundList[currentIndexTurn]);
        }

        Grid.instance.CreateGrid();
    }

    public void EndTurn()
    {
        PlayerBattleManager.instance.ActivatePlayerBattleController(false);
        if((currentIndexTurn + 1) % roundList.Count == 0)
        {
            currentTurn++;

            for (int i = 0; i < roomManager.TurnEvents.Count; i++)
            {
                RoomTurnEvent turnEvt = roomManager.TurnEvents[i];
                if (turnEvt.turnIndex < 0 || turnEvt.turnIndex == currentTurn)
                {
                    turnEvt.PlayEvents();
                }
            }
        }

        if (!CheckForBattleEnd())
        {
            currentCharacterTurn.EndTurn();
            currentIndexTurn = (currentIndexTurn + 1) % roundList.Count;

            NewCharacterRound(roundList[currentIndexTurn]);
        }
    }
    #endregion

    #region Character Actions
    public void MoveCharacter(RuntimeBattleCharacter character, Vector2 destination, bool isForNextTurn)
    {
        Debug.Log(destination.ToString("F4"));
        AskToMove(character.gameObject, destination, character.movementLeft, isForNextTurn);
    }

    public static void EndCurrentAction()
    {
        instance.EndCurrentAction(instance.currentCharacterTurn);
    }

    public void EndCurrentAction(RuntimeBattleCharacter character)
    {
        if (character.IsAlive)
        {
            character.SetAnimation("Default");
        }
        character.ModifyCurrentNode(Grid.instance.NodeFromWorldPoint(character.transform.position));
        //Grid.instance.ResetUsableNode();
        if (character.GetTeam() == 0)
        {
            Pathfinding.instance.SearchPath(Grid.instance.NodeFromWorldPoint(currentCharacterTurn.transform.position), null, false);
            PlayerBattleManager.instance.ActivatePlayerBattleController(true);
        }
        else if(character.GetTeam() > -1)
        {
            AiBattleManager.instance.SearchNextMove(0.5f);
        }
    }

    private void CancelCurrentAction()
    {
        if(currentWantedAction.incantationTime != ActionIncantation.Rapide)
        {
            currentCharacterTurn.ResetOneAction();
        }
        EndCurrentAction();
    }

    /*public void IncantateAction(string animToPlay)
    {
        currentCharacterTurn.SetAnimation(animToPlay);
    }*/

    /*IEnumerator SpellIncantation(CharacterActionScriptable wantedAction, int maanaSpent, RuntimeBattleCharacter caster, Vector2 positionWanted, bool effectAction, float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        LaunchAction (wantedAction, maanaSpent, caster, positionWanted, effectAction);
    }*/

    public void LaunchAction(CharacterActionScriptable wantedAction, int maanaSpent, RuntimeBattleCharacter caster, Vector2 positionWanted, bool effectAction)
    {
        if (!effectAction)
        {
            caster.SetSpriteDirection((positionWanted.x < caster.transform.position.x));
        }
        if (effectAction || wantedAction.isWeaponBased)
        {
            UseAction(wantedAction, maanaSpent, caster, positionWanted, effectAction);
        }
        else
        {
            caster.SetAnimation("LaunchSpell", UseCurrentAction);
            actData = new LaunchActionData(wantedAction, maanaSpent, caster, positionWanted, effectAction);
        }
    }

    public void LaunchActionWithoutCaster(CharacterActionScriptable wantedAction, Vector2 positionWanted, bool effectAction)
    {
        UseAction(wantedAction, 0, passiveCheater, positionWanted, effectAction);
    }

    public void UseCurrentAction()
    {
        UseAction(actData.wantedAction, actData.maanaSpent, actData.caster, actData.positionWanted, actData.effectAction);
    }

    private void UseAction(CharacterActionScriptable wantedAction, int maanaSpent, RuntimeBattleCharacter caster, Vector2 positionWanted, bool effectAction)
    {
        if (!effectAction)
        {
            caster.useActionEvt?.Invoke();
            caster.ResolveEffects(EffectTrigger.DoAction);

            diary.AddText(caster.name + " utilise " + wantedAction.nom + ".");
        }

        currentWantedAction = wantedAction;
        currentCaster = caster;
        currentMaanaSpent = maanaSpent;

        if (wantedAction.incantationTime != ActionIncantation.Rapide && !effectAction)
        {
            caster.UseAction(wantedAction.isWeaponBased);
        }

        switch (wantedAction.SpellType)
        {
            case SpellType.Direct:
                if (wantedAction.projectile != null)
                {
                    BattleAnimationManager.instance.PlayProjectile(caster.transform.position, positionWanted, wantedAction.projectile, wantedAction.speed);
                }
                else
                {
                    DoAction((CharacterActionDirect)wantedAction, maanaSpent, caster, positionWanted, effectAction);
                }
                break;
            case SpellType.Invocation:
                InvokeAlly(caster, (CharacterActionInvocation)wantedAction, positionWanted);
                break;
            case SpellType.Teleportation:
                TeleportationSpell(caster, (CharacterActionTeleportation)wantedAction, maanaSpent, positionWanted);
                break;
            case SpellType.SimpleEffect:
                break;
        }
    }

    public void DoCurrentAction(Vector2 positionWanted)
    {
        DoAction((CharacterActionDirect)currentWantedAction, currentMaanaSpent, currentCaster, positionWanted, false);
    }

    public static List<Node> GetSpellUsableNodes(Node casterNode, CharacterActionScriptable spell)
    {
        List<Node> canSpellOn = Pathfinding.instance.GetNodesWithMaxDistance(casterNode, spell.range, false);

        if (spell.hasViewOnTarget)
        {
            for (int i = 1; i < canSpellOn.Count; i++)
            {
                if (!BattleManager.instance.IsNodeVisible(canSpellOn[0], canSpellOn[i]))
                {
                    canSpellOn.RemoveAt(i);
                    i--;
                }
            }
        }
        if (!spell.castOnSelf)
        {
            canSpellOn.RemoveAt(0);
        }

        return canSpellOn;
    }

    public static List<Node> GetHitNodes(Vector2 position, Vector2 casterPosition, CharacterActionScriptable spell)
    {
        Vector2 direction = Vector2.one;
        if (spell.doesFaceCaster)
        {
            direction = casterPosition;
            direction = new Vector2(Grid.instance.NodeFromWorldPoint(position).gridX, Grid.instance.NodeFromWorldPoint(position).gridY) - direction;
        }
        List<Vector2Int> spellZone = new List<Vector2Int>();
        foreach (Vector2Int vect in spell.activeZoneCases)
        {
            if (direction.y == 0 && direction.x == 0)
            {
                spellZone.Add(new Vector2Int(vect.x, vect.y));
            }
            else if (direction.y > 0 && (Mathf.Abs(direction.y) > Mathf.Abs(direction.x) || direction.x == direction.y))
            {
                spellZone.Add(new Vector2Int(vect.x, vect.y));
            }
            else if (direction.x < 0 && (Mathf.Abs(direction.x) > Mathf.Abs(direction.y) || direction.x == -direction.y))
            {
                spellZone.Add(new Vector2Int(-vect.y, vect.x));
            }
            else if (direction.y < 0 && (Mathf.Abs(direction.y) > Mathf.Abs(direction.x) || direction.x == direction.y))
            {
                spellZone.Add(new Vector2Int(-vect.x, -vect.y));
            }
            else
            {
                spellZone.Add(new Vector2Int(vect.y, -vect.x));
            }
        }

        return Grid.instance.GetZoneFromPosition(position, spellZone);
    }

    public void DoAction(CharacterActionDirect wantedAction, int maanaSpent, RuntimeBattleCharacter caster, Vector2 positionWanted, bool effectAction)
    {
        List<Node> hitNodes = GetHitNodes(positionWanted, new Vector2(caster.currentNode.gridX, caster.currentNode.gridY), wantedAction);

        switch (wantedAction.target)
        {
            case ActionTargets.EveryAllies:
                hitNodes = new List<Node>();
                List<RuntimeBattleCharacter> allyTeam = GetAllyTeamCharacters(caster.GetTeam());

                for (int i = 0; i < allyTeam.Count; i++)
                {
                    hitNodes.Add(allyTeam[i].currentNode);
                }
                break;
            case ActionTargets.EveryEnnemies:
                hitNodes = new List<Node>();
                List<RuntimeBattleCharacter> ennemyTeam = GetEnemyTeamCharacters(caster.GetTeam());
                for (int i = 0; i < ennemyTeam.Count; i++)
                {
                    hitNodes.Add(ennemyTeam[i].currentNode);
                }
                break;
        }

        if(wantedAction.zoneSprite != null)
        {
            if (effectAction)
            {
                BattleAnimationManager.instance.PlayOnNode(positionWanted, wantedAction.zoneSprite, wantedAction.caseFeedback, -1, wantedAction.soundToPlay);
            }
            else
            {
                BattleAnimationManager.instance.PlayOnNode(positionWanted, wantedAction.zoneSprite, wantedAction.caseFeedback, 0.5f, wantedAction.soundToPlay);
            }
        }

        if(wantedAction.wantedEffectOnCaster.Count>0)
        {
            foreach (SpellEffectScriptables eff in wantedAction.wantedEffectOnCaster)
            {
                ApplyEffects(eff, maanaSpent, caster, caster);
            }
        }

        List<Vector2> nodesPos = new List<Vector2>();

        foreach (Node n in hitNodes)
        {
            nodesPos.Add(n.worldPosition);

            if (n.HasCharacterOn && IsTargetAvailable(caster.GetTeam(), n.chara.GetTeam(), wantedAction.target,caster.GetInvocations().Contains(n.chara)))
            {
                ResolveSpell(wantedAction, maanaSpent, caster, n.chara, effectAction);
            }

            //Application d'effets sur le Sol
            if (wantedAction.wantedEffectOnGround.Count>0)
            {
                foreach (SpellEffectScriptables eff in wantedAction.wantedEffectOnGround)
                {
                    RuntimeSpellEffect runEffet = new RuntimeSpellEffect(
                    eff.effet,
                    maanaSpent,
                    eff.duree,
                    caster
                    );

                    n.AddEffect(eff, runEffet, caster);

                    if (eff.spriteCase != null)
                    {
                        BattleAnimationManager.instance.AddZoneEffect(n.worldPosition, eff.spriteCase, caster, eff.duree, runEffet);
                    }
                }
            }
        }

        if (wantedAction.caseSprite != null)
        {
            if (effectAction)
            {
                BattleAnimationManager.instance.PlayOnNode(nodesPos, wantedAction.caseSprite, wantedAction.caseFeedback, -1, wantedAction.soundToPlay);
            }
            else
            {
                BattleAnimationManager.instance.PlayOnNode(nodesPos, wantedAction.caseSprite, wantedAction.caseFeedback, 1f, wantedAction.soundToPlay);
            }
        }

        if (!effectAction && currentCharacterTurn == caster)
        {
            TimerSyst.CreateTimer(0.5f, EndCurrentAction);
        }
    }

    public bool IsActionAvailable(RuntimeBattleCharacter character, CharacterActionScriptable wantedAction)
    {
        if (character.GetSpellCooldown(wantedAction) > 0 || character.HasSpellUtilisationLeft(wantedAction))
        {
            return false;
        }

        if (character.GetCurrentMaana() < wantedAction.maanaCost)
        {
            return false;
        }

        if ((wantedAction.attackType != AttackType.Magical && character.CheckForAffliction(Affliction.Atrophie)) || (wantedAction.attackType == AttackType.Magical && character.CheckForAffliction(Affliction.Silence)))
        {
            return false;
        }

        switch (wantedAction.incantationTime)
        {
            case ActionIncantation.Rapide:
                return true;
            case ActionIncantation.Simple:
                if (character.CanDoAction())
                {
                    return true;
                }
                return false;
            case ActionIncantation.Lent:
                if (character.CanDoAction() && character.CanMove)
                {
                    return true;
                }
                return false;
            case ActionIncantation.Hard:
                if (character.CanDoAction())
                {
                    return true;
                }
                return false;
        }
        return false;
    }

    public bool IsTargetAvailable(int casterTeam, int targetTeam, ActionTargets wantedTarget, bool isInvocation)
    {
        switch (wantedTarget)
        {
            case ActionTargets.SelfAllies:
                if (casterTeam == targetTeam)
                {
                    return true;
                }
                return false;
            case ActionTargets.Ennemies:
                if (casterTeam != targetTeam)
                {
                    return true;
                }
                return false;
            case ActionTargets.EveryAllies:
                if (casterTeam == targetTeam)
                {
                    return true;
                }
                return false;
            case ActionTargets.EveryEnnemies:
                if (casterTeam != targetTeam)
                {
                    return true;
                }
                return false;
            case ActionTargets.All:
                return true;
            case ActionTargets.Invocations:
                return isInvocation;
        }
        return false;
    }
    #endregion

    #region Spell Resolution
    public void ResolveSpell(CharacterActionDirect wantedAction, int maanaSpent, RuntimeBattleCharacter caster, RuntimeBattleCharacter target, bool isEffectSpell)
    {
        int applyEffect = 0;
        if(wantedAction.hasPowerEffect)
        {
            if(wantedAction.damageType == DamageType.Heal)
            {
                DoHeal(wantedAction, caster, target);
            }
            else
            {
                applyEffect = DoDamage(wantedAction, caster, target);
                caster.TakeHeal(Mathf.CeilToInt(applyEffect * wantedAction.lifeStealPercent));
            }
        }


        if (wantedAction.wantedEffectOnTarget.Count > 0)
        {
            foreach (SpellEffectScriptables eff in wantedAction.wantedEffectOnTarget)
            {
                ApplyEffects(eff, maanaSpent, caster, target);
            }
        }

        if (applyEffect > 0)
        {
            if (!isEffectSpell)
            {
                caster.ResolveEffects(EffectTrigger.DamageDealSelf);
                caster.ResolveEffects(EffectTrigger.DamageDealTarget, target.currentNode.worldPosition);
            }
            
            if (wantedAction.wantedHitEffectOnTarget.Count > 0)
            {
                foreach (SpellEffectScriptables eff in wantedAction.wantedHitEffectOnTarget)
                {
                    if (!target.ContainsEffect(eff.effet))
                    {
                        ApplyEffects(eff, maanaSpent, caster, target);
                    }
                }
            }

            if (wantedAction.wantedHitEffectOnCaster.Count > 0)
            {
                foreach (SpellEffectScriptables eff in wantedAction.wantedHitEffectOnCaster)
                {
                    if (!caster.ContainsEffect(eff.effet))
                    {
                        ApplyEffects(eff, maanaSpent, caster, caster);
                    }
                }
            }
        }
    }

    public void DoHeal(CharacterActionDirect wantedAction, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        int baseHeal = wantedAction.GetBaseDamage() + caster.GetCharacterDatas().GetSoinApplique();

        if(!wantedAction.ignorePower && wantedAction.GetBaseDamage() != 0)
        {
            baseHeal += caster.GetCharacterDatas().GetPower();
        }

        int neededDices = wantedAction.GetDices(caster.GetCharacterDatas().GetPower());

        for (int i = 0; i < neededDices; i++)
        {
            if(GameDices.RollD6() > 3)
            {
                baseHeal++;
            }
        }

        target.TakeHeal(baseHeal);
    }

    public int DoDamage(CharacterActionDirect wantedAction, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        if (!audioSource.isPlaying)
        {
            SoundSyst.PlaySound(diceClip[UnityEngine.Random.Range(0, diceClip.Count)], audioSource);
        }

        return target.TakeDamage(wantedAction.damageType, DoesHit(wantedAction, caster, target), caster);
    }

    public int DoesHit(CharacterActionDirect wantedAction, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        int targetDefenseScore = 0;
        int dealtDamage = 0;

        List<int> diceValues = new List<int>();
        List<BattleDiceResult> diceResult = new List<BattleDiceResult>();

        int neededDices = wantedAction.GetDices(caster.GetCharacterDatas().GetPower());

        targetDefenseScore = target.GetCharacterDatas().GetDefense();

        int resultAtt = 0;
        for (int i = 0; i < neededDices; i++)
        {
            resultAtt = GameDices.RollD6() + caster.GetCharacterDatas().GetAccuracy();

            if(resultAtt <= targetDefenseScore)
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
            dealtDamage += wantedAction.GetBaseDamage() + caster.GetCharacterDatas().GetBaseDamage();
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
                    if (eff.trigger == triggerWanted)
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

                caster.AddInvocation(roundList[roundList.Count-1]);

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
        for(int i = possibleInvoc.Count-1; i >= 0; i--)
        {
            if(possibleInvoc[i] != null && i < 1)
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
            if(Grid.instance.NodeFromWorldPoint(possiblePosition).walkable)
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
        if(spell.isJump)
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

    #endregion

    //Variables Utilities
    #region Utilities
    public List<RuntimeBattleCharacter> GetAllChara()
    {
        return new List<RuntimeBattleCharacter>(roundList);
    }

    public List<RuntimeBattleCharacter> GetAllyTeamCharacters(int teamIndex)
    {
        if(teamIndex != 0)
        {
            return GetEnemyChara(true);
        }
        return GetPlayerChara();
    }

    public List<RuntimeBattleCharacter> GetEnemyTeamCharacters(int teamIndex)
    {
        if (teamIndex != 0)
        {
            return GetPlayerChara();
        }
        return GetEnemyChara(true);
    }

    public List<RuntimeBattleCharacter> GetPlayerChara()
    {
        List<RuntimeBattleCharacter> tPlayerTeam = new List<RuntimeBattleCharacter>();
        for(int i = 0; i < charaTeamOne.Count; i++)
        {
            if(charaTeamOne[i].IsAlive)
            {
                tPlayerTeam.Add(charaTeamOne[i]);
            }
        }
        return tPlayerTeam;
    }

    public List<RuntimeBattleCharacter> GetEnemyChara(bool needAlive)
    {
        List<RuntimeBattleCharacter> tEnnemyTeam = new List<RuntimeBattleCharacter>();
        for (int i = 0; i < charaTeamTwo.Count; i++)
        {
            if (charaTeamTwo[i].IsAlive || !needAlive)
            {
                tEnnemyTeam.Add(charaTeamTwo[i]);
            }
        }
        return tEnnemyTeam;
    }

    public RuntimeBattleCharacter GetCurrentTurnChara()
    {
        return roundList[currentIndexTurn];
    }

    public IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class//, IComparable<T>
    {
        List<T> objects = new List<T>();
        foreach (Type type in
            Assembly.GetAssembly(typeof(T)).GetTypes())
        {
            if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(T)))
            {
                objects.Add((T)Activator.CreateInstance(type, constructorArgs));
            }
        }
        //objects.Sort();
        return objects;
    }

    protected virtual void SortInitiativeList(List<int> initiativeList, List<RuntimeBattleCharacter> persoList, int start, int end)
    {
        Quick_Sort(initiativeList, persoList, start, end);
        persoList.Reverse();
        initiatives.Reverse();
        BattleUiManager.instance.SetNewTurn(currentIndexTurn, roundList);
    }

    private void Quick_Sort(List<int> arr, List<RuntimeBattleCharacter> persoList, int left, int right)
    {
        if (left < right)
        {
            int pivot = Partition(arr, persoList, left, right);

            Quick_Sort(arr, persoList, left, pivot - 1);
            Quick_Sort(arr, persoList, pivot + 1, right);
        }
    }

    private int Partition(List<int> array, List<RuntimeBattleCharacter> persoList, int low, int high)
    {
        //1. Select a pivot point.
        int pivot = array[high];
        int hpPivot = persoList[high].GetMaxHp;

        int lowIndex = (low - 1);

        //2. Reorder the collection.
        for (int j = low; j < high; j++)
        {
            if (array[j] < pivot)
            {
                lowIndex++;

                int temp = array[lowIndex];
                array[lowIndex] = array[j];
                array[j] = temp;

                RuntimeBattleCharacter tempChara = persoList[lowIndex];
                persoList[lowIndex] = persoList[j];
                persoList[j] = tempChara;
            }
            else if(array[j] == pivot && persoList[j].GetMaxHp < hpPivot)
            {
                lowIndex++;

                int temp = array[lowIndex];
                array[lowIndex] = array[j];
                array[j] = temp;

                RuntimeBattleCharacter tempChara = persoList[lowIndex];
                persoList[lowIndex] = persoList[j];
                persoList[j] = tempChara;
            }
        }

        int temp1 = array[lowIndex + 1];
        array[lowIndex + 1] = array[high];
        array[high] = temp1;

        RuntimeBattleCharacter tempChara1 = persoList[lowIndex+1];
        persoList[lowIndex+1] = persoList[high];
        persoList[high] = tempChara1;

        return lowIndex + 1;
    }

    public bool CanCameraGoNextDestination(Vector2 position)
    {
        return (position.x < roomManager.cameraMaxRightBottom.x && position.x > roomManager.cameraMaxLeftTop.x && position.y < roomManager.cameraMaxLeftTop.y && position.y > roomManager.cameraMaxRightBottom.y);
    }

    public Vector2 PossibleCameraDirection(Vector2 position)
    {
        Vector2 toReturn = Vector2.zero;
        if(position.x < roomManager.cameraMaxRightBottom.x && position.x > roomManager.cameraMaxLeftTop.x)
        {
            toReturn = new Vector2(1, toReturn.y);
        }
        if(position.y < roomManager.cameraMaxLeftTop.y && position.y > roomManager.cameraMaxRightBottom.y)
        {
            toReturn = new Vector2(toReturn.x, 1);
        }
        return toReturn;
    }

    public void OpenRoom(int index)
    {
        roomManager.OpenRoom(index);
    }


    public List<Node> CheckForOpportunityAttack(Node toCheck)
    {
        List<Node> nodeToCheck = Grid.instance.GetNeighbours(toCheck);

        List<Node> toReturn = new List<Node>();

        foreach (Node n in nodeToCheck)
        {
            if (n.HasCharacterOn && n.chara.GetTeam() != currentCharacterTurn.GetTeam())
            {
                toReturn.Add(n);
            }
        }

        return toReturn;
    }


    private void CheckForOpportunityAttack(Vector2 nextPosition)
    {
        if (!currentCharacterTurn.CheckForAffliction(Affliction.Evasion))
        {
            List<Node> nodeToCheck = CheckForOpportunityAttack(currentCharacterTurn.currentNode);
            foreach (Node n in nodeToCheck)
            {
                n.chara.AttackOfOpportunity(currentCharacterTurn);
            }
        }
    }

    public bool IsNodeVisible(Node startNode, Node targetNode)
    {
        int x = targetNode.gridX;
        int y = targetNode.gridY;
        int j = y;

        float realJ = y;

        int diffX = targetNode.gridX - startNode.gridX;
        int diffY = targetNode.gridY - startNode.gridY;

        float absX = Mathf.Abs((float)diffX);
        float absY = Mathf.Abs((float)diffY);

        int xCoef = 1;
        int yCoef = 1;

        if (diffX < 0)
        {
            xCoef = -1;
        }
        else if (diffX == 0)
        {
            xCoef = 0;
        }

        if (diffY < 0)
        {
            yCoef = -1;
        }
        else if (diffY == 0)
        {
            yCoef = 0;
        }

        if (absX == absY)
        {
            for (int i = x; i != startNode.gridX; i -= xCoef)
            {
                if (Grid.instance.GetNode(i, j).blockVision)
                {
                    return false;
                }

                realJ -= yCoef;
                j = Mathf.RoundToInt(realJ);
            }
        }
        else if (absX > absY)
        {
            for (int i = x; i != startNode.gridX; i -= xCoef)
            {
                if (Grid.instance.GetNode(i, j).blockVision)
                {
                    return false;
                }

                if (yCoef != 0 && diffY != 0)
                {
                    realJ -= (absY / absX) * (float)yCoef;
                    j = Mathf.RoundToInt(realJ);
                }
            }
        }
        else if (absX < absY)
        {
            realJ = x;
            j = x;
            for (int i = y; i != startNode.gridY; i -= yCoef)
            {
                if (Grid.instance.GetNode(j, i).blockVision)
                {
                    return false;
                }

                if (xCoef != 0 && diffX != 0)
                {
                    realJ -= (absX / absY) * (float)xCoef;
                    j = Mathf.RoundToInt(realJ);
                }
            }
        }

        return true;
    }

    #endregion

    #region Déplacement

    private GameObject toMove;

    public float speed = 1;
    [HideInInspector]
    public Vector3[] path;
    private Vector3 lastPathPosition;
    int targetIndex = -1;

    Vector2 posUnit, posTarget, direction;

    public void AskToMove(GameObject objectToMove, Vector3 destination, int maxDistance, bool isForNextTurn)
    {
        toMove = objectToMove;
        int distance = Pathfinding.instance.GetDistance(Grid.instance.NodeFromWorldPoint(objectToMove.transform.position), Grid.instance.NodeFromWorldPoint(destination));
        currentCharacterTurn.movedEvt.Invoke(distance);
        PathRequestManager.RequestPath(objectToMove.transform.position, destination, maxDistance, isForNextTurn, OnPathFound);
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            targetIndex = 0;
            CheckForOpportunityAttack(path[0]);
            currentCharacterTurn.SetAnimation("Moving");
            Grid.instance.ResetNodeFeedback();
            StopCoroutine(FollowPath());
            StartCoroutine(FollowPath());
        }
        else
        {
            Debug.Log(currentCharacterTurn + "No Path");
            if(currentCharacterTurn.GetTeam() != 0)
            {
                EndTurn();
            }
            else
            {
                EndCurrentAction(currentCharacterTurn);
            }
        }
    }

    IEnumerator FollowPath()
    {
        if (path.Length > 0)
        {
            Vector3 currentWaypoint = path[0];

            if (toMove == currentCharacterTurn.gameObject)
            {
                currentCharacterTurn.SetSpriteDirection((currentWaypoint.x < toMove.transform.position.x));
            }

            while (true)
            {
                posUnit = new Vector2(toMove.transform.position.x, toMove.transform.position.y);
                posTarget = new Vector2(currentWaypoint.x, currentWaypoint.y);
                if (Vector2.Distance(posUnit, posTarget) < (0.01f * speed))
                {
                    targetIndex++;
                    if (targetIndex >= path.Length || !currentCharacterTurn.IsAlive)
                    {
                        Grid.instance.CreateGrid();
                        EndCurrentAction(currentCharacterTurn);
                        yield break;
                    }

                    if (toMove == currentCharacterTurn.gameObject)
                    {
                        currentCharacterTurn.SetSpriteDirection((path[targetIndex].x < toMove.transform.position.x));
                        currentCharacterTurn.ModifyCurrentNode(Grid.instance.NodeFromWorldPoint(currentWaypoint));
                        CheckForOpportunityAttack(path[targetIndex]);
                    }

                    currentWaypoint = path[targetIndex];
                    lastPathPosition = currentWaypoint;
                }
                direction = new Vector2(currentWaypoint.x - toMove.transform.position.x, currentWaypoint.y - toMove.transform.position.y).normalized;

                toMove.transform.position += new Vector3(direction.x, direction.y, 0) * speed * Time.deltaTime;

                yield return null;
            }
        }
    }

    public void OnDrawGizmos()
    {
        if (path != null && targetIndex >= 0)
        {
            for (int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one * 0.1f);

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }

    #endregion
}
