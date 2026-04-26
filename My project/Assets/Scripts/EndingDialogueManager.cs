using UnityEngine;
using UnityEngine.UI; // Wymagane dla komponentu Image
using System.Collections.Generic;
using System.Linq;

// Dodaj tę strukturę, żeby ładnie połączyć imiona z obrazkami
[System.Serializable]
public struct EndingPicture
{
    public string characterName; // Np. "CyberJohn", "AlienQueen" albo "Unhappy"
    public Sprite picture;
}

public class EndingDialogueManager : MonoBehaviour
{
    [Header("References")]
    public DialogueWriter dialogueWriter;
    public DialogueManager dialogueManager;
    public ButtonCreator buttonCreator;
    public EndingManager endingMenuManager;
    public AudioManager audioManager;
    public GameObject UIcanvas;

    [Header("Ending Pictures")]
    public Image finalPictureDisplay; // Canvasowy obiekt Image, który przykryje ekran
    public List<EndingPicture> endingPictures; // Twoja lista obrazków końcowych

    private string[] slideshowSequence;
    private int currentSlideIndex = 0;

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
        // 1. Zabezpieczenie przed brakiem dialogu
        if (dialogue == null || string.IsNullOrEmpty(dialogue.text))
        {
            Debug.LogError("<color=red>[JSON ERROR]</color> Obiekt dialogu jest pusty! Sprawdź, czy w JSONie istnieje poprawnie zapisane 'unhappy_ending'.");
            
            // Ratunkowe zakończenie gry, żeby nie zawiesić ekranu
            FinalizeGame(fullData);
            return;
        }

        // 2. Jeśli fullData to null (jak w unhappy_ending), mówi Narrator
        string speakerName = (fullData != null) ? fullData.name : "Presenter";

        dialogueManager.WriteEnding(dialogue.text, speakerName);
        buttonCreator.ClearButtons();

        // 3. BEZPIECZNA PĘTLA: Sprawdzamy czy lista 'choices' w ogóle istnieje
        if (dialogue.choices != null && dialogue.choices.Count > 0)
        {
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
        else
        {
            // FALLBACK: Jeśli zapomniałeś dodać "choices" w JSONie dla unhappy_ending,
            // skrypt wygeneruje domyślny przycisk kończący grę.
            buttonCreator.ShowContinueCustom("[ Zakończ grę ]", () => {
                FinalizeGame(fullData);
            });
        }
        
    }

    private void FinalizeGame(EndingData data)
    {
        buttonCreator.ClearButtons();
        dialogueWriter.Hide();

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
        Sprite spriteToShow = endingPictures.FirstOrDefault(p => p.characterName == key).picture;

        if (spriteToShow != null && finalPictureDisplay != null)
        {
            finalPictureDisplay.sprite = spriteToShow;
            UIcanvas.SetActive(true);
            finalPictureDisplay.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[Ending] Brakuje obrazka dla klucza: {key}");
        }
    }
}