using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SoundDatas", menuName = "Create SoundData")]
public class SoundSystemDatas : ScriptableObject
{
    public float globalVolume = 1, musicVolume = 1, sfxVolume = 1, uiVolume = 1; 
}
