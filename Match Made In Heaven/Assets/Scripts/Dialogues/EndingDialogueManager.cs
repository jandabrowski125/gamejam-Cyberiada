using UnityEngine;
using UnityEngine.UI;

public class EndingDialogueManager : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private DialogueWriter _dialogueWriter;
    [SerializeField] private DialogueManager _dialogueManager;
    [SerializeField] private ButtonCreator _buttonCreator;
    [SerializeField] private EndingManager _endingMenuManager;
    [SerializeField] private CreditsManager _creditsManager;
    [SerializeField] private CharacterDatabase _characterDB;



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
            _creditsManager
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
            string endingKey = (fullData != null) ? fullData.name : "Unhappy";
            _creditsManager.ShowCredits(true, endingKey);
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
                        string endingKey = (fullData != null) ? fullData.name : "Unhappy";
                        _creditsManager.ShowCredits(true, endingKey);
                    } else {
                        ShowCharacterSelection();
                    }
                });
            }
        }
        else
        {
            _buttonCreator.ShowContinueCustom("[ Zakończ grę ]", () => { 
                string endingKey = (fullData != null) ? fullData.name : "Unhappy";
                _creditsManager.ShowCredits(true, endingKey);
            });
        }
    }
}