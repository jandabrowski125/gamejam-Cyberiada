using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(VideoPlayer))]
public class VideoSceneChanger : MonoBehaviour
{
    [Header("Ustawienia")]
    [Tooltip("Wpisz dokładną nazwę sceny, do której chcesz przejść")]
    public string nextSceneName;

    private VideoPlayer videoPlayer;

    void Awake()
    {
        // Pobieramy komponent VideoPlayer podpięty do tego samego obiektu
        videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            // Subskrybujemy event wywoływany, gdy wideo dojdzie do końca
            videoPlayer.loopPointReached += OnVideoEnd;
        }
        else
        {
            Debug.LogError("Brak komponentu VideoPlayer na obiekcie: " + gameObject.name);
        }
    }

    // Metoda wywoływana automatycznie przez event
    private void OnVideoEnd(VideoPlayer vp)
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("Wideo zakończone. Ładowanie sceny: " + nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Nazwa następnej sceny jest pusta! Wpisz ją w Inspektorze.");
        }
    }

    void OnDestroy()
    {
        // Dobra praktyka: odpinamy event, gdy obiekt jest niszczony, aby uniknąć błędów
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}