using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject popup;
    [SerializeField] private FadingEffects _fadingEffect;

    void OnEnable()
    {
        _fadingEffect.RequestFadeIn();
    }

    public void Play()
    {
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        yield return _fadingEffect.WaitForFadeOut(1f, fadeMusicSource: true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Exit() 
    {
        Application.Quit();
    }
}



