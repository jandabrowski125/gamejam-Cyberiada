using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitGame : MonoBehaviour
{
    [SerializeField] private FadingEffects _fadingEffects;

    public void QuitGame()
    {
        StartCoroutine(QuitGameRoutine());
    }

    public void QuitToMainMenu()
    {
        StartCoroutine(QuitToMainMenuRoutine());
    }

    private IEnumerator QuitGameRoutine()
    {
        Time.timeScale = 1f;
        PauseMenu.isPaused = false;
        yield return _fadingEffects.WaitForFadeOut(2f, fadeMusicSource: true);
        Application.Quit();
    }

    private IEnumerator QuitToMainMenuRoutine()
    {
        Time.timeScale = 1f;
        PauseMenu.isPaused = false;
        yield return _fadingEffects.WaitForFadeOut(1f, fadeMusicSource: true);
        SceneManager.LoadScene("Menu");
    }
}
