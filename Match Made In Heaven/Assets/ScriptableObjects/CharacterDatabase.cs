using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "ScriptableObjects/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    [Serializable]
    public struct CharacterEntry
    {
        public string Name;
        public Sprite Portrait;
        public Sprite Background;
        public TMP_FontAsset Font;
        public Sprite EndingPicture;
        public AudioResource Voice;
        public AudioResource EndingMusic;
    }

    [SerializeField] private List<CharacterEntry> characters;

    /// <summary>
    /// Returns the sprite of the character <see langword="from"/> the characterDB. 
    /// </summary>
    /// <param name="characterName"></param>
    /// <returns></returns>
    public Sprite GetSprite(string characterName)
    {
        var entry = characters.FirstOrDefault(c => c.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
        return entry.Portrait;
    }

    /// <summary>
    /// Returns the background sprite of the character <see langword="from"/> the characterDB. 
    /// </summary>
    /// <param name="characterName"></param>
    /// <returns></returns>
    public Sprite GetBackground(string characterName)
    {
        var entry = characters.FirstOrDefault(c => c.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
        return entry.Background;
    }

    /// <summary>
    /// Returns the font of the character's language <see langword="from"/> the characterDB. 
    /// </summary>
    /// <param name="characterName"></param>
    /// <returns></returns>
    public TMP_FontAsset GetFont(string characterName)
    {
        var entry = characters.FirstOrDefault(c => c.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
        return entry.Font;
    }

    /// <summary>
    /// Returns the happy ending picture of the character <see langword="from"/> the characterDB. 
    /// </summary>
    /// <param name="characterName"></param>
    /// <returns></returns>
    public Sprite GetEndingPicture(string characterName)
    {
        var entry = characters.FirstOrDefault(c => c.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
        return entry.EndingPicture;
    }

    /// Retrieves the characters' voices from the CharacterDB.
    /// <param name="characterName"></param>
    /// <summary>
    /// Returns a name - audioGenerator dictionary.
    /// </summary>
    /// <returns>
    /// Dictionary: key- characterName, value- AudioRandomGroup of the character's voices
    /// </returnes>
    public Dictionary<string, AudioResource> GetVoices()
    {
        return characters.ToDictionary(
            character => character.Name, 
            character => character.Voice
        );
    }

    /// <summary>
    /// Plays the ending music of the character.
    /// </summary>
    /// <param name="characterName"></param>
    public Dictionary<string, AudioResource> GetEndingMusicSamples()
    {
        return characters.ToDictionary(
            character => character.Name,
            character => character.EndingMusic
        );
    } 
}