using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomAction_SpawnAlly : MonoBehaviour, IRoomAction
{
    [SerializeField] private List<PersonnageScriptables> characters;
    [SerializeField] private List<Transform> spawnPoints;
    public void PlayAction()
    {
        for(int i = 0; i < characters.Count; i++)
        {
            BattleManager.instance.SpawnNewAllyCharacter(characters[i], spawnPoints[i].position);
        }

    }
}
