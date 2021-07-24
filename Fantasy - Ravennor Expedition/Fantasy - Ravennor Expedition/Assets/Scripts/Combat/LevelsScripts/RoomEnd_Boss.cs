using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEnd_Boss : RoomEnd
{
    [SerializeField] private int doorIndexForEnd;

    [SerializeField]
    private PersonnageScriptables bossWanted;

    private RuntimeBattleCharacter bossRuntime;

    public override void SetEnd()
    {
        roomManager.openRoomAct += SpawnBoss;
        roomManager.checkTurnAct += CheckEnd;
    }

    protected override void CheckEnd()
    {
        if (bossRuntime != null && bossRuntime.GetCurrentHps() <= 0)
        {
            roomManager.WinLevel();
        }
    }

    protected override void CheckEnd(int index)
    {
        
    }

    private void SpawnBoss(int index)
    {
        foreach (RuntimeBattleCharacter chara in BattleManager.instance.GetEnemyChara())
        {
            if (chara.name == bossWanted.nom)
            {
                bossRuntime = chara;
                break;
            }
        }
    }

    private void OnDestroy()
    {
        roomManager.openRoomAct -= SpawnBoss;
        roomManager.checkTurnAct -= CheckEnd;
    }
}
