using UnityEngine;
using UnityEngine.UI;

public class ButtonCreator : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform parentPanel;
    private List DialogueOptions;

    void Start()
    {
        
        
        CreateButton("Button 1", OnButton1Clicked);
        CreateButton("Button 2", OnButton2Clicked);
        CreateButton("Button 3", OnButton3Clicked);
    }

    void CreateButton(string buttonText, UnityEngine.Events.UnityAction action)
    {
        // Instantiate button
        GameObject newButton = Instantiate(buttonPrefab, parentPanel);

        // Set button text
        Text textComponent = newButton.GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            textComponent.text = buttonText;
        }

        // Add click listener
        Button btn = newButton.GetComponent<Button>();
        btn.onClick.AddListener(action);
    }

    // Functions for buttons
    void OnButton1Clicked()
    {
        Debug.Log("Button 1 clicked!");
    }

    void OnButton2Clicked()
    {
        Debug.Log("Button 2 clicked!");
    }

    void OnButton3Clicked()
    {
        Debug.Log("Button 3 clicked!");
    }
}
