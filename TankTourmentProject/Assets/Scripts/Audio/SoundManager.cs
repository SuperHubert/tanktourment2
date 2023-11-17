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
    [SerializeField] private AudioClip music;
    public AudioClip validateEffect, cancelEffect, explosion, shoot, click;
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

    public static void PlaySound(AudioClip clip)
    {
        if(instance == null) return;
        instance.EffectSource.PlayOneShot(clip);
    }
    
    public static void ChangeMusicVolume(float volume)
    { 
        if(instance == null) return;
        instance.audioMixer.SetFloat("MusiqueVolume", volume);
    }
    public static void ChangeEffectVolume(float volume)
    { 
        if(instance == null) return;
        PlaySound(instance.validateEffect);
        instance.audioMixer.SetFloat("SFXVolume", volume);
    }
    
}
