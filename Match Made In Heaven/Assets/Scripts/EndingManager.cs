using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingManager : MonoBehaviour
{
    [Header("References")]
    public TextAsset endingsJson;
    public CharacterDatabase characterDB;
    public EndingDialogueManager endingDialogueManager;
    [SerializeField] private FadingEffects _fadingEffects;
    [SerializeField] private WordleProgressTracker _wordleProgressTracker;

    [Header("Audio")]
    [SerializeField] private AudioSource _backgroundMusicSource;
    [SerializeField] private AudioSource _endingMusicSource;

    [Header("UI Layout")]
    public GameObject charButtonPrefab;
    public Transform buttonsParent;
    public GameObject selectionCanvas; 

    private EndingList endingList;
    private Dictionary<string, int> playerStats;
    private int remainingCharacters = 0;

    void Awake()
    {
        endingList = JsonUtility.FromJson<EndingList>(endingsJson.text);
    }

    private void OnEnable()
    {
        GameEvents.OnCreditsEnded += OnCreditsEndedHandler;
    }

    public void StartEndingPhase(Dictionary<string, int> stats)
    {   
        _fadingEffects.RequestFadeOut(
            fadeDuration: 0.5f,
            fadeMusicSource: false,
            forceSynchronous: true
        );

        StopBackgroundMusic();
        PlayEndingMusic();
        
        playerStats = stats;
        selectionCanvas.SetActive(true);
        GenerateCharacterButtons();

        _fadingEffects.RequestFadeIn(
            fadeDuration: 0.5f,
            fadeMusicSource: false,
            forceSynchronous: true
        );
    }

    private void GenerateCharacterButtons()
    {
        foreach (Transform child in buttonsParent) Destroy(child.gameObject);

        if (endingList == null || endingList.endings == null) return;

        remainingCharacters = 0;
        bool presenterUnlocked = IsPresenterUnlocked();

        foreach (var data in endingList.endings)
        {
            if (IsPresenterCharacter(data.name) && !presenterUnlocked)
                continue;

            CreateCharacterButton(data.name);
            remainingCharacters++;
        }

        if (presenterUnlocked && !endingList.endings.Any(e => IsPresenterCharacter(e.name)))
        {
            CreateCharacterButton(WordleProgressTracker.GetPresenterCharacterName());
            remainingCharacters++;
        }
    }

    private bool IsPresenterUnlocked()
    {
        return _wordleProgressTracker != null && _wordleProgressTracker.HasSolvedAllWordles();
    }

    private static bool IsPresenterCharacter(string characterName)
    {
        return characterName.Equals(
            WordleProgressTracker.GetPresenterCharacterName(),
            StringComparison.OrdinalIgnoreCase);
    }

    private void CreateCharacterButton(string characterName)
    {
        GameObject btnObj = Instantiate(charButtonPrefab, buttonsParent);

        Image portrait = btnObj.GetComponent<Image>();
        if (portrait != null && characterDB != null)
            portrait.sprite = characterDB.GetSprite(characterName);

        Button btn = btnObj.GetComponent<Button>();
        if (btn == null) return;

        GameObject buttonToDestroy = btnObj;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnCharacterSelected(characterName, buttonToDestroy));
    }

    private void OnCharacterSelected(string characterName, GameObject buttonClicked)
    {
        EndingData data = endingList.endings.FirstOrDefault(e => e.name == characterName);
        if (data == null) return;
        
        string statKey = characterName + "_paragon";
        playerStats.TryGetValue(statKey, out int score);
        int totalParagon = playerStats.Values.Where((v, i) => playerStats.Keys.ElementAt(i).EndsWith("_paragon")).Sum();

        bool success = score >= data.paragon_requirements && totalParagon >= data.overall_paragon_requirement;

        if (!success)
        {
            remainingCharacters--; 
            Destroy(buttonClicked); 
        }

        selectionCanvas.SetActive(false);
        endingDialogueManager.StartEndingDialogue(data, success);
    }

    public void StopEndingMusic()
    {
        if (_endingMusicSource == null) return;
        _endingMusicSource.Stop();
    }

    private void StopBackgroundMusic()
    {
        if (_backgroundMusicSource == null) return;
        _backgroundMusicSource.Stop();
    }

    private void PlayEndingMusic()
    {
        if (_endingMusicSource == null) return;
        if (!_endingMusicSource.isPlaying) _endingMusicSource.Play();
    }

    public void ReturnToSelection()
    {
        if (remainingCharacters <= 0)
        {
            endingDialogueManager.StartUnhappyEnding(endingList.unhappy_ending, endingList.credits);
        }
        else
        {
            _fadingEffects.RequestFadeOut(
                fadeDuration: 0.5f,
                fadeMusicSource: false,
                forceSynchronous: true
            );
            
            selectionCanvas.SetActive(true);
            PlayEndingMusic();

            _fadingEffects.RequestFadeIn(
                fadeDuration: 0.5f,
                fadeMusicSource: false,
                forceSynchronous: true
            );
        }
    }

    private void OnCreditsEndedHandler()
    {
        _fadingEffects.RequestFadeIn(
            fadeDuration: 0.5f,
            fadeMusicSource: false,
            forceSynchronous: true
        );
        SceneManager.LoadScene("Menu");
    }
}