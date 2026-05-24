using UnityEngine;

public class GameInit : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public string testNodeID = "node_001";
    public float delayBeforeWordle = 3.0f;

    void Start()
    {
        Invoke("LaunchTest", 0.5f);
    }

    void LaunchTest()
    {
        dialogueManager.Write(testNodeID);
    }
}