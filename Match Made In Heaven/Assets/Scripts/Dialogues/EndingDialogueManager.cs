using System.Linq;
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

    public void StartEndingDialogue(EndingData data, bool success)
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

    private void ShowEndingCredits(string endingKey)
    {
        _endingMenuManager.StopEndingMusic();
        _creditsManager.ShowCredits(true, endingKey);
    }

    private void DisplayEndingStep(EndingDialogue dialogue, EndingData fullData)
    {
        if (dialogue == null || string.IsNullOrEmpty(dialogue.text))
        {
            string endingKey = (fullData != null) ? fullData.name : "Unhappy";
            ShowEndingCredits(endingKey);
            return;
        }

        slideshowSequence = SplitTextIntoPages(dialogue.text);
        currentSlideIndex = 0;
        DisplayCurrentSlide(dialogue, fullData);
    }

    private void DisplayCurrentSlide(EndingDialogue dialogue, EndingData fullData)
    {
        string speakerName = ResolveSpeakerName(dialogue, fullData);

        _dialogueManager.WriteEnding(slideshowSequence[currentSlideIndex], speakerName);
        _buttonCreator.ClearButtons();

        if (currentSlideIndex < slideshowSequence.Length - 1)
        {
            _buttonCreator.ShowContinue(() => {
                currentSlideIndex++;
                DisplayCurrentSlide(dialogue, fullData);
            });
        }
        else
        {
            ShowEndingChoices(dialogue, fullData);
        }
    }

    private static string ResolveSpeakerName(EndingDialogue dialogue, EndingData fullData)
    {
        if (fullData != null) return fullData.name;
        if (!string.IsNullOrEmpty(dialogue.name)) return dialogue.name;
        return "Presenter";
    }

    private static string[] SplitTextIntoPages(string text)
    {
        return text
            .Split('\n')
            .Select(page => page.Trim())
            .Where(page => !string.IsNullOrEmpty(page))
            .ToArray();
    }

    private void ShowEndingChoices(EndingDialogue dialogue, EndingData fullData)
    {
        if (dialogue.choices != null && dialogue.choices.Count > 0)
        {
            foreach (var choice in dialogue.choices)
            {
                EndingChoice currentChoice = choice;
                _buttonCreator.ShowContinueCustom(currentChoice.text, () => {
                    if (currentChoice.end) {
                        string endingKey = (fullData != null) ? fullData.name : "Unhappy";
                        Debug.Log("[Ending Dialogue manager]: endingKey is " + endingKey);
                        ShowEndingCredits(endingKey);
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
                ShowEndingCredits(endingKey);
            });
        }
    }
}