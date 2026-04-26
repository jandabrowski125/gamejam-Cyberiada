using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    public DialogueWriter dialogueWriter;
    public DialogueLoader loader;
    public ButtonCreator buttonCreator;
    [SerializeField] private AudioManager _audioManager;
    public EndingManager endingManager;
    public StatsManager statsManager;

    [Header("Character System (Sprite2D)")]
    public CharacterDatabase characterDB;
    public SpriteRenderer characterPortraitRenderer;
    public SpriteRenderer backgroundImageRenderer;
    public AudioSource backgroundAudioSource;
    private string lastNodeId;

    private void OnEnable()
    {
        // Nasłuchiwanie eventów
        GameEvents.OnWordleRequired += HandleWordleRequested;
        GameEvents.OnWordleSuccess += HandleWordleResult;
    }

    private void OnDisable()
    {
        GameEvents.OnWordleRequired -= HandleWordleRequested;
        GameEvents.OnWordleSuccess -= HandleWordleResult;
    }


    public void Write(string nodeId)
    {
        lastNodeId = nodeId;
        DialogueNode node = loader.GetNode(nodeId);

        if (node != null)
        {
            UpdatePortrait(node.speaker);

            if (backgroundAudioSource != null && !backgroundAudioSource.isPlaying)
            {
                backgroundAudioSource.Play();
            }

            bool isPresenter = node.speaker.Equals("Presenter", System.StringComparison.OrdinalIgnoreCase);

            // 2. Wypisujemy tekst (jeśli Prezenter, to forceUnderstandable = true)
            dialogueWriter.Write(nodeId, null, false, isPresenter, node.wordle_solution);
            _audioManager.PlayVoice(node.speaker);

            if (isPresenter)
            {
                if (node.choices.Count == 0)
                {
                    buttonCreator.ShowContinue(() => {
                        if (!string.IsNullOrEmpty(node.next_node)) Write(node.next_node);
                        else StartEnding();
                    });
                    return;
                }
                buttonCreator.ShowChoices(node, (choice) => HandleChoice(choice, node));
            }
            else
            {
                buttonCreator.ShowContinue(() => StartWordleChallenge());
            }
        }
    }

    public void CompleteDialogueInstantly()
    {
        if (dialogueWriter != null)
        {
            dialogueWriter.CompleteInstantly();
        }
    }

    private void StartEnding()
    {
        Debug.Log("[DialogueManager] Brak kolejnego węzła. Uruchamiam zakończenie gry.");

        if (backgroundAudioSource != null) backgroundAudioSource.Stop();
        
        // Czyścimy obecny stan
        dialogueWriter.Hide();
        buttonCreator.ClearButtons();
        
        if (endingManager != null && statsManager != null)
        {
            endingManager.StartEndingPhase(statsManager.GetFinalStats());
        }
        else
        {
            Debug.LogError("DialogueManager: Brakuje referencji do EndingManager lub StatsManager!");
        }
    }

    private void UpdatePortrait(string speakerName)
    {
        if (characterDB == null || characterPortraitRenderer == null || backgroundImageRenderer == null) return;
        ;

        Sprite speakerSprite = characterDB.GetSprite(speakerName);
        Sprite backgroundSprite = characterDB.GetBackground(speakerName);

        if (speakerSprite != null && backgroundSprite != null)
        {
            characterPortraitRenderer.sprite = speakerSprite; 
            characterPortraitRenderer.gameObject.SetActive(true);
            characterPortraitRenderer.color = Color.white;

            backgroundImageRenderer.sprite = backgroundSprite;
            backgroundImageRenderer.gameObject.SetActive(true);
            backgroundImageRenderer.color = Color.white;
        }
        else
        {
            characterPortraitRenderer.gameObject.SetActive(false);
            backgroundImageRenderer.gameObject.SetActive(false);
        }
    }

    private void StartWordleChallenge()
    {
        DialogueNode node = loader.GetNode(lastNodeId);
        if (node != null && !string.IsNullOrEmpty(node.wordle_solution))
        {
            GameEvents.TriggerWordleRequired(node.wordle_solution);
        }
    }

    private void HandleWordleRequested(string solution)
    {
        if (backgroundAudioSource != null && backgroundAudioSource.isPlaying)
        {
            backgroundAudioSource.Pause();
        }
        dialogueWriter.Hide();
        buttonCreator.ClearButtons();
    }

    private void HandleWordleResult(string foundKeyword)
    {
        if (string.IsNullOrEmpty(lastNodeId)) return;
        DialogueNode node = loader.GetNode(lastNodeId);

        if (backgroundAudioSource != null)
        {
            backgroundAudioSource.UnPause();
        }

        if (!string.IsNullOrEmpty(foundKeyword))
        {
            // --- NOWOŚĆ: Dodajemy słowo do znanego słownika na stałe! ---
            dialogueWriter.AddKnownWord(foundKeyword);
            
            dialogueWriter.Write(lastNodeId, foundKeyword, false, false, node.wordle_solution);
        }
        else
        {
            dialogueWriter.Write(lastNodeId, null, true);
        }

        // Po Wordle zawsze pokazujemy wybory
        buttonCreator.ShowChoices(node, (choice) => HandleChoice(choice, node));
    }

    private void HandleChoice(DialogueChoice choice, DialogueNode currentNode)
    {
        GameEvents.TriggerStatsChanged(currentNode.speaker, choice.plus_paragon, choice.plus_renegade);

        bool isPresenter = currentNode.speaker.Equals("Prezenter", System.StringComparison.OrdinalIgnoreCase);

        if (!string.IsNullOrEmpty(choice.follow_up))
        {
            // Follow-up prezentera też musi być zrozumiały
            dialogueWriter.WriteRaw(choice.follow_up, currentNode.speaker, null, false, isPresenter);
            _audioManager.PlayVoice(currentNode.speaker);

            buttonCreator.ShowContinue(() => {
                if (!string.IsNullOrEmpty(currentNode.next_node)) Write(currentNode.next_node);
                else StartEnding();
            });
        }
        else
        {
            if (!string.IsNullOrEmpty(currentNode.next_node)) Write(currentNode.next_node);
            else StartEnding();
        }
    }
    public void WriteEnding(string text, string speakerName)
    {
        // 1. Najpierw ustawiamy wizualia (Postać + Tło)
        UpdatePortrait(speakerName);

        // 2. Potem każemy Writerowi wypisać tekst finałowy
        dialogueWriter.WriteEnding(text);
        
        // 3. Opcjonalnie: odpal głos (jeśli masz przygotowane klipy pod finał)
        if (_audioManager != null) _audioManager.PlayVoice(speakerName);
    }
}