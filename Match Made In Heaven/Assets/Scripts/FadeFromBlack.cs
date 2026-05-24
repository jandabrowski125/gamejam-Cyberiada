using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FadeFromBlack : MonoBehaviour
{
    public AudioSource musicSource;
    public Image fadeImage;

    public float fadeDuration;

    void Start()
    {
        musicSource.volume = 0f;
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float startVolume = musicSource.volume;
        float time = 0f;

        Color color = fadeImage.color;

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;

            musicSource.volume = Mathf.Lerp(0f, 1f, t);

            color.a = Mathf.Lerp(1f, 0f, t);
            fadeImage.color = color;

            time += Time.deltaTime;
            yield return null;
        }  
    }
}
