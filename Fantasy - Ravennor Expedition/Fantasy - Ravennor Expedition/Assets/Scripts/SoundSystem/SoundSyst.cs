using System;
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

    [SerializeField] private AudioSource mainMusic;
    [SerializeField] private AnimationCurve soundSlider;

    private float maxSound = 0f;
    private float minSound = -80f;

    private List<float> sliderProgresses = new List<float>();

    private float SoundRange => maxSound - minSound;

    private void Awake()
    {
        if (instance != null)
        {
            mainMusic.Stop();
            Destroy(mainMusic);
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        for (int i = 0; i < 3; i++)
        {
            sliderProgresses.Add(0.7f);
        }
    }

    private float GetCurveValue(float sliderProgress)
    {
        return soundSlider.Evaluate(sliderProgress);
    }

    private float GetSoundLevel(float sliderProgress)
    {
        return (SoundRange * GetCurveValue(sliderProgress)) + minSound;
    }

    public float GetVolumeValue(string name)
    {
        switch (name)
        {
            case "GlobalVolume":
                return sliderProgresses[0];
            case "MusicVolume":
                return sliderProgresses[1];
            case "SFXVolume":
                return sliderProgresses[2];
        }
        return 0;
    }

    public void ChangeMixerVolume(string varName, float value)
    {
        switch (varName)
        {
            case "GlobalVolume":
                sliderProgresses[0] = value;
                break;
            case "MusicVolume":
                sliderProgresses[1] = value;
                break;
            case "SFXVolume":
                sliderProgresses[2] = value;
                break;
        }
        mixer.SetFloat(varName, GetSoundLevel(value));
    }

    public static void ChangeMainMusic(AudioClip toPlay)
    {
        if(toPlay == null)
        {
            instance.mainMusic.Stop();
        }
        else if (toPlay != instance.mainMusic.clip)
        {
            instance.mainMusic.clip = toPlay;
            instance.mainMusic.Play();
        }
    }

    public static void PlaySound(RVN_AudioSound sound, AudioSource source)
    {
        AudioClip toPlay = sound.GetClip;

        source.outputAudioMixerGroup = sound.Mixer;
        source.PlayOneShot(toPlay);

        Debug.Log(sound.Mixer);

        if(sound.LoopInterval > 0)
        {
            TimerSyst.CreateTimer(sound.LoopInterval, () => PlaySound(sound, source));
        }
    }
}
