using UnityEngine;
using System.IO;
using System.Linq;

public class DialogueLoader : MonoBehaviour
{
    public TextAsset jsonFile;
    private DialogueContainer dialogueData;

    void Awake()
    {
        if (jsonFile != null)
        {
            // Zamiana tekstu JSON na obiekty C#
            dialogueData = JsonUtility.FromJson<DialogueContainer>(jsonFile.text);
        }
    }

    public DialogueNode GetNode(string id)
    {
        // Szukamy węzła o konkretnym ID w liście
        return dialogueData.nodes.FirstOrDefault(n => n.node_id == id);
    }

    public DialogueNode GetFirstNode()
    {
        return dialogueData.nodes[0];
    }
}