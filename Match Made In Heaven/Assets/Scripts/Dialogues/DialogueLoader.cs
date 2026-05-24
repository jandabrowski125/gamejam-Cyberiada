using UnityEngine;
using System.Linq;

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
}