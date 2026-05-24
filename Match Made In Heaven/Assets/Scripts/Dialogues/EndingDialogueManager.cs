using UnityEngine;
using UnityEngine.UI;

public class EndingDialogueManager : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private DialogueWriter _dialogueWriter;
    [SerializeField] private DialogueManager _dialogueManager;
    [SerializeField] private ButtonCreator _buttonCreator;
    [SerializeField] private EndingManager _endingMenuManager;
    [SerializeField] private CharacterDatabase _characterDB;

    [Header("Ending Pictures")]
    [SerializeField] private Image _finalPictureDisplay;
    [SerializeField] private GameObject _UIcanvas;


    private string[] slideshowSequence;
    private int currentSlideIndex = 0;

    //check if any references are null
    private void Start()
    {
        HelperFunctions.IsAnyNull(
            "EndingDialogueManager - Core",
            _dialogueWriter,
            _dialogueManager,
            _buttonCreator,
            _endingMenuManager,
            _UIcanvas
            );

        HelperFunctions.IsAnyNull(
            "EndingDialogueManager - Endings",
            _finalPictureDisplay
            );
    }

    public void StartEndingDialogue(EndingData data, bool success, string credits)
    {        
        EndingDialogue dialogueToShow = success ? data.acceptance_dialogue : data.rejection_dialogue;
        DisplayEndingStep(dialogueToShow, data);
    }
    private void ShowCharacterSelection()
    {
        _dialogueWriter.Hide();
        _buttonCreator.ClearButtons();
        _endingMenuManager.ReturnToSelection();
    }

    public void StartUnhappyEnding(EndingDialogue unhappyDialogue, string credits)
    {
        DisplayEndingStep(unhappyDialogue, null);
    }

    private void DisplayEndingStep(EndingDialogue dialogue, EndingData fullData)
    {
        if (dialogue == null || string.IsNullOrEmpty(dialogue.text))
        {
            Debug.LogError("<color=red>[JSON ERROR]</color> Obiekt dialogu jest pusty! Sprawdź, czy w JSONie istnieje poprawnie zapisane 'unhappy_ending'.");
            FinalizeGame(fullData);
            return;
        }

        // Jeśli fullData to null (jak w unhappy_ending), mówi Narrator
        string speakerName = (fullData != null) ? fullData.name : "Presenter";

        _dialogueManager.WriteEnding(dialogue.text, speakerName);
        _buttonCreator.ClearButtons();

        if (dialogue.choices != null && dialogue.choices.Count > 0) //TODO: zmienic json parser tak aby choices zawsze istnialo
        {
            foreach (var choice in dialogue.choices)
            {
                EndingChoice currentChoice = choice; 
                _buttonCreator.ShowContinueCustom(currentChoice.text, () => {
                    if (currentChoice.end) {
                        FinalizeGame(fullData);
                    } else {
                        ShowCharacterSelection();
                    }
                });
            }
        }
        else
        {

            _buttonCreator.ShowContinueCustom("[ Zakończ grę ]", () => { 
                FinalizeGame(fullData);
            });
        }
        
    }

    private void FinalizeGame(EndingData data)
    {
        _buttonCreator.ClearButtons();
        _dialogueWriter.Hide();

        string endingKey = (data != null) ? data.name : "Unhappy";
        
        slideshowSequence = new string[] { endingKey, "Credits1", "Credits2", "Credits3" };
        
        currentSlideIndex = 0;

        ShowEndingPicture(slideshowSequence[currentSlideIndex]);
    }

    public void OnBackgroundClicked()
    {
        currentSlideIndex++;

        if (currentSlideIndex < slideshowSequence.Length)
        {
            ShowEndingPicture(slideshowSequence[currentSlideIndex]);
        }
        else
        {
            Debug.Log("<color=gold>GRA CAŁKOWICIE ZAKOŃCZONA!</color>");
            
            Application.Quit(); 
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