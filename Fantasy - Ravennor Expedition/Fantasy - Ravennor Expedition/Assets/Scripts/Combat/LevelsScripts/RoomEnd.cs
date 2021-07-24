using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RoomEnd : MonoBehaviour
{
    [SerializeField] protected RoomManager roomManager;

    public abstract void SetEnd();

    protected abstract void CheckEnd();
    protected abstract void CheckEnd(int index);
}
