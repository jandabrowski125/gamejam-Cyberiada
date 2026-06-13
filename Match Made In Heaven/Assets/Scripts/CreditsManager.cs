using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CreditsManager : MonoBehaviour
{

    [Header("Key dependencies")]
    [SerializeField] private CharacterDatabase _characterDB;
    [SerializeField] private FadingEffects _fadingEffects;
    
    [Header("Ending Pictures")]
    [SerializeField] private Image _finalPictureDisplay;
    [SerializeField] private GameObject _UIcanvas;
    
    [Header("Credits")]
    [SerializeField] private List<string> _creditsSceneNames = new List<string>{ "Credits1", "Credits2", "Credits3" } ;

    private int _currentSlideIndex = 0;

    private void Start()
    {
        HelperFunctions.IsAnyNull
        (
            "CreditsManager",
            _characterDB,
            _fadingEffects,
            _finalPictureDisplay,
            _UIcanvas
        );
    }

    public void ShowCreditsWithoutEnding()
    {
        ShowCredits(false, "");
    }

    public void ShowCredits(
        bool gameFinished = false,
        string ending = ""
    )
    {   
        if (gameFinished && !String.IsNullOrEmpty(ending))
        {
            _creditsSceneNames.Insert(0, ending);
        }
        
        _currentSlideIndex = 0;

        ShowEndingPicture(_creditsSceneNames[_currentSlideIndex]);
    }

    public void OnBackgroundClicked()
    {
        _currentSlideIndex++;

        if (_currentSlideIndex < _creditsSceneNames.Count)
        {
            ShowEndingPicture(_creditsSceneNames[_currentSlideIndex]);
        }
        else
        {
            _fadingEffects.RequestFadeOut(fadeDuration: 0.5f, fadeMusicSource: false, forceSynchronous: true);
            _UIcanvas.SetActive(false);
            _finalPictureDisplay.gameObject.SetActive(false);
            GameEvents.TriggerCredintsEnded();
            _fadingEffects.RequestFadeIn(fadeDuration: 0.5f, fadeMusicSource: false, forceSynchronous: true);
        }
    }

    private void ShowEndingPicture(string key)
    {
        _fadingEffects.RequestFadeOut(fadeDuration: 0.5f, fadeMusicSource: false, forceSynchronous: true);
        _fadingEffects.RequestFadeIn(fadeDuration: 0.5f, fadeMusicSource: false, forceSynchronous: true);
        Sprite spriteToShow = _characterDB.GetEndingPicture(key);

        if (spriteToShow != null)
        {
            _finalPictureDisplay.sprite = spriteToShow;
            _UIcanvas.SetActive(true);
            _finalPictureDisplay.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[Ending] Brakuje obrazka dla klucza: {key}");
        }
    }
}