using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomAction_SpawnEnnemies : MonoBehaviour, IRoomAction
{
    [SerializeField] private List<PersonnageScriptables> characters;
    [SerializeField] private List<Transform> spawnPoints;

    public void PlayAction()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            Node n = Grid.instance.NodeFromWorldPoint(spawnPoints[i].position);
            int j = 0;
            while (j < 5 && !n.walkable)
            {
                n = Grid.instance.GetRandomNeighbours(n);
            }

            BattleManager.instance.SpawnNewCharacter(characters[i], n.worldPosition);
        }
    }
}
