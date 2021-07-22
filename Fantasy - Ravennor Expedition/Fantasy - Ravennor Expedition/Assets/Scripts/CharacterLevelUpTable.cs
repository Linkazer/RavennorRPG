using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelTable
{
    [System.Serializable]
    public class StatLevelUp
    {
        public CharacterStats stat;
        public int value;
    }

    public List<StatLevelUp> stats;
    public List<CharacterActionScriptable> possibleSpells = new List<CharacterActionScriptable>();
    public List<LevelUpCapacity> capacities = new List<LevelUpCapacity>();
}

[System.Serializable]
public class LevelUpCapacity
{
    public string nom;
    public string description;
    public SpellEffectScriptables passif;
}

[CreateAssetMenu(fileName = "New Level up Table", menuName = "Character/Table Niveaux")]
public class CharacterLevelUpTable : ScriptableObject
{
    [SerializeField]
    private List<LevelTable> tables;

    public LevelTable GetLevelTable(int levelWanted)
    {
        return tables[levelWanted-1];
    }
}
