using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEnd_MultipleInteracion : RoomEnd
{
    [SerializeField] private int wantedInteractionCount;

    private int currentInteractionDone;

    public void AddInteraction()
    {
        currentInteractionDone++;

        CheckEnd();
    }

    public override void SetEnd()
    {
        
    }

    protected override void CheckEnd()
    {
        if (currentInteractionDone >= wantedInteractionCount)
        {
            roomManager.WinLevel();
        }
    }

    protected override void CheckEnd(int index)
    {
        
    }
}
