using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuButton : MonoBehaviour
{
    public GameObject popup;
    public AudioSource musicSource;
    public Image fadeImage;

    public float fadeDuration = 2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void LoadimgScene()
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

        SceneManager.LoadScene("Intro");
    }

    

    public void LoadingPopUp()
    {
        Debug.Log("KURWAMAĆ");
        Debug.Log("Credits");

    }

    public void doExitGame() 
    {
        Application.Quit();
        Debug.Log("Game is exiting");
    }
}



