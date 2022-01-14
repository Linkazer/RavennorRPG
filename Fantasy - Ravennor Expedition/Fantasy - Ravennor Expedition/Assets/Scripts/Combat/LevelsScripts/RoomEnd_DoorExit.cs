using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEnd_DoorExit : RoomEnd
{
    [SerializeField] private int doorIndexForEnd;

    public override void SetEnd()
    {
        roomManager.beforeOpenRoomAct += CheckEnd;
    }

    protected override void CheckEnd()
    {

    }

    protected override void CheckEnd(int index)
    {
        if (index == doorIndexForEnd)
        {
            roomManager.WinLevel();
        }
    }

    private void OnDestroy()
    {
        roomManager.beforeOpenRoomAct -= CheckEnd;
    }
}
