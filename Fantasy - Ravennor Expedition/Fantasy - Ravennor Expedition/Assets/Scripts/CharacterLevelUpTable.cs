using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelTable
{
    public bool autoLevelUp;
    public int levelUpStat;
    public int levelNeeded;
    public List<CharacterActionScriptable> possibleSpells = new List<CharacterActionScriptable>();
    public List<LevelUpCapacity> capacities = new List<LevelUpCapacity>();
}

[System.Serializable]
public class LevelUpCapacity
{
    public EffectType bonusType;
    public int bonusValue;
    public SpellEffectScriptables passif;
    public string description;
}

[CreateAssetMenu(fileName = "New Level up Table", menuName = "Create New LevelUp Table")]
public class CharacterLevelUpTable : ScriptableObject
{
    [SerializeField]
    private List<LevelTable> tables;

    public List<int> levelUnlockSpell = new List<int>(), levelUnlockCapacity = new List<int>(), autoLevelUps = new List<int>();

    public List<LevelTable> GetUsableTables(int levelWanted)
    {
        List<LevelTable> toReturn = new List<LevelTable>();
        for(int i = 0; i < levelWanted; i++)
        {
            if (!tables[i].autoLevelUp)
            {
                toReturn.Add(tables[i]);
            }
        }
        return toReturn;
    }
}
