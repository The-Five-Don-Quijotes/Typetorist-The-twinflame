using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static UnityEngine.InputManagerEntry;
using static UnityEngine.Rendering.DebugUI.Table;

public class OptionUI : MonoBehaviour
{
    [Header("UI Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    // This function is called when the GameObject becomes active.
    // Perfect for setting up the sliders every time the options menu is opened.
    private void OnEnable()
    {
        // Make sure the AudioManager exists before we try to use it.
        if (AudioManager.instance != null)
        {
            // Set the sliders' initial values to match the current volume.
            musicSlider.value = AudioManager.instance.GetMusicVolume();
            sfxSlider.value = AudioManager.instance.GetSFXVolume();

            // Add listeners. These will call our functions whenever a slider's value changes.
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    // This function will be called by the music slider.
    public void SetMusicVolume(float volume)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetMusicVolume(volume);
        }
    }

    // This function will be called by the SFX slider.
    public void SetSFXVolume(float volume)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetSFXVolume(volume);
        }
    }

    // It's good practice to remove listeners when the object is disabled.
    private void OnDisable()
    {
        musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
        sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);
    }
}


