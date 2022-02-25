using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RoomDetection : MonoBehaviour
{
    [SerializeField] private int minimumEnnemies = 3;
    [SerializeField] private UnityEvent ToPlay;

    private void Start()
    {
        BattleManager.newTurnBegin += CheckOnTurn;
    }

    private void CheckOnTurn()
    {
        if(BattleManager.instance.GetEnemyChara(true).Count <= minimumEnnemies)
        {
            ToPlay?.Invoke();
        }
    }

    private void OnDestroy()
    {
        BattleManager.newTurnBegin -= CheckOnTurn;
    }
}
