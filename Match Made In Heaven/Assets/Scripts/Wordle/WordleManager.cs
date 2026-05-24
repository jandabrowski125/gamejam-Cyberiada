using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;

public class WordleManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject rowPrefab;
    public GameObject tilePrefab;
    public Transform container;
    public Transform wordContainer;
    public AudioSource wordleInputSound;
    
    [Header("Colors - Tiles")]
    public Color correctColor;
    public Color presentColor;
    public Color absentColor;
    public Color targetWordTileColor;
    public Color emptyTileColor;

    [Header("Colors - Letters")]
    [SerializeField] private Color _targetWordLetterColor;

    [Header("Effects")]
    public GameObject successParticlesPrefab;

    [Header("Dictionary")]
    [SerializeField] private TextAsset _dictionaryTextFile;
    private HashSet<string> _validWords = new HashSet<string>();

    private string targetWord;
    private int currentAttempt = 0;
    private string currentInput = "";
    private List<WordleTile[]> rows = new List<WordleTile[]>();
    private bool isProcessing = false;

    [Header("Audio Settings")]
    public AudioSource wordleSuccess;
    public AudioSource wordleFailed;

    [Header("Alien font")]
    public TMP_FontAsset alienFont;

    [SerializeField] private int _availableAttempts = 5;

    public AudioSource audienceAudio;

    private void Awake()
    {
        ParseDictionary();
    }

    // --- SUBSKRYPCJE ---
    private void OnEnable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput += HandleTextInput;
    }

    private void OnDisable()
    {        
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= HandleTextInput;
    }
     
    private void ParseDictionary()
    {
        if (_dictionaryTextFile != null)
        {
            string[] lines = _dictionaryTextFile.text.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string line in lines)
            {
                _validWords.Add(line.Trim().ToUpper());
            }
            
            Debug.Log($"WordleManager: Wczytano {_validWords.Count} słów do słownika.");
        }
        else
        {
            Debug.LogError("WordleManager: Brak pliku słownika w Inspektorze!");
        }
    }

    private void HandleTextInput(char character)
    {
        if (isProcessing || string.IsNullOrEmpty(targetWord) || currentInput.Length >= targetWord.Length) 
            return;
        
        if (char.IsLetter(character) && !PauseMenu.isPaused)
        {
            currentInput += char.ToUpper(character);
            wordleInputSound.Play();
            UpdateUI();
        }
    }

    void Update()
    {
        if (string.IsNullOrEmpty(targetWord) || isProcessing) return;

        // Enter i Backspace sprawdzamy tradycyjnie w Update
        if (Keyboard.current.backspaceKey.wasPressedThisFrame && currentInput.Length > 0)
        {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
            UpdateUI();
        }
        
        if (Keyboard.current.enterKey.wasPressedThisFrame && currentInput.Length == targetWord.Length)
        {
            StartCoroutine(CheckWordRoutine());
        }
    }

    /// <summary>
    /// Initializes the Wordle minigame with the word keyword. 
    /// </summary>
    /// <param name="word">Solution of the minigame</param>
    public void InitWordle(string word)
    {
        if (container == null || rowPrefab == null || tilePrefab == null)
        {
            Debug.LogError("WordleManager: Brakuje referencji w Inspektorze!");
            return;
        }

        foreach (Transform child in container) Destroy(child.gameObject);
        foreach (Transform child in wordContainer) Destroy(child.gameObject);
        rows.Clear();
        currentAttempt = 0;
        currentInput = "";
        
        targetWord = word.ToUpper();

        var wordObj = Instantiate(rowPrefab, wordContainer);
        WordleTile[] wordRow = new WordleTile[targetWord.Length];
        for (int i = 0; i < targetWord.Length; i++)
        {
            GameObject tileObj = Instantiate(tilePrefab, wordObj.transform);
            wordRow[i] = tileObj.GetComponent<WordleTile>();
            wordRow[i].SetLetter(
                targetWord[i],
                _targetWordLetterColor,
                font: alienFont,
                fontSize: 70
                );
            wordRow[i].SetBackgroundColor(targetWordTileColor);
        }

        for (int i = 0; i < _availableAttempts; i++) {
            var rowObj = Instantiate(rowPrefab, container);
            WordleTile[] tileRow = new WordleTile[targetWord.Length];
            for (int j = 0; j < targetWord.Length; j++) {
                GameObject tileObj = Instantiate(tilePrefab, rowObj.transform);
                tileRow[j] = tileObj.GetComponent<WordleTile>();
                
                tileRow[j].SetLetter(' ', Color.white);
                tileRow[j].SetBackgroundColor(emptyTileColor);
            }
            rows.Add(tileRow);
        }
    }

    void UpdateUI()
    {
        if (currentAttempt >= rows.Count) return;

        var currentTiles = rows[currentAttempt];
        for (int i = 0; i < currentTiles.Length; i++)
        {
            currentTiles[i].SetLetter(i < currentInput.Length ? currentInput[i] : ' ', Color.white);
            
        }
    }
    
    public void SkipWordle()
    {
        targetWord = "";
        audienceAudio.Play();
        isProcessing = false;
        wordleFailed.Play();
        GameEvents.TriggerWordleSuccess(string.Empty);
    }

    private IEnumerator CheckWordRoutine()
    {
        isProcessing = true;
        
        // currentInput jest już zbudowane z wielkich liter, 
        // więc pasuje do elementów w HashSet
        if (!_validWords.Contains(currentInput))
        {
            Debug.Log($"Słowo '{currentInput}' nie istnieje w słowniku!");
            currentInput = "";
            UpdateUI();
            isProcessing = false;
            audienceAudio.Play();
            yield break;
        }

        ColorizeCurrentRow();

        if (currentInput == targetWord)
        {
            wordleSuccess.Play();
            yield return new WaitForSeconds(1f);
            
            GameEvents.TriggerWordleSuccess(targetWord);
            targetWord = ""; 
        }
        else
        {
            currentAttempt++;
            currentInput = "";
            if (currentAttempt >= _availableAttempts)
            {
                wordleFailed.Play();
                yield return new WaitForSeconds(1f);                
                GameEvents.TriggerWordleSuccess(string.Empty);
                targetWord = "";

                audienceAudio.Play();
            }
        }
        isProcessing = false;
    }

    private void ColorizeCurrentRow()
    {
        WordleTile[] currentTiles = rows[currentAttempt];
        char[] targetChars = targetWord.ToCharArray();
        char[] inputChars = currentInput.ToCharArray();
        
        bool[] matched = new bool[targetChars.Length];

        for (int i = 0; i < inputChars.Length; i++)
        {
            if (inputChars[i] == targetChars[i])
            {
                currentTiles[i].SetBackgroundColor(correctColor);
                matched[i] = true;
            }
        }

        for (int i = 0; i < inputChars.Length; i++)
        {
            if (matched[i]) continue;

            bool foundYellow = false;
            for (int j = 0; j < targetChars.Length; j++)
            {
                if (!matched[j] && inputChars[i] == targetChars[j])
                {
                    currentTiles[i].SetBackgroundColor(presentColor);
                    foundYellow = true;
                    break;
                }
            }

            if (!foundYellow)
                currentTiles[i].SetBackgroundColor(absentColor);
        }
    }
}