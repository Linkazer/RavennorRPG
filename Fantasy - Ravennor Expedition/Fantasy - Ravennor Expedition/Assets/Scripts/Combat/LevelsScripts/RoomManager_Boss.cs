using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager_Boss : RoomManager
{
    [SerializeField]
    private PersonnageScriptables bossWanted;

    private RuntimeBattleCharacter bossRuntime;

    // Start is called before the first frame update
    public override void SetRoomManager()
    {
        
    }

    public override bool CheckForEnd()
    {
        if(bossRuntime != null && bossRuntime.GetCurrentHps()<=0)
        {
            WinLevel();
            return true;
        }
        return false;
    }

    protected override void ActivateRoom(Room toActivate)
    {
        base.ActivateRoom(toActivate);

        foreach (RuntimeBattleCharacter chara in BattleManager.instance.GetTeamTwo())
        {
            if (chara.name == bossWanted.nom)
            {
                bossRuntime = chara;
                break;
            }
        }
    }
}
