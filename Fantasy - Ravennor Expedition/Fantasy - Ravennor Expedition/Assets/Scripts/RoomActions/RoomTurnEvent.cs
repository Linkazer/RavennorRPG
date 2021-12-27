using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class RoomTurnEvent
{
    public List<UnityEvent> toPlayOnTurn;
    [Tooltip("If less than 0, will be played every turn.")] public int turnIndex;

    public void PlayEvents()
    {
        for(int i = 0; i < toPlayOnTurn.Count; i++)
        {
            toPlayOnTurn[i]?.Invoke();
        }
    }
}
