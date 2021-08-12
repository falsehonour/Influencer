using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "SoundLibrary", order = 1)]
public class SoundLibrary : ScriptableObject
{
    public SoundEffect[] soundEffects;
    public MusicTrack[] musicTracks;

    public AudioSource oneShotSoundPreFab;
}
