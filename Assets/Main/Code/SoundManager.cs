using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundNames : byte
{
    None = 0, BananaSlip=1, BananaThrow=2, BananaCollect=3, MenuMusic=4,
    GunShot=5, BulletHit=6, Collect=7, CountdownCritical=8, Tagged=9, FootballCollect=10, SprintCollect=11,
    HealthCollect =12, GunCollect=13,
    FootballKicked = 14, FootballHit = 22, CountdownTimeIsUp =17,
    PushSwoosh = 19, PushHit =20, LobbyMusic = 23, GameMusic = 24, SlowedDown = 25,
    AudiencePositiveSurprise =26, AudienceNeutralSurprise = 18, AudienceNegativeSurprise = 27,
    AudienceMinorApplause = 15, AudienceMinorDisappointment = 21, AudienceLaugh = 16, 
}

[Serializable]
public class SoundEffect
{
    public SoundNames name;
    public CorrectedAudioClip[] correctedAudioClips;
    public FloatRange pitchRange = new FloatRange { min = 1, max = 1 };
}

[Serializable]
public class MusicTrack
{
    public SoundNames name;
    public AudioClip audioClip;
}

[Serializable]
public class CorrectedAudioClip
{
    public AudioClip clip;
    [Range(0,1)] public float volume = 1;
}

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    private void Awake()
    {
        if(instance != null)
        {
            Debug.LogError("an instance of SoundManager already exists!");
        }
        else if (Mirror.NetworkServer.active)
        {
            Destroy(this);
        }
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
    private static float sfxNormalVolume;
    private static float musicNormalVolume;
    private const float MUTED_VOLUME = -80;

    private void Start()
    {

        mixer.GetFloat("sfxVolume",out sfxNormalVolume);
        mixer.GetFloat("musicVolume", out musicNormalVolume);

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

            CorrectedAudioClip correctedAudioClip = null;
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
                        clipIndex = UnityEngine.Random.Range(0, sound.correctedAudioClips.Length);
                    }
                    correctedAudioClip = sound.correctedAudioClips[clipIndex];
                    minPitch = sound.pitchRange.min; maxPitch = sound.pitchRange.max;
                }
            }
            if (correctedAudioClip == null)
            {
                Debug.LogError("NO SOUND FOUND: " + name.ToString());
            }
            else
            {
                AudioSource audioSource = GetAnAvailableAudioSource();

                if(position == null)
                {
                    //TODO: Decide whether we want panning or not
                    audioSource.spatialBlend = 0;
                }
                else
                {
                    audioSource.transform.position = (Vector3)position;
                    audioSource.spatialBlend = 1;

                }
                audioSource.clip = correctedAudioClip.clip;
                audioSource.volume = correctedAudioClip.volume;

                audioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
                //oneShotSound.volume = clip.volume + volumeModification;
                audioSource.Play();
            }
        }
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
            audioSource.volume = 1;
        }
        
    }

    public static void ConformToPlayerSettings()
    {
        PlayerSettings playerSettings = StaticData.playerSettings;

        instance.mixer.SetFloat("sfxVolume", StaticData.playerSettings.sfx == OnOffSwitch.On ? sfxNormalVolume : MUTED_VOLUME);
        instance.mixer.SetFloat("musicVolume", StaticData.playerSettings.music == OnOffSwitch.On ? musicNormalVolume : MUTED_VOLUME);

    }

    public static void FadeOutMusic()
    {
        instance.StartCoroutine(instance.FadeOutMusicCoroutine());
    }

    private IEnumerator FadeOutMusicCoroutine()
    {
        //HARDCODING ahead

        float fadeDuration = 2f;
        float fadeInterval = 0.1f;
        WaitForSeconds waitForSeconds = new WaitForSeconds(fadeInterval);
        float fadeStep = fadeInterval / fadeDuration;
        while (musicSource.volume > 0)
        {
            musicSource.volume -= fadeStep;
            yield return  waitForSeconds;
        }
        musicSource.Stop();
    }
}
