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
        if (forceSynchronous) SynchronousFadeIn(fadeDuration, fadeMusicSource);
        else StartCoroutine(FadeIn(fadeDuration, fadeMusicSource));
    }

    public void RequestFadeOut(float fadeDuration = 2f, bool fadeMusicSource = true, bool forceSynchronous = false)
    {
        if (forceSynchronous) SynchronousFadeOut(fadeDuration, fadeMusicSource);
        else StartCoroutine(FadeOut(fadeDuration, fadeMusicSource));
    }
    
    private Coroutine SynchronousFadeIn(float fadeDuration = 2f, bool fadeMusicSource = true)
    {
        return StartCoroutine(FadeIn(fadeDuration, fadeMusicSource));
    }

    private Coroutine SynchronousFadeOut(float fadeDuration = 2f, bool fadeMusicSource = true)
    {
        return StartCoroutine(FadeOut(fadeDuration, fadeMusicSource));
    }
    private IEnumerator FadeIn(float fadeDuration, bool fadeMusicSource)
    {
        if (fadeMusicSource) _musicSource.volume = 0f;
        float time = 0f;

        Color color = _fadeImage.color;

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;

            if (fadeMusicSource) _musicSource.volume = Mathf.Lerp(0f, 1f, t);

            color.a = Mathf.Lerp(1f, 0f, t);
            _fadeImage.color = color;

            time += Time.deltaTime;
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

            time += Time.deltaTime;
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