using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager_DoorExit : RoomManager
{
    [SerializeField]
    private int doorIndexForEnd;

    // Start is called before the first frame update
    public override void SetRoomManager()
    {

    }

    public override bool CheckForEnd()
    {
        return false;
    }

    protected override void ActivateRoom(int index)
    {
        if (index == doorIndexForEnd)
        {
            WinLevel();
        }
        else
        {
            base.ActivateRoom(index);
        }
    }
}
