using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    [SerializeField]
    private List<RuntimeBattleCharacter> roundList = new List<RuntimeBattleCharacter>();
    private int currentIndexTurn;

    [SerializeField]
    private List<PersonnageScriptables> teamOne = new List<PersonnageScriptables>();//, teamTwo = new List<PersonnageScriptables>();

    [SerializeField]
    private List<RuntimeBattleCharacter> usableRuntimeCharacter = new List<RuntimeBattleCharacter>();
    [SerializeField]
    private List<RuntimeBattleCharacter> charaTeamOne = new List<RuntimeBattleCharacter>(), charaTeamTwo = new List<RuntimeBattleCharacter>();
    private RuntimeBattleCharacter currentCharacterTurn;

    public static UnityEvent TurnBeginEvent = new UnityEvent();

    private List<int> initiatives = new List<int>();

    private CharacterActionScriptable currentWantedAction;
    private RuntimeBattleCharacter currentCaster, currentTarget;

    public GameObject level;
    private RoomManager roomManager;

    public bool battleState = false;

    [SerializeField]
    private BattleDiary diary;

    [SerializeField]
    private RuntimeBattleCharacter passiveCheater;

    [SerializeField]
    private AudioClip storyMusic, battleMusic;
    [SerializeField]
    private AudioSource backgroundSource;

    #region Set Up
    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        if (RavenorGameManager.instance != null)
        {
            level = RavenorGameManager.instance.GetBattle();
            teamOne = new List<PersonnageScriptables>();

            roomManager = level.GetComponent<RoomManager>();

            foreach (PersonnageScriptables perso in RavenorGameManager.instance.playerPersos)
            {
                if(roomManager.characterInLevel.Contains(perso.nom))
                {
                    teamOne.Add(perso);
                }
            }
        }

        roomManager = level.GetComponent<RoomManager>();

        Camera.main.transform.position = roomManager.cameraPos;
        Camera.main.transform.position += new Vector3(0, 0, - 10);

        Instantiate(level);

        for (int i = 0; i < teamOne.Count; i++)
        {
            if (i < roomManager.playerStartPositions.Count)
            {
                SetCharacter(teamOne[i], roomManager.playerStartPositions[i]);
            }
        }

        /*for (int i = 0; i < teamTwo.Count; i++)
        {
            if (i < RoomManager.instance.ennemiesStartPosition.Count)
            {
                SetCharacter(teamTwo[i], RoomManager.instance.ennemiesStartPosition[i]);
            }
        }*/

        roomManager.OpenRoom(0);

        foreach (RuntimeBattleCharacter r in usableRuntimeCharacter)
        {
            r.transform.position = new Vector2(-10, -10);
        }

        SortInitiativeList(initiatives, roundList, 0, initiatives.Count - 1);

        //Grid.instance.SetAllUsableNodes();
        //Grid.instance.ResetUsableNode();

        //PlayerBattleManager.instance.ActivatePlayerBattleController(false);

        roomManager.SetRoomManager();

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

    public void BattleBegin()
    {
        backgroundSource.clip = battleMusic;
        backgroundSource.Play();

        BattleUiManager.instance.SetUI();

        Grid.instance.CreateGrid();

        NewCharacterRound(roundList[0]);
    }

    public void SpawnNewCharacter(PersonnageScriptables newPerso, Vector2 position)
    {
        SetCharacter(newPerso, position);
        SortInitiativeList(initiatives, roundList, 0, initiatives.Count - 1);
    }

    private void SetCharacter(PersonnageScriptables newPerso, Vector2 position)
    {
        int team = 0;
        if (teamOne.Contains(newPerso))
        {
            charaTeamOne.Add(usableRuntimeCharacter[0]);
            team = 1;
        }
        else
        {
            charaTeamTwo.Add(usableRuntimeCharacter[0]);
        }
        usableRuntimeCharacter[0].UseRuntimeCharacter(newPerso, team, position);
        roundList.Add(usableRuntimeCharacter[0]);
        usableRuntimeCharacter.RemoveAt(0);

        foreach(SpellEffectScriptables eff in roundList[roundList.Count - 1].GetCharacterDatas().passifs)
        {
            ApplyEffects(eff, roundList[roundList.Count - 1], roundList[roundList.Count - 1]);
        }

        initiatives.Add(roundList[roundList.Count - 1].GetInitiative());
    }

    public void KillCharacter(RuntimeBattleCharacter toKill)
    {
        toKill.deathEvt.Invoke();
        toKill.ResolveEffect(EffectTrigger.Die);

        diary.AddText(toKill.name + " succombe.");

        toKill.gameObject.SetActive(false);

        if(currentCharacterTurn == toKill && toKill.GetTeam()==1)
        {
            EndTurn();
        }
    }

    public bool CheckForBattleEnd()
    {
        for (int i = 0; i < charaTeamOne.Count; i++)
        {
            if (charaTeamOne[i].GetCurrentHps() > 0)
            {
                break;
            }
            else if (i == charaTeamOne.Count - 1)
            {
                EndBattle(false);
                return true;
            }
        }

        return roomManager.CheckForEnd();
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
            backgroundSource.clip = storyMusic;
            backgroundSource.Play();

            Debug.Log("Won");
            if (roomManager.endDialogue != null)
            {
                BattleUiManager.instance.StartDialogue(roomManager.endDialogue);
            }
            else
            {
                if(roomManager.levelAtEnd != 0)
                {
                    foreach(PersonnageScriptables p in RavenorGameManager.instance.playerPersos)
                    {
                        p.SetLevel(roomManager.levelAtEnd);
                        RavenorGameManager.instance.SetLevelUp();
                    }
                }

                ExitBattle();

            }
        }
        else
        {
            ExitBattle();
            Debug.Log("Lose");
        }
    }

    public void ExitBattle()
    {
        if (level.GetComponent<RoomManager>().nextLvl != null)
        {
            RavenorGameManager.instance.SetNextBattle(level.GetComponent<RoomManager>().nextLvl);
        }

        if (roomManager.endGame)
        {
            RavenorGameManager.instance.LoadMainMenu();
        }
        else
        {
            RavenorGameManager.instance.LoadCamp();
        }
    }

    #endregion

    #region Turn Management
    public void NewCharacterRound(RuntimeBattleCharacter character)
    {
        Grid.instance.CreateGrid();

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

        TurnBeginEvent.Invoke();

        BattleUiManager.instance.SetNewTurn(currentIndexTurn, roundList);

        character.NewTurn();

        if (character.GetCurrentHps() > 0 && !character.CheckForAffliction(Affliction.Paralysie))
        {
            Pathfinding.instance.SetAllNodes(Grid.instance.NodeFromWorldPoint(character.transform.position), null);

            if (character.GetTeam() == 1)
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
    }

    public void EndTurn()
    {
        PlayerBattleManager.instance.ActivatePlayerBattleController(false);
        if (!CheckForBattleEnd())
        {
            currentIndexTurn = (currentIndexTurn + 1) % roundList.Count;
            NewCharacterRound(roundList[currentIndexTurn]);
        }
    }
    #endregion

    #region Character Actions
    public void MoveCharacter(RuntimeBattleCharacter character, Vector2 destination)
    {
        if (character.GetTeam() != 1)
        {
            AskToMove(character.gameObject, destination, character.movementLeft);
        }
        else
        {
            AskToMove(character.gameObject, destination, character.movementLeft +50);
        }
    }
    
    public void EndCurrentActionWithDelay(float timeDelay)
    {
        StartCoroutine(WaitForActionToEnd(timeDelay));
    }

    IEnumerator WaitForActionToEnd(float timeDelay)
    {
        yield return new WaitForSeconds(timeDelay);
        EndCurrentAction(currentCharacterTurn);
    }

    public void EndCurrentAction()
    {
        EndCurrentAction(currentCharacterTurn);
    }

    public void EndCurrentAction(RuntimeBattleCharacter character)
    {
        character.SetAnimation("Default");
        character.currentNode = Grid.instance.NodeFromWorldPoint(character.transform.position);
        //Grid.instance.ResetUsableNode();
        if (character.GetTeam() == 1)
        {
            Pathfinding.instance.SetAllNodes(Grid.instance.NodeFromWorldPoint(currentCharacterTurn.transform.position), null);
            PlayerBattleManager.instance.ActivatePlayerBattleController(true);
        }
        else if(character.GetTeam() > -1)
        {
            AiBattleManager.instance.SearchNextMove(0.5f);
        }
    }

    public void IncantateAction(string animToPlay)
    {
        currentCharacterTurn.SetAnimation(animToPlay);
        //SpellIncantation()
    }

    IEnumerator SpellIncantation(CharacterActionScriptable wantedAction, RuntimeBattleCharacter caster, Vector2 positionWanted, bool effectAction, float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        LaunchAction (wantedAction, caster, positionWanted, effectAction);
    }

    public void LaunchAction(CharacterActionScriptable wantedAction, RuntimeBattleCharacter caster, Vector2 positionWanted, bool effectAction)
    {
        caster.SetSpriteDirection((positionWanted.x < caster.transform.position.x));

        caster.useActionEvt.Invoke();
        caster.ResolveEffect(EffectTrigger.DoAction);

        currentWantedAction = wantedAction;
        currentCaster = caster;

        Debug.Log(caster + " use action " + wantedAction);
        diary.AddText(caster.name + " utilise " + wantedAction.nom);

        switch(wantedAction.spellType)
        {
            case SpellType.Direct:
                if (wantedAction.projectile != null)
                {
                    BattleAnimationManager.instance.PlayProjectile(caster.transform.position, positionWanted, wantedAction.projectile, wantedAction.speed);
                }
                else
                {
                    DoAction((CharacterActionDirect)wantedAction, caster, positionWanted, effectAction);
                }
                break;
            case SpellType.Invocation:
                InvokeAlly(caster, (CharacterActionInvocation)wantedAction, positionWanted);
                break;
        }
    }

    public void DoCurrentAction(Vector2 positionWanted)
    {
        DoAction((CharacterActionDirect)currentWantedAction, currentCaster, positionWanted, false);
    }

    public void DoAction(CharacterActionDirect wantedAction, RuntimeBattleCharacter caster, Vector2 positionWanted, bool effectAction)
    {
        Vector2 direction = Vector2.one;
        if (wantedAction.doesFaceCaster)
        {
            direction = new Vector2(caster.currentNode.gridX, caster.currentNode.gridY);
            direction = new Vector2(Grid.instance.NodeFromWorldPoint(positionWanted).gridX, Grid.instance.NodeFromWorldPoint(positionWanted).gridY) - direction;
        }
        List<Vector2Int> spellZone = new List<Vector2Int>();
        foreach (Vector2Int vect in wantedAction.activeZoneCases)
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

        List<Node> hitNodes = Grid.instance.GetZoneFromPosition(positionWanted, spellZone);

        /*if((!hitNodes[0].hasCharacterOn && wantedAction.castTarget != ActionTargets.All) && !(IsTargetAvailable(caster.GetTeam(),hitNodes[0].chara.GetTeam(),wantedAction.castTarget, caster.GetInvocations().Contains(hitNodes[0].chara))))
        {
            EndCurrentActionWithDelay(0.2f);
            return;
        }*/

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

        if (wantedAction.wantedEffectOnGround.Count>0)
        {
            foreach (SpellEffectScriptables eff in wantedAction.wantedEffectOnGround)
            {
                BattleAnimationManager.instance.AddZoneEffect(positionWanted, eff.spriteZone, caster, eff.duree, eff.effet);
            }
        }

        //wantedAction.SpecialAction();

        if(wantedAction.wantedEffectOnCaster.Count>0)
        {
            foreach (SpellEffectScriptables eff in wantedAction.wantedEffectOnCaster)
            {
                ApplyEffects(eff, caster, caster);
            }
        }

        List<Vector2> nodesPos = new List<Vector2>();

        foreach (Node n in hitNodes)
        {
            nodesPos.Add(n.worldPosition);

            if (n.hasCharacterOn && IsTargetAvailable(caster.GetTeam(), n.chara.GetTeam(), wantedAction.target,caster.GetInvocations().Contains(n.chara)))
            {
                ResolveSpell(wantedAction, caster, n.chara);
            }

            //Application d'effets sur le Sol
            if (wantedAction.wantedEffectOnGround.Count>0)
            {
                foreach (SpellEffectScriptables eff in wantedAction.wantedEffectOnGround)
                {
                    RuntimeSpellEffect runEffet = new RuntimeSpellEffect(
                        eff.effet,
                        eff.duree,
                        caster
                        );

                    foreach (SpellEffect effS in runEffet.effet.effects)
                    {
                        SetEffectValues(effS, caster);
                    }

                    n.AddEffect(runEffet, caster);

                    if (eff.spriteCase != null)
                    {
                        BattleAnimationManager.instance.AddZoneEffect(n.worldPosition, eff.spriteCase, caster, eff.duree, eff.effet);
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
                BattleAnimationManager.instance.PlayOnNode(nodesPos, wantedAction.caseSprite, wantedAction.caseFeedback, 0.5f, wantedAction.soundToPlay);
            }
        }

        if (wantedAction.incantationTime != ActionIncantation.Rapide)
        {
            caster.actionAvailable = false;
        }

        if(!wantedAction.HadFeedback() && !effectAction)
        {
            EndCurrentActionWithDelay(0.2f);
        }
    }

    public bool IsActionAvailable(RuntimeBattleCharacter character, CharacterActionScriptable wantedAction)
    {
        if(character.GetSpellCooldown(wantedAction) > 0)
        {
            return false;
        }

        if(character.GetCurrentMaana() < wantedAction.maanaCost)
        {
            return false;
        }

        if ((wantedAction.attackType != AttackType.PuissMagique && character.CheckForAffliction(Affliction.Atrophie)) || (wantedAction.attackType == AttackType.PuissMagique && character.CheckForAffliction(Affliction.Silence)))
        {
            return false;
        }

        switch (wantedAction.incantationTime)
        {
            case ActionIncantation.Rapide:
                return true;
            case ActionIncantation.Simple:
                if (character.actionAvailable)
                {
                    return true;
                }
                return false;
            case ActionIncantation.Lent:
                if (character.actionAvailable && !character.hasMoved)
                {
                    return true;
                }
                return false;
            case ActionIncantation.Hard:
                if (character.actionAvailable)
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
            case ActionTargets.All:
                return true;
            case ActionTargets.Invocations:
                return isInvocation;
        }
        return false;
    }
    #endregion

    #region Spell Resolution
    public void ResolveSpell(CharacterActionDirect wantedAction, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
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
        else
        {
            applyEffect = 1;
        }

        if (applyEffect > 0 && wantedAction.wantedEffectOnTarget.Count>0)
        {
            foreach (SpellEffectScriptables eff in wantedAction.wantedEffectOnTarget)
            {
                ApplyEffects(eff, caster, target);
            }
        }
    }

    public void DoHeal(CharacterActionDirect wantedAction, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        List<Dice> neededDices = new List<Dice>(wantedAction.GetDices());
        if (wantedAction.GetLevelBonusDices(caster.GetCharacterDatas().level) != null)
        {
            neededDices.Add(wantedAction.GetLevelBonusDices(caster.GetCharacterDatas().level));
        }

        Debug.Log("Heal Dices : " + neededDices.Count);

        target.TakeHeal(neededDices, (int)caster.GetCharacterDatas().GetSoinApplique());
    }

    public int DoDamage(CharacterActionDirect wantedAction, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        int bonusDamages = 0;

        switch (wantedAction.attackType)
        {
            case AttackType.Force:
                bonusDamages += (int)caster.GetCharacterDatas().GetPhysicalDamageMelee();
                break;
            case AttackType.Dexterite:
                Debug.Log("Attack dex");
                bonusDamages += (int)caster.GetCharacterDatas().GetPhysicalDamageDistance();
                break;
            case AttackType.PuissMagique:
                bonusDamages += (int)caster.GetCharacterDatas().GetMagicalDamage();
                break;
        }
        return target.TakeDamage(wantedAction.damageType, DoesHit(wantedAction, caster, target), wantedAction.noBonusSpell? 0 : bonusDamages);
    }

    public List<Dice> DoesHit(CharacterActionDirect wantedAction, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        int targetDefenseScore = 0, casterHitScore = 0;

        List<Dice> neededDices = new List<Dice>(wantedAction.GetDices());
        if (wantedAction.GetLevelBonusDices(caster.GetCharacterDatas().level) != null)
        {
            neededDices.Add(wantedAction.GetLevelBonusDices(caster.GetCharacterDatas().level));
        }

        EffectType wantedDiceBonus = EffectType.Agilite;

        //float damage = wantedAction.DamageRoll(caster.GetCharacterDatas().level);

        /* A mettre à jour avec les Armures
         * if (wantedAction.damageType != DamageType.Brut)
        {
            targetDefense = target.currentScriptable.GetDefense();
        }*/

        //Jet de la défense
        int defenseLucky = 0, attackLucky = 0;
        targetDefenseScore = target.GetCharacterDatas().GetBrutDefense();//AttackRoll(target.GetCharacterDatas().GetDefenseDice(), DiceType.D4, target.GetCharacterDatas().GetBrutDefense(), 1, out defenseLucky);

        //Jet de l'attaque
        switch (wantedAction.attackType)
        {
            case AttackType.Force:
                casterHitScore = AttackRoll(caster.GetCharacterDatas().GetTouchDices(1), DiceType.D6, caster.GetCharacterDatas().GetBrutToucheMelee(), caster.GetCharacterDatas().GetCriticalChanceBonus(), out attackLucky);
                wantedDiceBonus = EffectType.PhysicalMeleDamage;
                //damage += caster.GetCharacterDatas().GetPhysicalDamageMelee();
                break;
            case AttackType.Dexterite:
                casterHitScore = AttackRoll(caster.GetCharacterDatas().GetTouchDices(2), DiceType.D6, caster.GetCharacterDatas().GetBrutToucheDistance(), caster.GetCharacterDatas().GetCriticalChanceBonus(), out attackLucky);
                //damage += caster.GetCharacterDatas().GetPhysicalDamageDistance();
                break;
            case AttackType.PuissMagique:
                casterHitScore = AttackRoll(caster.GetCharacterDatas().GetTouchDices(3), DiceType.D6, caster.GetCharacterDatas().GetBrutToucheMagical(), caster.GetCharacterDatas().GetCriticalChanceBonus(), out attackLucky);
                wantedDiceBonus = EffectType.MagicalDamage;
                //damage += caster.GetCharacterDatas().GetMagicalDamage();
                break;
        }

        //Résolution des Touches et des LuckyDices
        if (casterHitScore < targetDefenseScore)
        {
            if (attackLucky > 0)
            {
                switch (wantedAction.attackType)
                {
                    case AttackType.Force:
                        casterHitScore = NormalRoll(caster.GetCharacterDatas().GetTouchDices(1), caster.GetCharacterDatas().GetBrutToucheMelee(),DiceType.D6);
                        break;
                    case AttackType.Dexterite:
                        casterHitScore = NormalRoll(caster.GetCharacterDatas().GetTouchDices(2), caster.GetCharacterDatas().GetBrutToucheDistance(), DiceType.D6);
                        break;
                    case AttackType.PuissMagique:
                        casterHitScore = NormalRoll(caster.GetCharacterDatas().GetTouchDices(3), caster.GetCharacterDatas().GetBrutToucheMagical(), DiceType.D6);
                        break;
                }
            }
            else
            {
                diary.AddText(currentCaster.name + " rate son action (" + casterHitScore + " vs " + targetDefenseScore + ")");
                return new List<Dice>();
            }
        }
        else if (defenseLucky > 0)
        {
            //targetDefenseScore = NormalRoll(target.GetCharacterDatas().GetDefenseDice(), target.GetCharacterDatas().GetBrutDefense(), DiceType.D4);
        }
        

        if(casterHitScore >= targetDefenseScore)
        {
            //Récupération de tous les Dé nécessaires aux dégâts

            if (attackLucky > 0 || wantedAction.autoCritical)
            {
                float critBonus = caster.GetCharacterDatas().GetCriticalDamageMultiplier();
                Debug.Log("Does crit : " + critBonus);
                int diceNb = neededDices.Count;

                for(int i = 0; i < diceNb; i++)
                {
                    int numberDices = (int)(neededDices[i].numberOfDice * critBonus) - neededDices[i].numberOfDice;
                    if(numberDices < 0)
                    {
                        numberDices = 0;
                    }
                    neededDices.Add(new Dice(neededDices[i].wantedDice, numberDices, neededDices[i].wantedDamage));
                }
            }

            foreach (Dice d in caster.GetCharacterDatas().GetBonusDice(wantedDiceBonus))
            {
                neededDices.Add(d);
            }

            diary.AddText(currentCaster.name + " touche (" + casterHitScore + " vs " + targetDefenseScore + ")");

            return neededDices;
        }
        else
        {
            diary.AddText(currentCaster.name + " rate son action (" + casterHitScore + " vs " + targetDefenseScore + ")");
        }

        Debug.Log("Nothing happen ? : " + casterHitScore +" >= "+targetDefenseScore);
        return new List<Dice>();
    }

    /*public void ApplyEffects(CharacterActionDirect wantedAction, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        if (wantedAction.wantedEffect != null)
        {
            BattleDiary.instance.AddText(target.name + " est affecté par " + wantedAction.nom);

            RuntimeSpellEffect runEffet = new RuntimeSpellEffect(
                wantedAction.wantedEffect.effet,
                wantedAction.wantedEffect.duree,
                caster
                );

            foreach (SpellEffect eff in runEffet.effet.effects)
            {
                SetEffectValues(eff, caster);
            }

            target.AddEffect(runEffet);

            foreach (SpellEffectScriptables eff in wantedAction.wantedEffect.bonusToCancel)
            {
                target.RemoveEffect(eff);
            }

            ResolveEffect(runEffet.effet, target, EffectTrigger.Apply);
        }
    }*/

    public void ApplyEffects(SpellEffectScriptables wantedEffect, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        RuntimeSpellEffect runEffet = new RuntimeSpellEffect(
            wantedEffect.effet,
            wantedEffect.duree,
            caster
            );

        foreach (SpellEffect eff in runEffet.effet.effects)
        {
            SetEffectValues(eff, caster);
        }

        target.AddEffect(runEffet);

        foreach(SpellEffectScriptables eff in wantedEffect.bonusToCancel)
        {
            target.RemoveEffect(eff);
        }

        ResolveEffect(runEffet.effet, target, EffectTrigger.Apply);
    }

    public void SetEffectValues(SpellEffect effet, RuntimeBattleCharacter caster)
    {
        effet.value = (int)((effet.value + effet.scaleByLevel * caster.GetCharacterDatas().level));
    }

    public void ResolveEffect(SpellEffectCommon effect, Vector2 positionWanted, EffectTrigger triggerWanted, int stack)
    {
        for (int i = 0; i < stack; i++)
        {

            if (Grid.instance.NodeFromWorldPoint(positionWanted).hasCharacterOn)
            {
                RuntimeBattleCharacter target = Grid.instance.NodeFromWorldPoint(positionWanted).chara;
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
                    LaunchAction(effAct.spellToUse, effAct.caster, positionWanted, true);
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
                LaunchAction(effAct.spellToUse, effAct.caster, target.transform.position, true);
            }
        }
    }

    /*public void ApplyTimeEffect(SpellEffectCommon effect, RuntimeBattleCharacter target)
    {
        //...
    }*/

    private void InvokeAlly(RuntimeBattleCharacter caster, CharacterActionInvocation spell, Vector2 wantedPosition)
    {
        PersonnageScriptables toInvoke = spell.invocation;

        if (!caster.CheckForInvocations(toInvoke))
        {
            Node nodeWanted = Grid.instance.NodeFromWorldPoint(wantedPosition);
            if (nodeWanted.usableNode && !nodeWanted.hasCharacterOn)
            {
                teamOne.Add(toInvoke);
                SetCharacter(toInvoke, wantedPosition);

                caster.AddInvocation(roundList[roundList.Count-1]);

                SortInitiativeList(initiatives, roundList, 0, initiatives.Count - 1);

                if (roundList[roundList.Count - 1].GetInitiative() < currentCharacterTurn.GetInitiative())
                {
                    //currentIndexTurn++;
                }

                Grid.instance.CreateGrid();
            }
        }
        EndCurrentAction();
    }

    #endregion

    #region Dice Rolling
    public int AttackRoll(int diceNumber, DiceType wantedDice, int bonus, int luckyDiceNumber, out int luckyDiceResult)
    {
        luckyDiceResult = 0;
        int result = bonus;
        for(int i = 0; i <diceNumber; i++)
        {
            int currentDice = 0;
            switch (wantedDice)
            {
                case DiceType.D4:
                    currentDice = GameDices.RollD4();
                    break;
                case DiceType.D6:
                    currentDice = GameDices.RollD6();
                    break;
            }


            if(i<luckyDiceNumber)
            {
                if(i == 0 && currentDice == 1)
                {
                    luckyDiceResult = -1;
                }
                else if(currentDice == 6)
                {
                    luckyDiceResult++;
                }
            }

            result += currentDice;
        }
        return result;
    }

    /*public int AttackRoll(List<Dice> diceToUse, int bonus, int luckyDiceNumber, out int luckyDiceResult)
    {
        int value = bonus;
        luckyDiceResult = 0;

        for(int i = 0; i < diceToUse.Count; i++)
        {
            int newVal = GameDices.RollDice(diceToUse[i].numberOfDice, diceToUse[i].wantedDice);
            if (i == 1)
            {
                if(newVal == 1)
                {
                    luckyDiceResult = -1;
                }
            }

            if(i<luckyDiceNumber && newVal == 6 && luckyDiceResult >= 0)
            {
                luckyDiceResult = 1;
            }

            value += newVal;
        }

        return value;
    }*/

    public int NormalRoll(int diceNumber, int bonus, DiceType dice)
    {
        return GameDices.RollDice(diceNumber, dice)+bonus;
    }

    public int NormalRoll(List<Dice> diceToUse, int bonus)
    {
        int value = bonus;

        for (int i = 0; i < diceToUse.Count; i++)
        {
            value += GameDices.RollDice(diceToUse[i].numberOfDice, diceToUse[i].wantedDice);
        }

        return value;
    }

    #endregion

    //Variables Utilities
    #region Utilities
    public List<RuntimeBattleCharacter> GetAllChara()
    {
        return new List<RuntimeBattleCharacter>(roundList);
    }

    public List<RuntimeBattleCharacter> GetTeamOne()
    {
        return new List<RuntimeBattleCharacter>(charaTeamOne);
    }

    public List<RuntimeBattleCharacter> GetTeamTwo()
    {
        return new List<RuntimeBattleCharacter>(charaTeamTwo);
    }

    public RuntimeBattleCharacter GetCurrentTurnChara()
    {
        return roundList[currentIndexTurn];
    }

    /*private bool IsValidTarget(RuntimeBattleCharacter caster, RuntimeBattleCharacter target, ActionTargets targetType)
    {
        return (targetType == ActionTargets.All || (targetType == ActionTargets.Ennemies && caster.GetTeam() != target.GetTeam()) || (targetType == ActionTargets.SelfAllies && caster.GetTeam() == target.GetTeam()));
    }*/
    #endregion

    //Get Character team


    //Kill Someone

    #region Utilities
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
        if (start < end)
        {
            int pivot = initiativeList[end];
            int pIndex = start;
            int swap;
            RuntimeBattleCharacter pSwap;

            for (int i = start; i < end; i++)
            {
                if (initiativeList[i] > pivot)
                {
                    swap = initiativeList[pIndex];
                    initiativeList[pIndex] = initiativeList[i];
                    initiativeList[i] = swap;

                    pSwap = persoList[pIndex];
                    persoList[pIndex] = persoList[i];
                    persoList[i] = pSwap;

                    pIndex++;
                }
            }

            initiativeList[end] = initiativeList[pIndex];
            initiativeList[pIndex] = pivot;

            SortInitiativeList(initiativeList, persoList, start, pIndex - 1);
            SortInitiativeList(initiativeList, persoList, pIndex + 1, end);
        }
    }

    public bool CanCameraGoNextDestination(Vector2 position)
    {
        return (position.x < roomManager.cameraMaxRightBottom.x && position.x > roomManager.cameraMaxLeftTop.x && position.y < roomManager.cameraMaxLeftTop.y && position.y > roomManager.cameraMaxRightBottom.y);
    }

    public void OpenRoom(int index)
    {
        roomManager.OpenRoom(index);
    }

    private void CheckForOpportunityAttack()
    {
        if(!currentCharacterTurn.CheckForAffliction(Affliction.Evasion) && !currentCharacterTurn.hasMoved)
        {
            List<Node> nodeToCheck = Grid.instance.GetNeighbours(currentCharacterTurn.currentNode);

            foreach (Node n in nodeToCheck)
            {
                if (n.hasCharacterOn && n.chara.GetTeam() != currentCharacterTurn.GetTeam())
                {
                    n.chara.AttackOfOpportunity(currentCharacterTurn.currentNode.worldPosition);
                }
            }
        }
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

    public void AskToMove(GameObject objectToMove, Vector3 destination, int maxDistance)
    {
        toMove = objectToMove;
        int distance = Pathfinding.instance.GetDistance(Grid.instance.NodeFromWorldPoint(objectToMove.transform.position), Grid.instance.NodeFromWorldPoint(destination));
        currentCharacterTurn.movedEvt.Invoke(distance);
        PathRequestManager.RequestPath(objectToMove.transform.position, destination, maxDistance, OnPathFound);
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            CheckForOpportunityAttack();
            path = newPath;
            targetIndex = 0;
            currentCharacterTurn.SetAnimation("Moving");
            //Grid.instance.ResetUsableNode();
            StopCoroutine(FollowPath());
            StartCoroutine(FollowPath());
        }
        else
        {
            Debug.Log(currentCharacterTurn + "No Path");
            if(currentCharacterTurn.GetTeam() != 1)
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
                    //toMove.transform.position = posTarget;

                    targetIndex++;
                    if (targetIndex >= path.Length)
                    {
                        Grid.instance.CreateGrid();
                        currentCharacterTurn.hasMoved = true;
                        EndCurrentAction(currentCharacterTurn);
                        yield break;
                    }
                    currentWaypoint = path[targetIndex];
                    lastPathPosition = currentWaypoint;

                    if (toMove == currentCharacterTurn.gameObject)
                    {
                        currentCharacterTurn.SetSpriteDirection((currentWaypoint.x < toMove.transform.position.x));
                    }
                }
                direction = new Vector2(currentWaypoint.x - toMove.transform.position.x, currentWaypoint.y - toMove.transform.position.y).normalized;

                toMove.transform.position += new Vector3(direction.x, direction.y, 0) * speed * Time.deltaTime;

              /*if(currentCharacterTurn.currentNode != Grid.instance.NodeFromWorldPoint(currentCharacterTurn.transform.position))
                {
                    CheckForOpportunityAttack();
                    currentCharacterTurn.currentNode = Grid.instance.NodeFromWorldPoint(currentCharacterTurn.transform.position);
                }*/
                //	Vector3.MoveTowards(transform.position,currentWaypoint,speed * Time.deltaTime);
                //testFollowPath = false;
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
