using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject popup;
    [SerializeField] private FadingEffects _fadingEffect;

    void Start()
    {
        _fadingEffect.RequestFadeIn();
    }
    public void Play()
    {
        _fadingEffect.RequestFadeOut(
            fadeDuration: 1f,
            fadeMusicSource: true,
            forceSynchronous: true
        );

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Exit() 
    {
        Application.Quit();
    }
}



