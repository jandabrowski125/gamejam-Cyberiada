using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem.Controls;

public class WordleTile : MonoBehaviour
{
    private TextMeshProUGUI textElement;
    private Image backgroundImage;

    public void SetLetter(
        char letter,
        TMP_FontAsset font = null, 
        int fontSize = 0
        )
    {
        textElement.text = letter.ToString();
        if (font != null) textElement.font = font;
        if (fontSize > 0) textElement.fontSize = fontSize;
    }
    public void SetColor(Color color) => backgroundImage.color = color;
    public string GetLetter() {
        return textElement.text;
    }
}