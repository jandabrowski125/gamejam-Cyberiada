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

        remainingCharacters = endingList.endings.Count;

        foreach (var data in endingList.endings)
            CreateCharacterButton(data.name);

        if (ShouldShowPresenterOption())
        {
            CreateCharacterButton(WordleProgressTracker.GetPresenterCharacterName());
            remainingCharacters++;
        }
    }

    private bool ShouldShowPresenterOption()
    {
        if (_wordleProgressTracker == null || !_wordleProgressTracker.HasSolvedAllWordles())
            return false;

        string presenterName = WordleProgressTracker.GetPresenterCharacterName();
        return !endingList.endings.Any(e =>
            e.name.Equals(presenterName, StringComparison.OrdinalIgnoreCase));
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