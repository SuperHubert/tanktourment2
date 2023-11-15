using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{ 
    public static SoundManager instance;
    [SerializeField] private AudioSource musicSource, EffectSource;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioClip music, validateEffect;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            musicSource.PlayOneShot(music);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        EffectSource.PlayOneShot(clip);
    }
    
    public void ChangeMusicVolume(float volume)
    { 
        audioMixer.SetFloat("MusiqueVolume", volume);
    }
    public void ChangeEffectVolume(float volume)
    { 
        PlaySound(validateEffect);
        audioMixer.SetFloat("SFXVolume", volume);
    }
    
}
