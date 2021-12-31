using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_PlayGlobalSound : MonoBehaviour, IFeedbackSystem
{
    [SerializeField] private RVN_AudioSound sound;

    public void Play()
    {
        SoundSyst.PlayGlobalSound(sound);
    }

    public void Play(RVN_AudioSound toPlay)
    {
        SoundSyst.PlayGlobalSound(toPlay);
    }
}
