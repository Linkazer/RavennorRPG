using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Dice
{
    public DiceType wantedDice;
    public int numberOfDice;
    public DamageType wantedDamage;

    public Dice()
    {

    }

    public Dice(DiceType wantedType, int number, DamageType damage)
    {
        wantedDice = wantedType;
        numberOfDice = number;
        wantedDamage = damage;
    }
}
