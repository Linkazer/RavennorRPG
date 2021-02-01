using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundSyst : MonoBehaviour
{
    [SerializeField]
    private SoundSystemDatas datas;

    public static SoundSyst instance;


    [SerializeField]
    private AudioMixer mixer;

    private void Awake()
    {
        instance = this;
    }

    public float GetVolumeValue(string name)
    {
        float toReturn = 0;
        mixer.GetFloat(name, out toReturn);
        return (toReturn+40)/40;
    }

    public void ChangeMixerVolume(string varName, float value)
    {
        mixer.SetFloat(varName, value*40-40);
    }
}
