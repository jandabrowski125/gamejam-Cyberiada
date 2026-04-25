using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ButtonCreator : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform parentPanel;

    // Czyści wszystkie przyciski
    public void ClearButtons()
    {
        foreach (Transform child in parentPanel)
        {
            Destroy(child.gameObject);
        }
    }

    // Tworzy pojedynczy przycisk (Helper)
    private void CreateButton(string text, Action onClickAction)
    {
        GameObject newButton = Instantiate(buttonPrefab, parentPanel);
        
        TextMeshProUGUI textComponent = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null) textComponent.text = text;

        Button btn = newButton.GetComponent<Button>();
        btn.onClick.AddListener(() => onClickAction.Invoke());
    }

    // Specjalny przycisk "Kontynuuj"
    public void ShowContinue(Action onContinueClick)
    {
        ClearButtons();
        CreateButton("Continue...", onContinueClick);
    }

    // Przyciski wyborów z logiką Paragon/Renegade
    public void ShowChoices(DialogueNode node, Action<DialogueChoice> onChoiceSelected)
    {
        ClearButtons();
        foreach (var choice in node.choices)
        {
            CreateButton(choice.text, () => onChoiceSelected.Invoke(choice));
        }
    }

    public void ShowContinueCustom(string label, System.Action onClickAction)
    {
        GameObject newButton = Instantiate(buttonPrefab, parentPanel);
        newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = label;
        
        Button btn = newButton.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClickAction.Invoke());
    }
}