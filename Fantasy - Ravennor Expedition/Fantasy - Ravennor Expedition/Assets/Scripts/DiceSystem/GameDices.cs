using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DiceType { D4, D6, D8, D10, D12, D20}

public class GameDices
{
    public static int RollD4()
    {
        return Random.Range(1, 5);
    }

    public static int RollD6()
    {
        return Random.Range(1, 7);
    }

    public static int RollD8()
    {
        return Random.Range(1, 9);
    }

    public static int RollD10()
    {
        return Random.Range(1, 11);
    }

    public static int RollD12()
    {
        return Random.Range(1, 13);
    }

    public static int RollD20()
    {
        return Random.Range(1, 21);
    }

    public static int RollDice(int diceNumber, DiceType dice)
    {
        int result = 0;
        switch(dice)
        {
            case DiceType.D4:
                for(int i = 0; i < diceNumber; i++)
                {
                    result += RollD4();
                }
                break;
            case DiceType.D6:
                for (int i = 0; i < diceNumber; i++)
                {
                    result += RollD6();
                }
                break;
            case DiceType.D8:
                for (int i = 0; i < diceNumber; i++)
                {
                    result += RollD8();
                }
                break;
            case DiceType.D10:
                for (int i = 0; i < diceNumber; i++)
                {
                    result += RollD10();
                }
                break;
            case DiceType.D12:
                for (int i = 0; i < diceNumber; i++)
                {
                    result += RollD12();
                }
                break;
            case DiceType.D20:
                for (int i = 0; i < diceNumber; i++)
                {
                    result += RollD20();
                }
                break;
        }
        return result;
    }
}
