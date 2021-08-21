using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private int levelId;

    [Header("Camera")]
    [SerializeField]
    private Vector2 cameraPos;
    [SerializeField]
    public Vector2 cameraMaxLeftTop, cameraMaxRightBottom;
    private static Vector2 sCameraPos;
    [Header("Dialogues")]
    public ParcheminScriptable startDialogue;
    public ParcheminScriptable endDialogue;
    public ParcheminScriptable campDialogue;
    [Header("Global Informations")]
    public List<PersonnageScriptables> characterInLevel = new List<PersonnageScriptables>();
    public bool endGame;
    public GameObject nextLvl;

    [Header("Camps")]
    public Sprite backgroundCamp;
    public AudioClip backgroundMusic;
    public Sprite fireSprite;
    public List<CampDisplayableCharacter> characterInCamp = new List<CampDisplayableCharacter>();

    [Header("Rooms Informations")]
    [SerializeField]
    protected List<Room> rooms;

    [Header("Player Informations")]
    [SerializeField]
    protected List<Transform> playerPos;
    [HideInInspector]
    public List<Vector2> playerStartPositions;

    [SerializeField]
    protected List<int> openRoomIndexes = new List<int>();

    public Action<int> openRoomAct;
    public Action checkTurnAct;

    [SerializeField] private RoomEnd end;

    private bool hasEnd;

    public RoomManager()
    {
        
    }

    private void Awake()
    {
        sCameraPos = cameraPos;
    }

    private void Start()
    {
        openRoomIndexes = new List<int>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            BattleManager.instance.EndBattle(false);
        }
        if(Input.GetKeyDown(KeyCode.M))
        {
            BattleManager.instance.EndBattle(true);
        }
    }

    [ContextMenu("Set Chara positions")]
    protected void SetPositions()
    {
        playerStartPositions = new List<Vector2>();
        foreach(Transform tr in playerPos)
        {
            playerStartPositions.Add(tr.position);
        }

        foreach(Room r in rooms)
        {
            r.ennemisPositions = new List<Vector2>();
            foreach (Transform tr in r.ennemisPos)
            {
                r.ennemisPositions.Add(tr.position);
            }
        }
    }

    public void SetRoomManager()
    {
        end.SetEnd();
    }

    public void OpenRoom(int index)
    {
        Debug.Log("Open room : " + index);
        if (index == 0)
        {
            openRoomIndexes = new List<int>();
        }

        if (!openRoomIndexes.Contains(index))
        {
            openRoomIndexes.Add(index);
            ActivateRoom(index);
        }
    }

    public void ActivateRoom(int index)
    {
        openRoomAct?.Invoke(index);
        if (!hasEnd)
        {
            Room toActivate = rooms[index];

            for (int i = 0; i < toActivate.toEnable.Count; i++)
            {
                toActivate.toEnable[i].SetActive(true);
            }

            for (int i = 0; i < toActivate.toDisable.Count; i++)
            {
                toActivate.toDisable[i].SetActive(false);
                Debug.Log(toActivate.toDisable[i].activeSelf);
            }

            for (int i = 0; i < toActivate.ennemis.Count; i++)
            {
                BattleManager.instance.SpawnNewCharacter(toActivate.ennemis[i], toActivate.ennemisPositions[i]);
            }

        }
    }

    public virtual bool CheckEndTurn()
    {
        checkTurnAct?.Invoke();
        return hasEnd;
    }

    public void WinLevel()
    {
        hasEnd = true;
        if (campDialogue != null)
        {
            RavenorGameManager.instance.dialogueToDisplay = campDialogue;
        }

        RavenorGameManager.SetUnlockedLevel(levelId);
        BattleManager.instance.EndBattle(true);
    }

    public static Vector2 GetCamStartPosition()
    {
        return sCameraPos;
    }
}
