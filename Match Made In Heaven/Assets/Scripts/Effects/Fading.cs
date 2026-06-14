using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FadingEffects : MonoBehaviour
{
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private Image _fadeImage;
    [SerializeField] private GameObject _fadeCanvas;
    
    public void RequestFadeIn(float fadeDuration = 2f, bool fadeMusicSource = true, bool forceSynchronous = false)
    {
        StartCoroutine(FadeIn(fadeDuration, fadeMusicSource));
    }

    public void RequestFadeOut(float fadeDuration = 2f, bool fadeMusicSource = true, bool forceSynchronous = false)
    {
        StartCoroutine(FadeOut(fadeDuration, fadeMusicSource));
    }

    public IEnumerator WaitForFadeOut(float fadeDuration = 2f, bool fadeMusicSource = true)
    {
        yield return FadeOut(fadeDuration, fadeMusicSource);
    }

    void Awake()
    {
        EnsureBlackScreen();
    }

    private void EnsureBlackScreen()
    {
        _fadeCanvas.SetActive(true);
        Color color = _fadeImage.color;
        color.a = 1f;
        _fadeImage.color = color;
    }

    private IEnumerator FadeIn(float fadeDuration, bool fadeMusicSource)
    {
        EnsureBlackScreen();
        if (fadeMusicSource) _musicSource.volume = 0f;
        float time = 0f;

        Color color = _fadeImage.color;

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;

            if (fadeMusicSource) _musicSource.volume = Mathf.Lerp(0f, 1f, t);

            color.a = Mathf.Lerp(1f, 0f, t);
            _fadeImage.color = color;

            time += Time.unscaledDeltaTime;
            yield return null;
        }  

        _fadeCanvas.SetActive(false);
    }

    private IEnumerator FadeOut(float fadeDuration, bool fadeMusicSource)
    {
        _fadeCanvas.SetActive(true);
        float startVolume = _musicSource.volume;
        float time = 0f;

        Color color = _fadeImage.color;

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;

            if (fadeMusicSource) _musicSource.volume = Mathf.Lerp(startVolume, 0f, t);

            color.a = Mathf.Lerp(0f, 1f, t);
            _fadeImage.color = color;

            time += Time.unscaledDeltaTime;
            yield return null;
        }
        if (fadeMusicSource)
        {
            _musicSource.volume = 0f;
            _musicSource.Stop(); 
        } 

        color.a = 1f;
        _fadeImage.color = color;
        
    }
}