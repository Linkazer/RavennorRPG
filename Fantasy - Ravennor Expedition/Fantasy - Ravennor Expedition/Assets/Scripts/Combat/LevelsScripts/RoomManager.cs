using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    //public static RoomManager instance;

    public Vector2 cameraPos;
    public ParcheminScriptable startDialogue, endDialogue, campDialogue;
    public List<string> characterInLevel = new List<string>(), characterInCamp = new List<string>();
    public bool endGame;

    public GameObject nextLvl;
    public int levelAtEnd;

    [SerializeField]
    protected List<Room> rooms;

    [SerializeField]
    protected List<Transform> playerPos;
    //[HideInInspector]
    public List<Vector2> playerStartPositions;

    [SerializeField]
    protected List<int> usedIndex = new List<int>();

    [SerializeField]
    private int numberOfEnnemiesToKill = 0;

    [SerializeField]
    public Vector2 cameraMaxLeftTop, cameraMaxRightBottom;

    public RoomManager()
    {
        characterInLevel.Add("Eliza");
        characterInLevel.Add("Nor");
        characterInLevel.Add("Okun");
        characterInLevel.Add("Shedun");
        characterInLevel.Add("Vanyaenn");
        characterInLevel.Add("Mira");

        characterInCamp.Add("Eliza");
        characterInCamp.Add("Nor");
        characterInCamp.Add("Okun");
        characterInCamp.Add("Shedun");
        characterInCamp.Add("Vanyaenn");
        characterInCamp.Add("Mira");
    }

    private void Start()
    {
        usedIndex = new List<int>();
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

    public virtual void SetRoomManager()
    {
        
    }

    /*public void SetBossChara(List<RuntimeBattleCharacter> characters)
    {
        if (bossChara != null)
        {
            foreach (RuntimeBattleCharacter chara in characters)
            {
                if (chara.GetCharacterDatas().nom == bossToKill.nom)
                {
                    bossChara = chara;
                    break;
                }
            }
        }
    }*/

    public void OpenRoom(int index)
    {
        if(!usedIndex.Contains(index) || index == 0)
        {
            usedIndex.Add(index);
            ActivateRoom(index);
        }
    }

    protected virtual void ActivateRoom(int index)
    {
        Room toActivate = rooms[index];
        for (int i = 0; i < toActivate.ennemis.Count;i++)
        {
            BattleManager.instance.SpawnNewCharacter(toActivate.ennemis[i], toActivate.ennemisPositions[i]);
        }
    }

    public virtual bool CheckForEnd()
    {
        int killedChara = 0;
        foreach(RuntimeBattleCharacter chara in BattleManager.instance.GetTeamTwo())
        {
            if(chara.GetCurrentHps()<=0)
            {
                killedChara++;
            }
        }
        if (killedChara >= numberOfEnnemiesToKill)
        {
            WinLevel();
            return true;
        }

        return false;
    }

    public void WinLevel()
    {
        RavenorGameManager.instance.dialogueToDisplay = campDialogue;

        BattleManager.instance.EndBattle(true);
    }

}
