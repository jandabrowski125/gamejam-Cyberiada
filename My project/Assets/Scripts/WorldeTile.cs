using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WordleTile : MonoBehaviour
{
    public TextMeshProUGUI textElement;
    public Image backgroundImage;

    public void SetLetter(char letter) => textElement.text = letter.ToString();
    public void SetColor(Color color) => backgroundImage.color = color;
}