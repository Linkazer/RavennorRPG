using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEnd_ReachTurn : RoomEnd
{
    [SerializeField] private int roundToReach;

    public override void SetEnd()
    {
        roomManager.checkTurnAct += CheckEnd;
    }

    protected override void CheckEnd()
    {
        if (BattleManager.TurnNumber >= roundToReach)
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

