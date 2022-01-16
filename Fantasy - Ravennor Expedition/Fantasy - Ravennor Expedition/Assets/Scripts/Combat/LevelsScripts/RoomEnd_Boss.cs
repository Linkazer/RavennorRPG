using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEnd_Boss : RoomEnd
{
    [SerializeField]
    private PersonnageScriptables bossWanted;

    [SerializeField] private RuntimeBattleCharacter bossRuntime;

    public override void SetEnd()
    {
        roomManager.afterOpenRoomAct += SpawnBoss;
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
        Debug.Log(BattleManager.instance.GetEnemyChara(true).Count);
        foreach (RuntimeBattleCharacter chara in BattleManager.instance.GetEnemyChara(true))
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
        roomManager.afterOpenRoomAct -= SpawnBoss;
        roomManager.checkTurnAct -= CheckEnd;
    }
}
