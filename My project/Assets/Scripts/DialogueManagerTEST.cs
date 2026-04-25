using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DialogueManager : MonoBehaviour 
{
    [Header("UI References")]
    public TextMeshProUGUI uiText;
    public GameObject choiceContainer;
    public GameObject choiceButtonPrefab;
    public WordleManager wordleManager;

    [Header("Dictionary Settings")]
    // Słowa, które gracz zna na starcie ("uszkodzony tłumacz")
    public List<string> startingWords = new List<string> { "I", "YOU", "IS", "THE", "ARE" };
    private HashSet<string> knownWords = new HashSet<string>();

    private const string ALIEN_CHARS = "⏃⏁⟒⋏⟒☌⏃⟊⍙⟒⍀⏁⊬⎐⏚⋔"; 

    private DialogueNode currentNode;
    private bool isWaitingForWordle = false;

    void Awake()
    {
        // Inicjalizacja słownika słowami startowymi
        foreach (string w in startingWords) knownWords.Add(w.ToUpper());
    }

    // ... (Metody DisplayNode i StartWordleChallenge pozostają bez zmian) ...

    void OnEnable() {
        GameEvents.OnWordleSuccess += HandleWordleWin;
    }

    void OnDisable() {
        GameEvents.OnWordleSuccess -= HandleWordleWin;
    }

    void HandleWordleWin(string solvedWord) 
    {
        isWaitingForWordle = false;
        wordleManager.gameObject.SetActive(false);
        
        // Dodajemy nowe słowo do zestawu znanych słów
        knownWords.Add(solvedWord.ToUpper());
        
        UpdateTextDisplay();
        // ShowActionButtons(); 
    }

    // --- NOWY SYSTEM TŁUMACZENIA ---

    private void UpdateTextDisplay()
    {
        if (currentNode != null)
            uiText.text = TranslateText(currentNode.text_original);
    }

    private string TranslateText(string original)
    {
        // Rozbijamy zdanie na słowa, zachowując znaki interpunkcyjne
        // Regex \b pozwala wyłapać granice słów
        string[] tokens = Regex.Split(original, @"(\W+)");
        string result = "";

        foreach (string token in tokens)
        {
            // Jeśli token to słowo (nie znaki interpunkcyjne)
            if (Regex.IsMatch(token, @"\w+"))
            {
                string upperToken = token.ToUpper();

                if (knownWords.Contains(upperToken))
                {
                    // Gracz zna to słowo - wyświetlamy normalnie
                    result += token;
                }
                else
                {
                    // Gracz nie zna - zamieniamy na "robaczki" o tej samej długości
                    result += GenerateAlienWord(token.Length);
                }
            }
            else
            {
                // To jest spacja lub znak interpunkcyjny - zostawiamy
                result += token;
            }
        }
        return result;
    }

    private string GenerateAlienWord(int length)
    {
        char[] alien = new char[length];
        for (int i = 0; i < length; i++)
        {
            alien[i] = ALIEN_CHARS[Random.Range(0, ALIEN_CHARS.Length)];
        }
        return new string(alien);
    }

    // ... (Reszta skryptu: ShowActionButtons i OnChoiceSelected bez zmian) ...
}