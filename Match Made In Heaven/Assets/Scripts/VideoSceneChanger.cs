using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(VideoPlayer))]
public class VideoSceneChanger : MonoBehaviour
{
    [SerializeField] private FadingEffects _fadingEffects;
    private VideoPlayer videoPlayer;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }
        else
        {
            Debug.LogError("Brak komponentu VideoPlayer na obiekcie: " + gameObject.name);
        }

        _fadingEffects.RequestFadeIn(
            fadeDuration: 2, 
            fadeMusicSource: true,
            forceSynchronous: false
        );
    }

    public void OnVideoEnd(VideoPlayer vp)
    {
        _fadingEffects.RequestFadeOut(
            fadeDuration: 1f,
            forceSynchronous: true
        );
        
        Invoke("NextScene", 2f);
    }
    void OnDestroy()
    {
        // Dobra praktyka: odpinamy event, gdy obiekt jest niszczony, aby uniknąć błędów
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }

    private void NextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}