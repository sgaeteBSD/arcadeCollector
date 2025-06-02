using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;
    [SerializeField] private AudioSource sfxObject;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip bgm;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        PlayMusic(bgm, 0.5f);
    }

    public AudioSource PlaySFXClip(AudioClip clip, Transform spawnTransform, float volume)
    {
        AudioSource source = Instantiate(sfxObject, spawnTransform.position, Quaternion.identity);

        source.clip = clip;

        source.volume = volume;

        source.Play();

        float clipLen = source.clip.length;

        Destroy(source.gameObject, clipLen);

        return source;
    }

    public void PlayMusic(AudioClip musicClip, float volume = 1f, bool loop = true)
    {
        if (musicSource == null)
        {
            GameObject musicObject = new GameObject("Music Source");
            musicSource = musicObject.AddComponent<AudioSource>();
            musicSource.loop = loop;
            DontDestroyOnLoad(musicObject);
        }

        musicSource.clip = musicClip;
        musicSource.volume = volume;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
}
