using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour
{
    public GameObject popup;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void LoadimgScene()
    {
        Debug.Log("loadload");
        SceneManager.LoadScene("Assets/Scenes/SampleScene.unity");
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



