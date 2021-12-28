using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;

/*public struct LaunchActionData
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
}*/

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

    //private LaunchActionData actData = default;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<RVN_AudioSound> diceClip;

    public static int TurnNumber => instance.currentTurn;

    public static List<RuntimeBattleCharacter> GetAllChara => new List<RuntimeBattleCharacter>(instance.roundList);

    public static List<RuntimeBattleCharacter> GetAllyTeamCharacters(int teamIndex)
    {
        if (teamIndex != 0)
        {
            return GetEnemyChara;
        }
        return GetPlayerChara;
    }

    public static List<RuntimeBattleCharacter> GetEnemyTeamCharacters(int teamIndex)
    {
        if (teamIndex != 0)
        {
            return GetPlayerChara;
        }
        return GetEnemyChara;
    }

    public static List<RuntimeBattleCharacter> GetPlayerChara => new List<RuntimeBattleCharacter>(instance.charaTeamOne);

    public static List<RuntimeBattleCharacter> GetEnemyChara => new List<RuntimeBattleCharacter>(instance.charaTeamTwo);

    public static RuntimeBattleCharacter GetCurrentTurnChara => instance.currentCharacterTurn;


    #region Set Up
    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        GameDices.SetRandomInit(); // Met à jour le Random pour en avoir un meilleur

        // Mise en place du niveau
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

        // Mise en place des personnages joueurs
        for (int i = 0; i < playerTeam.Count; i++)
        {
            if (i < roomManager.playerStartPositions.Count)
            {
                SetCharacter(playerTeam[i], roomManager.playerStartPositions[i]);
            }
        }

        // Ouverture de la première salle
        roomManager.OpenRoom(0);

        // Reset des positions des RuntimaCharacter pas encore utilisé
        foreach (RuntimeBattleCharacter r in usableRuntimeCharacter)
        {
            r.transform.position = new Vector2(-10, -10);
        }

        // Rangement pas initiative
        SortInitiativeList(initiatives, roundList, 0, initiatives.Count - 1);

        LoadingScreenManager.instance.HideScreen();

        // Lancement du combat
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

        // Mise en place du personnage permettant de lancer des sorts sans caster
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

    private void SetCharacter(PersonnageScriptables newPerso, Vector2 position)
    {
        // Choix de l'équipe du personnage
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

        // Utilisation du RuntimeCharacter
        usableRuntimeCharacter[0].UseRuntimeCharacter(newPerso, team, position);
        roundList.Add(usableRuntimeCharacter[0]);
        usableRuntimeCharacter.RemoveAt(0);

        // Application des passifs
        foreach(SpellEffectScriptables eff in roundList[roundList.Count - 1].GetCharacterDatas().passifs)
        {
            ApplyEffects(eff, 0, roundList[roundList.Count - 1], roundList[roundList.Count - 1]);
        }

        // Ajout de l'initiative
        initiatives.Add(roundList[roundList.Count - 1].GetInitiative());
    }

    public void KillCharacter(RuntimeBattleCharacter toKill)
    {
        toKill.ResolveEffect(EffectTrigger.Die);

        if (toKill.GetCurrentHps() <= 0)
        {
            toKill.deathEvt?.Invoke();
            diary.AddText(toKill.name + " succombe.");

            toKill.SetAnimation("DeathAnim");
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
        }
    }

    public void LoadBattle()
    {
        RavenorGameManager.instance.LoadBattle();
    }

    public void SetWinPanel()
    {
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

        // Gestion du score de dangerosité
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

        // Mise à jour de l'UI
        BattleUiManager.instance.SetNewTurn(currentIndexTurn, roundList);

        // Mise à jour du RuntimeCharacter
        character.NewTurn();

        // Début du tour en fonction de l'équipe et de l'état du personnage
        if (character.GetCurrentHps() > 0 && !character.CheckForAffliction(Affliction.Paralysie))
        {
            if (character.GetTeam() == 0)
            {
                PlayerBattleManager.instance.NewPlayerTurn(character);
            }
            else
            {
                PlayerBattleManager.instance.ActivatePlayerBattleController(false);

                AiBattleManager.instance.BeginNewTurn(character);
            }

            Grid.instance.CreateGrid();
        }
        else
        {
            currentIndexTurn = (currentIndexTurn + 1) % roundList.Count;

            NewCharacterRound(roundList[currentIndexTurn]);
        }
    }

    public void EndTurn()
    {
        PlayerBattleManager.instance.ActivatePlayerBattleController(false);

        // Mise à jour du nombre de tour complat effectué
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

        // Vérifiction de la fin d'une partie
        if (!CheckForBattleEnd())
        {
            // Lancement du prochain tour
            currentCharacterTurn.EndTurn();
            currentIndexTurn = (currentIndexTurn + 1) % roundList.Count;

            NewCharacterRound(roundList[currentIndexTurn]);
        }
    }
    #endregion

    #region Utilities
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
            else if (array[j] == pivot && persoList[j].GetMaxHp < hpPivot)
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

        RuntimeBattleCharacter tempChara1 = persoList[lowIndex + 1];
        persoList[lowIndex + 1] = persoList[high];
        persoList[high] = tempChara1;

        return lowIndex + 1;
    }

    #endregion
}
