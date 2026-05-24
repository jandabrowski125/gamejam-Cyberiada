using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FadeToBlack : MonoBehaviour
{
    public AudioSource musicSource;
    public Image fadeImage;

    public float fadeDuration = 2f;

    public void StartFadeOut()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float startVolume = musicSource.volume;
        float time = 0f;

        Color color = fadeImage.color;

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;

            musicSource.volume = Mathf.Lerp(startVolume, 0f, t);

            color.a = Mathf.Lerp(0f, 1f, t);
            fadeImage.color = color;

            time += Time.deltaTime;
            yield return null;
        }
        musicSource.volume = 0f;
        musicSource.Stop();

        color.a = 1f;
        fadeImage.color = color;   
        
    }
}



