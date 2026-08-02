using System.Collections.Generic;
using UnityEngine;
using System;

namespace MoonRabbitRush
{
    public class SoundManager : MonoBehaviour
    {
        private static readonly Dictionary<Sound, string> ResourcePaths = new()
        {
            { Sound.PlayerHit, "Audio/SFX/SFX_PlayerHit" },
            { Sound.PlayerDeath, "Audio/SFX/SFX_PlayerDeath" },
            { Sound.EnemyHit, "Audio/SFX/SFX_EnemyHit" },
            { Sound.EnemyDeath, "Audio/SFX/SFX_EnemyDeath" },
            { Sound.BossAlert, "Audio/SFX/SFX_BossAlert" },
            { Sound.LevelUp, "Audio/SFX/SFX_LevelUp" },
            { Sound.WeaponFire, "Audio/SFX/SFX_WeaponFire" },
            { Sound.UiClick, "Audio/SFX/SFX_UiClick" },
        };

        private const int SfxSourcePoolSize = 8;

        private readonly Dictionary<Sound, AudioClip> _clipCache = new();
        private readonly Dictionary<Sound, AudioClip> _registeredClips = new();

        private AudioSource[] _sfxSources;
        private int _nextSourceIndex;

        private float _sfxVolume = 1f;

        public float SfxVolume
        {
            get => _sfxVolume;
            set => _sfxVolume = Mathf.Clamp01(value);
        }

        private void Awake()
        {
            InitializeSources();
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
        }

        private AudioSource GetSource()
        {
            var source = _sfxSources[_nextSourceIndex];

            _nextSourceIndex = (_nextSourceIndex + 1) % _sfxSources.Length;

            return source;
        }

        public void Play(Sound sound)
        {
            Play(sound, 1f);
        }

        public void Play(Sound sound, float volumeScale)
        {
            if (!TryGetClip(sound, out var clip))
                return;

            float volume = Mathf.Clamp01(_sfxVolume * volumeScale);

            var source = GetSource();
            source.PlayOneShot(clip, volume);
        }

        public void Register(Sound sound, string resourcesPath)
        {
            if (sound == Sound.None)
            {
                Debug.LogError("[SoundManager] Cannot register Sound.None.");
                return;
            }

            if (string.IsNullOrWhiteSpace(resourcesPath))
            {
                Debug.LogError($"[SoundManager] Cannot register empty path for {sound}.");
                return;
            }

            ResourcePaths[sound] = resourcesPath;
            _clipCache.Remove(sound);
            _registeredClips.Remove(sound);
        }

        private bool TryGetClip(Sound sound, out AudioClip clip)
        {
            if (_registeredClips.TryGetValue(sound, out clip))
            {
                return true;
            }

            if (_clipCache.TryGetValue(sound, out clip))
            {
                return true;
            }

            if (!ResourcePaths.TryGetValue(sound, out var resourcesPath))
            {
                Debug.LogError($"[SoundManager] Unregistered sound: {sound}");
                clip = null;
                return false;
            }

            clip = Resources.Load<AudioClip>(resourcesPath);
            if (clip == null)
            {
                Debug.LogError($"[SoundManager] Missing audio clip at Resources/{resourcesPath} for {sound}.");
                return false;
            }

            _clipCache[sound] = clip;
            return true;
        }           
    }
}
