using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundNames : byte
{
   None = 0, BananaSlip, BananaThrow, BananaCollect, MenuMusic,
    GunShot, BulletHit,
    Collect,
    CountdownCritical,
    Tagged
}

[Serializable]
public class SoundEffect
{
    public SoundNames name;
    public AudioClip[] audioClips;
    public FloatRange pitchRange = new FloatRange { min = 1, max = 1 };
}

[Serializable]
public class MusicTrack
{
    public SoundNames name;
    public AudioClip audioClip;
}

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    private void Awake()
    {
        instance = this;
    }

    [SerializeField] private SoundLibrary soundLibrary;
    private static List<AudioSource> audioSourcePool;
    private static SoundEffect[] sounds;
    private static int audioSourcePoolSize = 24;
    private static int audioSourcePoolSizeAddition = audioSourcePoolSize/2;
    private static Transform soundsParent;
    [SerializeField] private AudioSource musicSource;

    [SerializeField] private AudioMixer mixer;
    private const float SFX_NORMAL_VOLUME = 0;
    private const float MUSIC_NORMAL_VOLUME = 0;
    private const float MUTED_VOLUME = -80;

    private void Start()
    {

        soundsParent = new GameObject("Sounds").transform;
        soundsParent.SetParent(transform.parent);

        audioSourcePool = new List<AudioSource>(audioSourcePoolSize);
        PopulateAudioSourcePool(audioSourcePoolSize);

        sounds = soundLibrary.soundEffects;
        ConformToPlayerSettings();
    }

    private static void PopulateAudioSourcePool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            audioSourcePool.Add(Instantiate(instance.soundLibrary.oneShotSoundPreFab));
            audioSourcePool[i].gameObject.transform.SetParent(soundsParent);
        }
    }

    public static void PlayOneShotSound(SoundNames name, Vector3? position = null)
    {
        //TODO: Add Interest management so that we do not recruit audio sources unesesesrily
        if (StaticData.playerSettings.sfx == OnOffSwitch.On && name != SoundNames.None)
        {

            AudioClip clip = null;
            float minPitch = 0;
            float maxPitch = 0;

            for (int i = 0; i < sounds.Length; i++)
            {
                SoundEffect sound = sounds[i];
                if (sounds[i].name == name)
                {
                    int clipIndex = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        clipIndex = UnityEngine.Random.Range(0, sound.audioClips.Length);
                    }
                    clip = sound.audioClips[clipIndex];
                    minPitch = sound.pitchRange.min; maxPitch = sound.pitchRange.max;
                }
            }
            if (clip == null)
            {
                Debug.LogError("NO SOUND FOUND: " + name.ToString());
            }
            else
            {
                AudioSource audioSource = GetAnAvailableAudioSource();

                if(position == null)
                {
                    audioSource.spatialBlend = 0;
                }
                else
                {
                    audioSource.transform.position = (Vector3)position;
                    audioSource.spatialBlend = 1;

                }
                audioSource.clip = clip;
                audioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
                //oneShotSound.volume = clip.volume + volumeModification;
                audioSource.Play();
            }
        }
    }

    internal static void PlayOneShotSound(object countdownCritical, object p)
    {
        throw new NotImplementedException();
    }

    private static AudioSource GetAnAvailableAudioSource()
    {

        AudioSource source = null;

        int count = audioSourcePool.Count;
        for (int i = 0; i < count; i++)
        {
            if (!audioSourcePool[i].isPlaying)
            {
                source = audioSourcePool[i];
                break;
            }
        }
        if (source == null)
        {
            Debug.LogWarning("No avialable Sound Sources! Creating more.");
            PopulateAudioSourcePool(audioSourcePoolSizeAddition);
            source = audioSourcePool[count];
        }
        return source;
    }

    public static void PlayMusicTrack(SoundNames name)
    {
        AudioClip clip = null;
        MusicTrack[] musicTracks = instance.soundLibrary.musicTracks;
        
        for (int i = 0; i < musicTracks.Length; i++)
        {
            MusicTrack track = musicTracks[i];
            if (musicTracks[i].name == name)
            {
                clip = track.audioClip;
            }
        }
        if (clip == null)
        {
            Debug.LogError("NO SOUND FOUND: " + name.ToString());
        }
        else
        {
            AudioSource audioSource = instance.musicSource;
            audioSource.clip = clip;
            audioSource.Play();
        }
        
    }


    public static void ConformToPlayerSettings()
    {
        PlayerSettings playerSettings = StaticData.playerSettings;

        instance.mixer.SetFloat("sfxVolume", StaticData.playerSettings.sfx == OnOffSwitch.On ? SFX_NORMAL_VOLUME : MUTED_VOLUME);
        instance.mixer.SetFloat("musicVolume", StaticData.playerSettings.music == OnOffSwitch.On ? MUSIC_NORMAL_VOLUME : MUTED_VOLUME);

    }
}
