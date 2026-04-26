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
        dialogueWriter.Hide(); // Ukrywamy okno dialogowe

        string endingKey = (data != null) ? data.name : "Unhappy";
        
        // Zamiast pokazywać od razu, uruchamiamy sekwencję "pokazu slajdów"
        StartCoroutine(EndingSlideshowSequence(endingKey));
    }

    private System.Collections.IEnumerator EndingSlideshowSequence(string characterKey)
    {
        // 1. Pokaż obrazek zakończenia postaci (lub Unhappy)
        ShowEndingPicture(characterKey);
        yield return new WaitForSeconds(5f);

        // 2. Pokaż pierwszą planszę z napisami
        ShowEndingPicture("Credits1");
        yield return new WaitForSeconds(5f);

        // 3. Pokaż drugą planszę
        ShowEndingPicture("Credits2");
        yield return new WaitForSeconds(5f);

        // 4. Pokaż trzecią planszę
        ShowEndingPicture("Credits3");
        yield return new WaitForSeconds(5f);

        // KONIEC GRY
        Debug.Log("<color=gold>GRA CAŁKOWICIE ZAKOŃCZONA!</color>");
        
        // (Opcjonalnie: Tutaj możesz dodać kod wracający do Main Menu, 
        // np. UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); )
    }

    // Nowa metoda pomocnicza, żeby nie powtarzać kodu w korutynie
    private void ShowEndingPicture(string key)
    {
        // Szukamy obrazka z przypisaną nazwą
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