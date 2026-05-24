using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ButtonCreator : MonoBehaviour
{
    [SerializeField] private GameObject _buttonPrefab;
    [SerializeField] private Transform _parentPanel;

    /// <summary>
    /// Deletes all buttons <see langword="from"/> the scene. 
    /// </summary>
    public void ClearButtons()
    {
        foreach (Transform child in _parentPanel) Destroy(child.gameObject);
    }

    /// <summary>
    /// Creates one button <see langword="with"/> specific <paramref name="text"/> <see langword="and"/> <paramref name="onClickAction"/>.  
    /// </summary>
    /// <param name="text">Text displayed <see langword="on"/> the button. </param>
    /// <param name="onClickAction">Action performed <see langword="if"/> the button <see langword="is"/> clicked.</param>
    private void CreateButton(string text, Action onClickAction)
    {
        GameObject newButton = Instantiate(_buttonPrefab, _parentPanel);
        
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
        GameObject newButton = Instantiate(_buttonPrefab, _parentPanel);
        newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = label;
        
        Button btn = newButton.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClickAction.Invoke());
    }
}