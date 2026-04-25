using UnityEngine;
using System.Collections.Generic;

public class EndingDialogueManager : MonoBehaviour
{
    [Header("References")]
    public DialogueWriter dialogueWriter;
    public DialogueManager dialogueManager;
    public ButtonCreator buttonCreator;
    public EndingManager endingMenuManager;
    public AudioManager audioManager;

    private string creditsText;

    public void StartEndingDialogue(EndingData data, bool success, string credits)
    {
        creditsText = credits;
        
        EndingDialogue dialogueToShow = success ? data.acceptance_dialogue : data.rejection_dialogue;
        
        DisplayEndingStep(dialogueToShow, data);
    }

    // --- POPRAWKA: Używamy referencji do EndingManager, zamiast szukać Canvasa ---
    private void ShowCharacterSelection()
    {
        if (dialogueWriter != null) dialogueWriter.Hide();
        if (buttonCreator != null) buttonCreator.ClearButtons();

        // Po prostu każemy wrócić do wyboru. 
        // EndingManager sam sprawdzi, czy zostało mu jeszcze jakieś UI do pokazania.
        if (endingMenuManager != null) 
            endingMenuManager.ReturnToSelection();
    }

    public void StartUnhappyEnding(EndingDialogue unhappyDialogue, string credits)
    {
        creditsText = credits;
        // Przekazujemy null, bo to nie jest rozmowa z konkretną postacią
        DisplayEndingStep(unhappyDialogue, null); 
    }

    private void DisplayEndingStep(EndingDialogue dialogue, EndingData fullData)
    {
        if (fullData == null)
        {
            name = "Presenter";
        }
        else
        {
            name = fullData.name;
        }

        dialogueManager.WriteEnding(dialogue.text, name);

        buttonCreator.ClearButtons();
        foreach (var choice in dialogue.choices)
        {
            EndingChoice currentChoice = choice; 
            buttonCreator.ShowContinueCustom(currentChoice.text, () => {
                if (currentChoice.end) {
                    FinalizeGame(fullData);
                } else {
                    ShowCharacterSelection();
                }
            });
        }
    }

    private void FinalizeGame(EndingData data)
    {
        buttonCreator.ClearButtons();
        dialogueManager.WriteEnding(data.ending, "Narrator");
        
        Invoke("ShowCredits", 4f);
    }

    private void ShowCredits()
    {
        // Używamy WriteRaw z Writera, bo kredyty to często czysty tekst na czarnym tle
        dialogueWriter.WriteRaw(creditsText, "SYSTEM", null, true, true);
        Debug.Log("GAME OVER");
    }
}