using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private Slider globalSlider, musicSlider, sfxSlider;

    [SerializeField]
    private GameObject controleFiche;

    public void OpenMenu()
    {
        globalSlider.value = SoundSyst.instance.GetVolumeValue("GlobalVolume");
        musicSlider.value = SoundSyst.instance.GetVolumeValue("MusicVolume");
        sfxSlider.value = SoundSyst.instance.GetVolumeValue("SFXVolume");
        gameObject.SetActive(true);
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }

    public void ChangeGlobalSound(Slider wantedSlider)
    {
        SoundSyst.instance.ChangeMixerVolume("GlobalVolume", wantedSlider.value);
    }

    public void ChangeMusicSound(Slider wantedSlider)
    {
        SoundSyst.instance.ChangeMixerVolume("MusicVolume", wantedSlider.value);
    }

    public void ChangeSFXSound(Slider wantedSlider)
    {
        SoundSyst.instance.ChangeMixerVolume("SFXVolume", wantedSlider.value);
    }

    public void GoToMainMenu()
    {
        RavenorGameManager.instance.LoadMainMenu();
    }

    public void CloseGame()
    {
        RavenorGameManager.instance.CloseGame();
    }

    public void OpenControl()
    {
        controleFiche.SetActive(true);
    }

    public void CloseControl()
    {
        controleFiche.SetActive(false);
    }
}
