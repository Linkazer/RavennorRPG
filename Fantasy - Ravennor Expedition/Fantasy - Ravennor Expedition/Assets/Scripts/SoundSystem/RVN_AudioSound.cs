using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "New Sound", menuName = "Sound/Create Sound Asset")]
public class RVN_AudioSound : ScriptableObject
{
    [SerializeField] private List<AudioClip> clips;
    [SerializeField] private AudioMixerGroup mixer;
    [SerializeField] private float loopTime;
    [SerializeField] private float loopInterval;

    public AudioClip GetClip => clips[Random.Range(0, clips.Count)];

    public AudioMixerGroup Mixer => mixer;

    public float LoopTime => loopTime;

    public float LoopInterval => loopInterval;
}
