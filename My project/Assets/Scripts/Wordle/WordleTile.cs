using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WordleTile : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textElement;
    [SerializeField] private Image backgroundImage;

    public void SetLetter(
        char letter,
        Color color,
        TMP_FontAsset font = null, 
        int fontSize = 0
        )
    {
        textElement.text = letter.ToString();
        textElement.color = color;
        if (font != null) textElement.font = font;
        if (fontSize > 0) textElement.fontSize = fontSize;
    }
    public void SetBackgroundColor(Color color) => backgroundImage.color = color;
    public string GetLetter() {
        return textElement.text;
    }
}