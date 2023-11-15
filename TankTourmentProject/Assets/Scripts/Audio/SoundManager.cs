using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{ 
    public static SoundManager instance;
    [SerializeField] private AudioSource musicSource, EffectSource;
    [SerializeField] private AudioClip music;
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

    public void PlaySond(AudioClip clip)
    {
        EffectSource.PlayOneShot(clip);
    }
}
