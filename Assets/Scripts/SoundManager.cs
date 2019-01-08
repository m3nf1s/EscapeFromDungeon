using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource music;
    public AudioSource sound;
    public static SoundManager instance = null;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);


        DontDestroyOnLoad(gameObject);
    }

    public void PlaySound(AudioClip clip)
    {
        sound.clip = clip;
        sound.Play();
    }
}
