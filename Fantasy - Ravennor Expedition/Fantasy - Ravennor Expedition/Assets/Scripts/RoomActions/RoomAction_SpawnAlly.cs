using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomAction_SpawnAlly : MonoBehaviour, IRoomAction
{
    [SerializeField] private PersonnageScriptables character;
    [SerializeField] private Vector2 position;
    public void PlayAction()
    {
        BattleManager.instance.SpawnNewAllyCharacter(character, position);
    }
}
