using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEnd_EnnemyDown : RoomEnd
{
    [SerializeField] private int ennemiesToDown;

    public override void SetEnd()
    {
        roomManager.checkTurnAct += CheckEnd;
    }

    protected override void CheckEnd()
    {
        int killedChara = 0;
        foreach (RuntimeBattleCharacter chara in BattleManager.instance.GetEnemyChara())
        {
            if (chara.GetCurrentHps() <= 0)
            {
                killedChara++;
            }
        }
        if (killedChara >= ennemiesToDown)
        {
            roomManager.WinLevel();
        }
    }

    protected override void CheckEnd(int index)
    {
        
    }

    private void OnDestroy()
    {
        roomManager.checkTurnAct -= CheckEnd;
    }

}
