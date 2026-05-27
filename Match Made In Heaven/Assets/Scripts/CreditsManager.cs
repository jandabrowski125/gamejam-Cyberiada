using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CreditsManager : MonoBehaviour
{

    [Header("Key dependencies")]
    [SerializeField] private CharacterDatabase _characterDB;
    
    [Header("Ending Pictures")]
    [SerializeField] private Image _finalPictureDisplay;
    [SerializeField] private GameObject _UIcanvas;
    
    [Header("Credits")]
    [SerializeField] private string[] _creditsSceneNames = { "Credits1", "Credits2", "Credits3" } ;

    private int _currentSlideIndex = 0;

    private void Start()
    {
        HelperFunctions.IsAnyNull
        (
            "CreditsManager",
            _characterDB,
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
        if (gameFinished && String.IsNullOrEmpty(ending))
        {
            _creditsSceneNames.Prepend(ending);
        }
        
        _currentSlideIndex = 0;

        ShowEndingPicture(_creditsSceneNames[_currentSlideIndex]);
    }

    public void OnBackgroundClicked()
    {
        _currentSlideIndex++;

        if (_currentSlideIndex < _creditsSceneNames.Length)
        {
            ShowEndingPicture(_creditsSceneNames[_currentSlideIndex]);
        }
        else
        {
            _UIcanvas.SetActive(false);
            _finalPictureDisplay.gameObject.SetActive(false);
            GameEvents.TriggerCredintsEnded();
        }
    }

    private void ShowEndingPicture(string key)
    {
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