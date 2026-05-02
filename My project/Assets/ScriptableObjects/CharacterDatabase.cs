using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Dialogue/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    [Serializable]
    public struct CharacterEntry
    {
        public readonly string Name {get;}
        public readonly Sprite Portrait {get;}
        public readonly Sprite Background {get;}
        public readonly TMP_FontAsset Font {get;}
        public readonly Sprite EndingPicture {get;}
        public readonly AudioSource Voice {get;}
        public readonly AudioSource EndingMusic {get;}
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

    /// <summary>
    /// Plays the voice of the character.  
    /// </summary>
    /// <remarks>
    /// Stops other characters' voices so they don't overlap.
    /// </remarks>
    /// <param name="characterName"></param>
    public void PlayVoice(string characterName)
    {
        foreach(var character in characters)
        {
            if (character.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase))
            {
                character.Voice.Play();
                continue;
            }
            character.Voice.Stop();
        }
    }

    /// <summary>
    /// Plays the ending music of the character.
    /// </summary>
    /// <param name="characterName"></param>
    public void PlayEndingMusic(string characterName)
    {
        var entry = characters.FirstOrDefault(c => c.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
        entry.EndingMusic.Play();
    } 
}