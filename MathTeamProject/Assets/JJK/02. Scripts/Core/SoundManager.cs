using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoSingleton<SoundManager>
    {
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource bgmSource;

        private readonly Dictionary<Sound, AudioClip> _sfxDict = new();
        private readonly Dictionary<BGM, AudioClip> _bgmDict = new();
        private float _masterVolume;
        private float _sfxVolume;
        private float _bgmVolume;

        protected override void Awake()
        {
            base.Awake();
            _masterVolume = PlayerPrefs.GetFloat("masterVolume", 1f);
            _sfxVolume = PlayerPrefs.GetFloat("sfxVolume", 1f);
            _bgmVolume = PlayerPrefs.GetFloat("bgmVolume", 1f);
            sfxSource.volume = _masterVolume * _sfxVolume;
            bgmSource.volume = _masterVolume * _bgmVolume;

            foreach (Sound s in Enum.GetValues(typeof(Sound)))
                _sfxDict[s] = Resources.Load<AudioClip>($"SFX/{s}");

            foreach (BGM b in Enum.GetValues(typeof(BGM)))
                _bgmDict[b] = Resources.Load<AudioClip>($"BGM/{b}");
        }

        // Awake가 모두 끝난 뒤 GameSettings(DontDestroyOnLoad) 값으로 덮어씁니다.
        void Start()
        {
            var gs = GameSettings.Instance;
            if (gs == null) return;
            _masterVolume = gs.MasterVolume;
            _sfxVolume    = gs.SFXVolume;
            _bgmVolume    = gs.BGMVolume;
            if (sfxSource) sfxSource.volume = _masterVolume * _sfxVolume;
            if (bgmSource) bgmSource.volume = _masterVolume * _bgmVolume;
        }

        public void PlaySFX(Sound sound, float volumeScale = 1f)
        {
            if (_sfxDict.TryGetValue(sound, out AudioClip clip) && clip != null)
                sfxSource.PlayOneShot(clip, volumeScale);
        }

        public void PlayBGM(BGM bgm)
        {
            if (!_bgmDict.TryGetValue(bgm, out AudioClip clip) || clip == null) return;
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        public void StopBGM() => bgmSource.Stop();

        public void PauseBGM() => bgmSource.Pause();

        public void ResumeBGM() => bgmSource.UnPause();

        public void SetMasterVolume(float value)
        {
            _masterVolume = Mathf.Clamp01(value);
            sfxSource.volume = _masterVolume * _sfxVolume;
            bgmSource.volume = _masterVolume * _bgmVolume;
            PlayerPrefs.SetFloat("masterVolume", _masterVolume);
            PlayerPrefs.Save();
        }

        public void SetSfxVolume(float value)
        {
            _sfxVolume = Mathf.Clamp01(value);
            sfxSource.volume = _masterVolume * _sfxVolume;
            PlayerPrefs.SetFloat("sfxVolume", _sfxVolume);
            PlayerPrefs.Save();
        }

        public void SetBgmVolume(float value)
        {
            _bgmVolume = Mathf.Clamp01(value);
            bgmSource.volume = _masterVolume * _bgmVolume;
            PlayerPrefs.SetFloat("bgmVolume", _bgmVolume);
            PlayerPrefs.Save();
        }

        public void DuckBGM(float multiplier = 0.2f) => bgmSource.volume = _masterVolume * _bgmVolume * multiplier;
        public void UnduckBGM() => bgmSource.volume = _masterVolume * _bgmVolume;

        public float GetMasterVolume() => _masterVolume;
        public float GetSfxVolume() => _sfxVolume;
        public float GetBgmVolume() => _bgmVolume;
    }
    
    public enum Sound
    {
        GetBuff,
        Shoot,
        Hit
    }
    
    public enum BGM
    {
        TitleScene, 
        GameScene,
        EndingScene
    }
