using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    class AudioClipData
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1.2f)]
        public float volume = 1;
    }

    [SerializeField]
    private AudioClipData[] _audioClips;
    private Dictionary<string, AudioClipData> _audioClipDic = new Dictionary<string, AudioClipData>();

    private AudioSource _sfxAudioSource;
    private AudioSource _musicAudioSource;
    private string _currentMusic;

    private void Awake()
    {
        for (int i = 0; i < _audioClips.Length; i++)
        {
            _audioClipDic[_audioClips[i].name] = _audioClips[i];
        }

        GameObject go = new GameObject();
        go.transform.parent = transform;
        go.name = "SFXAudioSource";
        _sfxAudioSource = go.AddComponent<AudioSource>();

        go = new GameObject();
        go.transform.parent = transform;
        go.name = "MusicAudioSource";
        _musicAudioSource = go.AddComponent<AudioSource>();
        _musicAudioSource.loop = true;
    }

    public void PlaySFX(string sfx)
    {
        _sfxAudioSource.PlayOneShot(_audioClipDic[sfx].clip);
        _sfxAudioSource.volume = _audioClipDic[sfx].volume;
    }

    public void PlayMusic(string music)
    {
        if (_currentMusic != music)
        {
            _musicAudioSource.Stop();
            _musicAudioSource.clip = _audioClipDic[music].clip;
            _musicAudioSource.volume = _audioClipDic[music].volume;
            _musicAudioSource.Play();
            _currentMusic = music;
        }
    }

    public void StopMusic()
    {
        _musicAudioSource.Stop();
    }
}
