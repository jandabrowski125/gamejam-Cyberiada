using UnityEngine;

public class GameInit : MonoBehaviour
{
    [Header("Key dependencies")]
    [SerializeField] private FadingEffects _fadingEffects;
    public DialogueManager dialogueManager;
    public string testNodeID = "node_001";
    public float delayBeforeWordle = 3.0f;

    void Start()
    {   
        Invoke("LaunchTest", 0.1f);
    }

    void LaunchTest()
    {
        dialogueManager.Write(testNodeID);
        _fadingEffects.RequestFadeIn(
            fadeDuration: 2f,
            fadeMusicSource: false,
            forceSynchronous: false
        );
    }
}