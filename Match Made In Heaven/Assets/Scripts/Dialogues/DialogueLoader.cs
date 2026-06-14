using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogueLoader : MonoBehaviour
{
    [SerializeField] private TextAsset _jsonFile;
    private DialogueContainer _dialogueData;

    void Awake()
    {
        if (_jsonFile != null)
        {
            _dialogueData = JsonUtility.FromJson<DialogueContainer>(_jsonFile.text);
        }
    }

    /// <summary>
    /// Returns DialogueNode <see langword="from"/> the JSON <see langword="file"/>.  
    /// </summary>
    /// <param name="id">ID of the node</param>
    /// <returns>First DialogueNode associated <see langword="with"/> this <paramref name="id"/> </returns>
    public DialogueNode GetNode(string id)
    {
        return _dialogueData.nodes.FirstOrDefault(n => n.node_id == id);
    }

    /// <summary>
    /// Returnes first DialogueNode <see langword="in"/> the JSON <see langword="file"/>. 
    /// </summary>
    /// <returns>First DialogueNode it finds</returns>
    public DialogueNode GetFirstNode()
    {
        return _dialogueData.nodes[0];
    }

    public int GetTotalWordleCount()
    {
        if (_dialogueData?.nodes == null) return 0;
        return _dialogueData.nodes.Count(n => !string.IsNullOrEmpty(n.wordle_solution));
    }

    public bool AreAllWordlesSolved(IReadOnlyCollection<string> solvedWordles)
    {
        if (_dialogueData?.nodes == null || solvedWordles == null) return false;

        var requiredWordles = _dialogueData.nodes
            .Where(n => !string.IsNullOrEmpty(n.wordle_solution))
            .Select(n => n.wordle_solution)
            .ToList();

        if (requiredWordles.Count == 0) return false;

        var solved = new HashSet<string>(solvedWordles, System.StringComparer.OrdinalIgnoreCase);
        return requiredWordles.All(word => solved.Contains(word));
    }
}