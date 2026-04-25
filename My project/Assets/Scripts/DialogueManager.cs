using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    public DialogueWriter dialogueWriter;
    public DialogueLoader loader;
    public ButtonCreator buttonCreator;
    public AudioSource taliaAudio;

    private string lastNodeId; // Zapamiętujemy, o czym rozmawiamy

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
        
        // 1. Piszemy tekst (Typewriter)
        dialogueWriter.Write(nodeId);
        taliaAudio.Play();

        // 2. Pokazujemy przycisk Continue
        buttonCreator.ShowContinue(() => {
            // KROK 2: Po kliknięciu Continue odpalamy Wordle
            StartWordleChallenge();
        });
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
        buttonCreator.ClearButtons(); // Chowamy przycisk na czas gry
    }

    // 2. Gdy Wordle się kończy (OnWordleSuccess)
    private void HandleWordleResult(string foundKeyword)
    {
        if (string.IsNullOrEmpty(lastNodeId)) return;
        DialogueNode node = loader.GetNode(lastNodeId);

        // 1. Wypisujemy tekst ponownie (Natychmiast)
        if (!string.IsNullOrEmpty(foundKeyword))
            dialogueWriter.Write(lastNodeId, foundKeyword, true);
        else
            dialogueWriter.Write(lastNodeId, null, true);

        // 2. KROK 4: Pokazujemy finalne przyciski wyborów
        buttonCreator.ShowChoices(node, (choice) => {
            Debug.Log($"Wybrano: {choice.text}. Paragon: +{choice.plus_paragon}, Renegade: +{choice.plus_renegade}");
            // Tutaj możesz wywołać kolejny Node: Write(choice.next_node_id);
        });
    }
}