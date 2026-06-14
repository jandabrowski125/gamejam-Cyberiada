using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private CharacterDatabase _characterDB;
    [SerializeField] private AudioMixerGroup _mixerGroup;
    private Dictionary<string, AudioSource> _playerVoices = new Dictionary<string, AudioSource>{};
    private Dictionary<string, AudioSource> _endingMusicSamples = new Dictionary<string, AudioSource>{};
    void Start()
    {
        var characterAudioGenerators = _characterDB.GetVoices();
        foreach(string characterName in characterAudioGenerators.Keys)
        {
            _playerVoices.Add(characterName, AssignAudioSource(characterAudioGenerators[characterName]));
        }

        var endingMusicAudioGenerators = _characterDB.GetEndingMusicSamples();
        foreach(string characterName in endingMusicAudioGenerators.Keys)
        {
            _endingMusicSamples.Add(characterName, AssignAudioSource(endingMusicAudioGenerators[characterName]));
        }
    }

    private AudioSource AssignAudioSource(AudioResource resource)
    {

        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.resource = resource;
        audioSource.outputAudioMixerGroup = _mixerGroup;
        return audioSource;
    }

    public void PlayVoice(string characterName)
    {
        foreach(var voice in _playerVoices.Values) voice.Stop();
        _playerVoices[characterName].Play();
    }

    public void PlayEndingMusic(string endingMusicId)
    {
        StopEndingMusic();
        _endingMusicSamples[endingMusicId].Play();
    }

    public void StopEndingMusic()
    {
        foreach(var music in _endingMusicSamples.Values) music.Stop();
    }

    public Sprite GetSprite(string characterName) => _characterDB.GetSprite(characterName);
    public Sprite GetBackground(string characterName) => _characterDB.GetBackground(characterName);
}