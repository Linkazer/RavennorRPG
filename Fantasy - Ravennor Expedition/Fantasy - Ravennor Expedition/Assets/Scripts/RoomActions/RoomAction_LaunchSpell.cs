using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomAction_LaunchSpell : MonoBehaviour, IRoomAction
{
    [SerializeField] private CharacterActionScriptable spell;
    [SerializeField] private List<Transform> possiblePositions;
    [SerializeField, Tooltip("If false, will choose a random position in the list")] private bool useAllPosition;

    public void PlayAction()
    {
        BattleManager.instance.LaunchActionWithoutCaster(spell, possiblePositions[Random.Range(0, possiblePositions.Count)].position, false);
    }
}
