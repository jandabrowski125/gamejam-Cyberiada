using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class GoToNextSceneOnVideoEnd : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public int nextSceneIndex; // set this in Inspector

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextSceneIndex);
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}
