using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct Clip
{
    public string name;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    //tmp
    public AudioClip[] testSounds;
    int index = 0;

    Dictionary<string, AudioClip> soundDictionary = new Dictionary<string, AudioClip>();

    public Clip[] clips;

    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        foreach (Clip clip in clips)
            soundDictionary.Add(clip.name, clip.clip);
    }

    public void PlaySound(string soundName)
    {
        if (soundName == "TestSound")
        {
            audioSource.PlayOneShot(testSounds[index]);

            index++;

            if (index >= testSounds.Count())
                index = 0;

            return;
        }
        audioSource.PlayOneShot(soundDictionary[soundName]);
    }

    static public SoundManager Get()
    {
        return GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }
}
