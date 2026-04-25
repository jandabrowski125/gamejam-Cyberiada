using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    public DialogueWriter dialogueWriter;
    public DialogueLoader loader;
    public ButtonCreator buttonCreator;

    [Header("Character System (Sprite2D)")]
    public CharacterDatabase characterDB;
    public SpriteRenderer characterPortraitRenderer;

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

            // 1. Sprawdzamy czy mówi Prezenter
            bool isPresenter = node.speaker.Equals("Presenter", System.StringComparison.OrdinalIgnoreCase);

            // 2. Wypisujemy tekst (jeśli Prezenter, to forceUnderstandable = true)
            dialogueWriter.Write(nodeId, null, false, isPresenter);

            // 3. Zarządzanie przyciskami po tekście
            if (isPresenter)
            {
                // Jeśli mówi Prezenter, nie ma Wordle! Od razu pokazujemy wybory (Choices)
                buttonCreator.ShowChoices(node, (choice) => HandleChoice(choice, node));
            }
            else
            {
                // Jeśli mówi ktokolwiek inny, standardowe Flow z Wordle
                buttonCreator.ShowContinue(() => StartWordleChallenge());
            }
        }
    }

    private void UpdatePortrait(string speakerName)
    {
        if (characterDB == null || characterPortraitRenderer == null) return;

        Sprite speakerSprite = characterDB.GetSprite(speakerName);

        if (speakerSprite != null)
        {
            characterPortraitRenderer.sprite = speakerSprite;
            
            characterPortraitRenderer.gameObject.SetActive(true);
            
            characterPortraitRenderer.color = Color.white;
        }
        else
        {
            characterPortraitRenderer.gameObject.SetActive(false);
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
        dialogueWriter.Hide();
        buttonCreator.ClearButtons();
    }

    private void HandleWordleResult(string foundKeyword)
    {
        if (string.IsNullOrEmpty(lastNodeId)) return;
        DialogueNode node = loader.GetNode(lastNodeId);

        if (!string.IsNullOrEmpty(foundKeyword))
        {
            // --- NOWOŚĆ: Dodajemy słowo do znanego słownika na stałe! ---
            dialogueWriter.AddKnownWord(foundKeyword);
            
            dialogueWriter.Write(lastNodeId, foundKeyword);
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
        GameEvents.TriggerStatsChanged(choice.plus_paragon, choice.plus_renegade);

        bool isPresenter = currentNode.speaker.Equals("Prezenter", System.StringComparison.OrdinalIgnoreCase);

        if (!string.IsNullOrEmpty(choice.follow_up))
        {
            // Follow-up prezentera też musi być zrozumiały
            dialogueWriter.WriteRaw(choice.follow_up, currentNode.speaker, null, false, isPresenter);
            
            buttonCreator.ShowContinue(() => {
                if (!string.IsNullOrEmpty(currentNode.next_node)) Write(currentNode.next_node);
            });
        }
        else
        {
            if (!string.IsNullOrEmpty(currentNode.next_node)) Write(currentNode.next_node);
        }
    }
}