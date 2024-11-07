using System.Collections.Generic;
using UnityEngine;

public class GameAudioHandler : MonoBehaviour
{
    private AudioSource _musicSource; // For background music
    private List<AudioSource> _sfxSources; // For sound effects
    private static Dictionary<string, AudioClip> _audioCache = new Dictionary<string, AudioClip>();
    private const int MaxSfxSources = 5; // Limit of simultaneous sound effects

    private void Awake()
    {
        // Initialize the music AudioSource
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop = true; // Music is typically looped

        // Initialize a pool of AudioSources for sound effects
        _sfxSources = new List<AudioSource>();
        for (int i = 0; i < MaxSfxSources; i++)
        {
            var sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            _sfxSources.Add(sfxSource);
        }
    }

    private void OnEnable()
    {
        GameEventHandler.Instance.Subscribe<PlayAudioEvent>(OnPlayAudio);
        GameEventHandler.Instance.Subscribe<StopAudioEvent>(OnStopAudio);
    }

    private void OnDisable()
    {
        GameEventHandler.Instance.Unsubscribe<PlayAudioEvent>(OnPlayAudio);
        GameEventHandler.Instance.Unsubscribe<StopAudioEvent>(OnStopAudio);
    }

    private void OnPlayAudio(PlayAudioEvent evt)
    {
        // Try to get the clip from cache or load it if needed
        if (!_audioCache.TryGetValue(evt.AudioClipPath, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>(evt.AudioClipPath);
            if (clip != null)
            {
                _audioCache[evt.AudioClipPath] = clip;
            }
            else
            {
                Debug.LogWarning("Audio clip not found at path: " + evt.AudioClipPath);
                return;
            }
        }

        // Determine if it's background music or sound effect
        if (evt.IsMusic)
        {
            PlayMusic(clip, evt.Loop);
        }
        else
        {
            PlaySoundEffect(clip, evt.Loop);
        }
    }

    private void OnStopAudio(StopAudioEvent evt)
    {
        // Check if the audio clip is cached
        if (!_audioCache.TryGetValue(evt.AudioClipPath, out AudioClip clip))
        {
            Debug.LogWarning("Audio clip not found in cache: " + evt.AudioClipPath);
            return;
        }

        // Stop music if it's the clip currently playing
        if (_musicSource.isPlaying && _musicSource.clip == clip)
        {
            _musicSource.Stop();
            return;
        }

        // Stop sound effect if it's playing in any of the SFX sources
        foreach (var sfxSource in _sfxSources)
        {
            if (sfxSource.isPlaying && sfxSource.clip == clip)
            {
                sfxSource.Stop();
                // return; // Stops the first matching clip; remove this if multiple instances need stopping
            }
        }

        Debug.Log("Audio clip is not currently playing: " + evt.AudioClipPath);
    }

    private void PlayMusic(AudioClip clip, bool loop)
    {
        if (_musicSource.isPlaying && _musicSource.clip == clip) return; // Avoid restarting same music

        _musicSource.clip = clip;
        _musicSource.loop = loop;
        _musicSource.Play();
    }

    private void PlaySoundEffect(AudioClip clip, bool loop)
    {
        // Find an available AudioSource or stop the first one if all are playing
        var sfxSource = _sfxSources.Find(source => !source.isPlaying) ?? _sfxSources[0];
        sfxSource.clip = clip;
        sfxSource.loop = loop;
        sfxSource.Play();
    }

    public static void ClearCache()
    {
        _audioCache.Clear(); // Optional: Call this to clear the audio cache when needed
    }
}