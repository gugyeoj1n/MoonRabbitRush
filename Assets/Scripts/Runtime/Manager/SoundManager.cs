using System.Collections.Generic;
using UnityEngine;
using System;

namespace MoonRabbitRush
{
    public class SoundManager : MonoBehaviour
    {
        private static readonly Dictionary<Sound, List<string>> ResourcePaths = new()
        {
            {
                Sound.BGM,
                new List<string>()
                {
                    "Audio/BGM/Start/Start_01", "Audio/BGM/Start/Start_02"
                }
            },
            {
                Sound.SFX,
                new List<string>()
                {
                    "Audio/SFX/Click/Click_01","Audio/SFX/GameOver/GameOver_01",
                }
            },
        };

        private Dictionary<AudioClip, Sound> _loadedClips = new Dictionary<AudioClip, Sound>();
        private Dictionary<string, AudioClip> _clipCache = new Dictionary<string, AudioClip>();

        private const int SfxSourcePoolSize = 8;

        private AudioSource[] _sfxSources;
        private int _nextSourceIndex;

        private float _sfxVolume = 1f;
        private float _bgmVolume = 1f;
        public float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                SetSFXVolume(_sfxVolume);
            }
        }

        public float BgmVolume
        {
            get => _bgmVolume;
            set
            {
                _bgmVolume = Mathf.Clamp01(value);
                SetBGMVolume(_bgmVolume);
            }
        }

        private void Awake()
        {
            InitializeSources();
        }

        private void SetBGMVolume(float bgmVolume)
        {            
            foreach (var source in _sfxSources)
            {
                if (source.clip != null && _loadedClips.TryGetValue(source.clip, out var sound))
                {
                    if (sound == Sound.BGM)
                        source.volume = _bgmVolume;
                }
            }
        }

        private void SetSFXVolume(float sfxVolume)
        {
            foreach (var source in _sfxSources)
            {
                if (source.clip != null && _loadedClips.TryGetValue(source.clip, out var sound))
                {
                    if (sound == Sound.SFX)
                        source.volume = _sfxVolume;
                }
            }
        }

        private void InitializeSources()
        {
            _sfxSources = new AudioSource[SfxSourcePoolSize];

            for (int i = 0; i < SfxSourcePoolSize; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;

                _sfxSources[i] = source;
            }

            foreach (var resource in ResourcePaths)
            {
                foreach (var source in resource.Value)
                {
                    var clip = Resources.Load<AudioClip>(source);
                    if (clip != null)
                    {
                        _loadedClips[clip] = resource.Key;
                        _clipCache[source] = clip;
                    }
                }
            }
        }
        
        private AudioSource GetSource()
        {
            var source = _sfxSources[_nextSourceIndex];

            _nextSourceIndex = (_nextSourceIndex + 1) % _sfxSources.Length;

            return source;
        }

        public void Play(string sound, bool playOnAwake = false)
        {
            Play(sound, 1f);
        }

        public void Stop(string sound)
        {
            if(!TryGetClip(sound, out var clip))
                return;
            foreach (var source in _sfxSources)
            {
                if (source == null)
                    continue;
                if (source.isPlaying && source.clip == clip)
                {
                    source.Stop();
                }
            }
        }        

        public void Play(string sound, float volumeScale)
        {
            if (!TryGetClip(sound, out var clip))
                return;

            float volume = Mathf.Clamp01(_sfxVolume * volumeScale);

            var source = GetSource();
            if (source == null)
                return;

            source.PlayOneShot(clip, volume);
        }

        public void PlayBGM(string sound, bool loop = true)
        {
            if (!TryGetClip(sound, out var clip))
                return;

            var source = GetSource();
            if (source == null)
                return;

            source.clip = clip;
            source.loop = loop;
            source.volume = _bgmVolume;
            source.Play();
        }

        private bool TryGetClip(string path, out AudioClip clip)
        {
            if (_clipCache.TryGetValue(path, out clip))
            {
                return true;
            }

            clip = Resources.Load<AudioClip>(path);
            if (clip != null)
            {
                _clipCache[path] = clip;
                return true;
            }

            Debug.LogError($"[SoundManager] Audio clip not found at Resources/{path}");
            return false;
        }
    }
}
