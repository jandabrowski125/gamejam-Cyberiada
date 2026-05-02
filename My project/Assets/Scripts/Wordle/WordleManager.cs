using UnityEngine;
using UnityEngine.Networking;
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
    public GameObject backgroundImagePrefab;
    public AudioSource wordleInputSound;
    
    [Header("Colors")]
    public Color correctColor = Color.green;
    public Color presentColor = Color.yellow;
    public Color absentColor = Color.gray;
    public Color targetWordTileColor;
    public Color emptyTileColor = new Color(1f, 1f, 1f, 0.2f);

    [Header("Effects")]
    public GameObject successParticlesPrefab;

    private string targetWord;
    private int currentAttempt = 0;
    private string currentInput = "";
    private List<WordleTile[]> rows = new List<WordleTile[]>();
    private bool isProcessing = false;

    [Header("Animation Settings")]
    public float animationDuration = 0.5f;
    public float startXPosition = -10; // Pozycja startowa (poza ekranem z lewej)
    public float endXPosition = 20;    // Ucieczka w prawo
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Krzywa dla płynności

    [Header("Audio Settings")]
    public AudioClip wordleMusic;
    public float maxVolume = 0.5f;
    public float audioFadeDuration = 0.8f;
    public AudioSource wordleSuccess;
    public AudioSource wordleFailed;

    [Header("Alien font")]
    public TMP_FontAsset alienFont;

    private AudioSource audioSource;
    private Coroutine audioFadeCoroutine;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 targetAnchoredPosition;

    [SerializeField] private int _availableAttempts = 5;

    public AudioSource audienceAudio;
    public AudioSource audienceClapAudio;

    // --- SUBSKRYPCJE ---
    private void OnEnable()
    {
        GameEvents.OnWordleRequired += StartWordleWithAnimation;
        if (Keyboard.current != null)
            Keyboard.current.onTextInput += HandleTextInput;
    }

    private void OnDisable()
    {
        GameEvents.OnWordleRequired -= StartWordleWithAnimation;
        
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= HandleTextInput;
    }


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        audioSource.clip = wordleMusic;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0;
        targetAnchoredPosition = rectTransform.anchoredPosition; // Zapisujemy gdzie ma docelowo być
    }

    private IEnumerator FadeAudio(float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;
        float time = 0;

        // Jeśli włączamy muzykę, musimy ją najpierw odpalić
        if (targetVolume > 0 && !audioSource.isPlaying) audioSource.Play();

        while (time < duration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;

        if (targetVolume <= 0) audioSource.Stop();
    }

    private void StartFade(float target)
    {
        if (audioFadeCoroutine != null) StopCoroutine(audioFadeCoroutine);
        audioFadeCoroutine = StartCoroutine(FadeAudio(target, audioFadeDuration));
    }

    private void HandleTextInput(char character)
    {
        // Blokujemy wpisywanie, jeśli procesujemy słowo, nie ma gry lub rząd jest pełny
        if (isProcessing || string.IsNullOrEmpty(targetWord) || currentInput.Length >= targetWord.Length) 
            return;
        
        if (char.IsLetter(character))
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

    private void StartWordleWithAnimation(string word)
    {
        StopAllCoroutines();
        
        StartFade(maxVolume);
        
        InitWordle(word);
        StartCoroutine(SlideInRoutine());

        audienceClapAudio.Play();
    }

    private IEnumerator SlideInRoutine()
    {
        float elapsedTime = 0;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        // Ustawienia startowe: poza ekranem i niewidoczne
        rectTransform.anchoredPosition = new Vector2(startXPosition, targetAnchoredPosition.y);
        canvasGroup.alpha = 0;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            float curveT = slideCurve.Evaluate(t); // Używamy krzywej dla efektu "smooth"

            // Przesunięcie
            rectTransform.anchoredPosition = Vector2.Lerp(
                new Vector2(startXPosition, targetAnchoredPosition.y), 
                targetAnchoredPosition, 
                curveT
            );

            // Fade in
            canvasGroup.alpha = Mathf.Lerp(0, 1, curveT);

            yield return null;
        }

        // Upewniamy się, że skończyło w idealnym miejscu
        rectTransform.anchoredPosition = targetAnchoredPosition;
        canvasGroup.alpha = 1;
    }

    /// <summary>
    /// Initializes the Wordle minigame <see langword="with"/> the word keyword. 
    /// </summary>
    /// <param name="word">Solution of the minigame</param>
    public void InitWordle(string word)
    {
        if (container == null || rowPrefab == null || tilePrefab == null)
        {
            Debug.LogError("WordleManager: Brakuje referencji w Inspektorze!");
            return;
        }

        // Czyszczenie poprzedniej gry
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
                font: alienFont,
                fontSize: 70
                );
            wordRow[i].SetColor(targetWordTileColor);
        }

        for (int i = 0; i < _availableAttempts; i++) {
            var rowObj = Instantiate(rowPrefab, container);
            WordleTile[] tileRow = new WordleTile[targetWord.Length];
            for (int j = 0; j < targetWord.Length; j++) {
                GameObject tileObj = Instantiate(tilePrefab, rowObj.transform);
                tileRow[j] = tileObj.GetComponent<WordleTile>();
                
                tileRow[j].SetLetter(' ');
                tileRow[j].SetColor(emptyTileColor);
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
            currentTiles[i].SetLetter(i < currentInput.Length ? currentInput[i] : ' ');
            
        }
    }
    public void SkipWordle()
    {
        GameEvents.TriggerWordleSuccess(string.Empty);
        targetWord = "";
        audienceAudio.Play();
        isProcessing = false;
        wordleFailed.Play();
        StartCoroutine(SlideOutRoutine());
    }

    private IEnumerator CheckWordRoutine()
    {
        isProcessing = true;
        string wordToCheck = currentInput.ToLower();

        using (UnityWebRequest www = UnityWebRequest.Get($"https://api.dictionaryapi.dev/api/v2/entries/en/{wordToCheck}"))
        {
            yield return www.SendWebRequest();

            bool exists = www.result == UnityWebRequest.Result.Success;

            if (!exists)
            {
                Debug.Log("Słowo nie istnieje w słowniku!");
                currentInput = "";
                UpdateUI();
                isProcessing = false;
                audienceAudio.Play();
                yield break;
                
            }
        }

        ColorizeCurrentRow();

        if (currentInput == targetWord)
        {
            wordleSuccess.Play();
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(SlideOutRoutine());
            
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
                yield return StartCoroutine(SlideOutRoutine());
                
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
        // Debug.Log("target: \n");
        // foreach (char c in targetChars) Debug.Log(c);
        // Debug.Log("attempt: \n");
        // foreach (char c in inputChars) Debug.Log(c);
        bool[] matched = new bool[targetChars.Length];

        for (int i = 0; i < inputChars.Length; i++)
        {
            if (inputChars[i] == targetChars[i])
            {
                currentTiles[i].SetColor(correctColor);
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
                    currentTiles[i].SetColor(presentColor);
                    foundYellow = true;
                    break;
                }
            }

            if (!foundYellow)
                currentTiles[i].SetColor(absentColor);
        }
    }

    private IEnumerator SlideOutRoutine()
    {
        StartFade(0f);
        
        float elapsedTime = 0;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        Vector2 currentPos = rectTransform.anchoredPosition;
        
        // Wyłączamy interakcję na początku animacji wyjazdu
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            float curveT = slideCurve.Evaluate(t);

            // Interpolacja do endXPosition (ucieczka w prawo)
            rectTransform.anchoredPosition = Vector2.Lerp(
                currentPos, 
                new Vector2(endXPosition, targetAnchoredPosition.y), 
                curveT
            );

            // Fade out
            canvasGroup.alpha = Mathf.Lerp(1, 0, curveT);

            yield return null;
        }

        canvasGroup.alpha = 0;
        rectTransform.anchoredPosition = new Vector2(startXPosition, targetAnchoredPosition.y);
    }
}