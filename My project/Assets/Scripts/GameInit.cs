using UnityEngine;

public class GameInit : MonoBehaviour
{
    [Header("References")]
    public DialogueLoader loader; // Przeciągnij tu obiekt z DialogueLoaderem
    public string testNodeID = "node_001"; // ID węzła, który chcesz przetestować

    void Start()
    {
        // Dajemy chwilę na załadowanie JSONa w Awake loaderze
        Invoke("LaunchTestWordle", 0.1f);
    }

    void LaunchTestWordle()
    {
        if (loader == null)
        {
            Debug.LogError("GameInit: Brakuje referencji do DialogueLoader!");
            return;
        }

        DialogueNode node = loader.GetNode(testNodeID);

        if (node != null && !string.IsNullOrEmpty(node.wordle_solution))
        {
            Debug.Log($"[GameInit] Testowanie Wordle dla słowa: {node.wordle_solution}");
            
            GameEvents.TriggerWordleRequired(node.wordle_solution);
        }
        else
        {
            Debug.LogError($"[GameInit] Nie znaleziono słowa Wordle w węźle: {testNodeID}");
        }
    }

    
    private void OnEnable()
    {
        GameEvents.OnWordleSuccess += HandleSuccess;
    }

    private void OnDisable()
    {
        GameEvents.OnWordleSuccess -= HandleSuccess;
    }

    private void HandleSuccess(string word)
    {
        Debug.Log($"<color=green>[GameInit] Test zakończony sukcesem! Odgadnięto: {word}</color>");
    }
}