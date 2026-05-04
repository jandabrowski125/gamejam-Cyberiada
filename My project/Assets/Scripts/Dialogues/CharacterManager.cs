using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private CharacterDatabase _characterDB;
    private Dictionary<string, AudioSource> _playerVoices = new Dictionary<string, AudioSource>{};
    void Start()
    {
        var characterAudioGenerators = _characterDB.GetVoices();
        foreach(string characterName in characterAudioGenerators.Keys)
        {
            _playerVoices.Add(characterName, AssignAudioSource(characterAudioGenerators[characterName]));
        }
    }

    private AudioSource AssignAudioSource(AudioResource resource)
    {

        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.resource = resource;
        return audioSource;
    }

    public void PlayVoice(string characterName)
    {
        foreach(var voice in _playerVoices.Values) voice.Stop();
        _playerVoices[characterName].Play();
    }

    public Sprite GetSprite(string characterName) => _characterDB.GetSprite(characterName);
    public Sprite GetBackground(string characterName) => _characterDB.GetBackground(characterName);
}