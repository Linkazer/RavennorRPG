using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_PlaySound : MonoBehaviour, IFeedbackSystem
{
    [SerializeField] private AudioSource source;
    [SerializeField] private RVN_AudioSound sound;
    public void Play()
    {
        SoundSyst.PlaySound(sound, source);
    }
}
