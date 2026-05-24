using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.VisualScripting;

public class DialogueWriter : MonoBehaviour
{
    [Header("References")]
    public DialogueLoader loader;
    public GameObject textPrefab;
    [SerializeField] private Transform _dialogueBoxParent; 
    [SerializeField] private Transform _sideBoxParent;
    public GameObject dialogueUI;
    public TextAsset _jsonWordFile;

    [Header("Fonts")]
    public TMP_FontAsset fontAsset;
    public TMP_FontAsset alienFontAsset;

    [Header("Typing Settings")]
    public int charsPerLine = 24;
    [SerializeField] private int _charsPerLineSideBox = 20;
    public float typingSpeed = 0.025f;
    public Color keywordHighlightColor = Color.cyan;
    [SerializeField] private Color _normalTextColor;

    [Header("Animation Settings")]
    public float animationDuration = 0.5f;
    public float offScreenX = -10; 
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("UI Rect")]
    public RectTransform speechPanelRect;

    private bool forceSkipTyping = false;
    private HashSet<string> knownWords = new HashSet<string>();
    private int currentColumn = 0;
    private int _currentColumnContextBox;
    private bool _isTypingKeywordContext = false;
    private bool _isTypingPresenterContext = false;
    private bool _wroteFullText = false;
    private List<GameObject> _keywordSentence = new List<GameObject>();
    private string _keywordContext;
    private string _contextSentece = "The presenter said something. I have no idea what it is though...";
    
    private Coroutine sequenceCoroutine;
    private Coroutine typingCoroutine;
    
    private RectTransform panelRect;
    private CanvasGroup canvasGroup;
    private Vector2 targetAnchoredPosition;

    void Awake()
    {
        PrepareDictionary();

        if (dialogueUI == null)
        {
            return;
        }

        if (canvasGroup == null) canvasGroup = dialogueUI.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = dialogueUI.GetComponentInChildren<CanvasGroup>();
        
        if (panelRect == null) panelRect = dialogueUI.GetComponent<RectTransform>();

        if (canvasGroup == null || panelRect == null)
        {
            return;
        }

        targetAnchoredPosition = panelRect.anchoredPosition;
        HideInstantly();
    }

    public void AddKnownWord(string word)
    {
        if (!string.IsNullOrEmpty(word))
            knownWords.Add(word.ToLower().Trim());
    }

    // --- NOWA METODA: Bezpieczne ubijanie wszystkich procesów pisania ---
    private void StopAllDialogue()
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        
        sequenceCoroutine = null;
        typingCoroutine = null;
    }

    private void HideInstantly()
    {
        canvasGroup.alpha = 0;
        panelRect.anchoredPosition = new Vector2(offScreenX, targetAnchoredPosition.y);
        dialogueUI.SetActive(false);
    }

    public void CompleteInstantly()
    {
        forceSkipTyping = true;
    }

    public void Hide()
    {
        StopAllDialogue();
        HideInstantly(); 
    }

    public void Write(string nodeId, string keyword = null, bool forceSkip = false, bool forceUnderstandable = false, string toLearn = null)
    {
        DialogueNode node = loader.GetNode(nodeId);
        if (node == null) return;
        forceSkipTyping = false;

        dialogueUI.SetActive(true);
        StopAllDialogue();
        ClearGrid();

        bool skipTypewriter = !string.IsNullOrEmpty(keyword) || forceSkip;
        sequenceCoroutine = StartCoroutine(DialogueSequence(node.text_original, keyword, skipTypewriter, forceUnderstandable, toLearn));
    }

    public void WriteEnding(string text)
    {
        bool forceUnderstandable = true;

        dialogueUI.SetActive(true);
        StopAllDialogue();
        ClearGrid();

        sequenceCoroutine = StartCoroutine(DialogueSequence(text, null, false, forceUnderstandable));
    }

    public void WriteRaw(string text, string speaker, string keyword = null, bool forceSkip = false, bool forceUnderstandable = false, string toLearn = null)
    {
        dialogueUI.SetActive(true);
        StopAllDialogue();
        ClearGrid();
        forceSkipTyping = false;

        bool skipTypewriter = !string.IsNullOrEmpty(keyword) || forceSkip;
        sequenceCoroutine = StartCoroutine(DialogueSequence(text, keyword, skipTypewriter, forceUnderstandable, toLearn));
    }

    private IEnumerator DialogueSequence(string text, string keyword, bool skipTypewriter, bool forceUnderstandable, string toLearn = null)
    {
        float elapsedTime = 0;
        Vector2 startPos = new Vector2(offScreenX, targetAnchoredPosition.y);

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            float curveT = slideCurve.Evaluate(t);

            panelRect.anchoredPosition = Vector2.LerpUnclamped(startPos, targetAnchoredPosition, curveT);
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);

            yield return null;
        }

        panelRect.anchoredPosition = targetAnchoredPosition;
        canvasGroup.alpha = 1;


        // ZMIANA: Zapisujemy zagnieżdżoną korutynę do zmiennej, żeby móc ją zabić!
        typingCoroutine = StartCoroutine(TypeTextRoutine(text, keyword, skipTypewriter, forceUnderstandable, toLearn));
        yield return typingCoroutine;
    }

    private IEnumerator TypeTextRoutine(
        string fullText,
        string keyword,
        bool skipTypewriter,
        bool forceUnderstandable,
        string toLearn = null
    )
    {
        _wroteFullText = false;
        _isTypingPresenterContext = false;
        _isTypingKeywordContext = false;
        string presenterContext = "";
        string keywordContext = "";
        string[] words = fullText.Split(' ');
        _keywordSentence.Clear();
        currentColumn = 0;
        
        foreach (string word in words)
        {   
            if (string.IsNullOrEmpty(word)) continue;
            if (IsSpecialChar(word)) continue;
            
            if (_isTypingPresenterContext) presenterContext += " " + word;
            else if (_isTypingKeywordContext) keywordContext += " " + word;
            
            string clean = CleanWord(word);
            string cleanLower = clean.ToLower();

            bool isKeyword = (keyword != null && cleanLower == keyword.ToLower());
            bool isToLearn = (toLearn != null && cleanLower == toLearn.ToLower());
            bool isUnderstandable = forceUnderstandable || isKeyword || knownWords.Contains(cleanLower);

            TMP_FontAsset fontToUse = isUnderstandable ? fontAsset : alienFontAsset;

            Color textColor = isToLearn ? keywordHighlightColor : _normalTextColor;
            

            if (word.Length > (charsPerLine - currentColumn))
            {
                if (currentColumn != 0)
                {
                    int spacesToFill = charsPerLine - currentColumn;
                    for (int i = 0; i < spacesToFill; i++) {
                        GameObject obj = CreateText(' ', fontAsset, _normalTextColor);
                        // //Add only one space
                        // if (i == 0 && _isTypingKeyword) _keywordSentence.Add(obj);
                    }
                    currentColumn = 0;
                }
            }

            for (int i = 0; i < word.Length; i++)
            {
                GameObject obj = CreateText(word[i], fontToUse, textColor);
                if (_isTypingKeywordContext) _keywordSentence.Add(obj);
                currentColumn++;
                if (currentColumn >= charsPerLine) currentColumn = 0;
                
                if (!skipTypewriter && !forceSkipTyping)
                {
                    float timer = 0f;
                    while (timer < typingSpeed)
                    {
                        if (forceSkipTyping) break; 
                        
                        timer += Time.deltaTime;
                        yield return null;
                    }
                }
            }

            if (currentColumn > 0 && currentColumn < charsPerLine)
            {
                GameObject obj = CreateText(' ', fontAsset, _normalTextColor);
                if (_isTypingKeywordContext) _keywordSentence.Add(obj);
                currentColumn++;
                if (currentColumn >= charsPerLine) currentColumn = 0;
            }

            if (!skipTypewriter && !forceSkipTyping)
            {
                float timer = 0f;
                while (timer < typingSpeed)
                {
                    if (forceSkipTyping) break; 
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
        }
        
        _wroteFullText = true;
        forceSkipTyping = false;
        if (!String.IsNullOrEmpty(presenterContext)) _contextSentece = presenterContext;
        if (!String.IsNullOrEmpty(keywordContext)) _keywordContext = keywordContext;
    }

    private bool IsSpecialChar(string c)
    {
        if (c == "{") {
            _isTypingKeywordContext = true;
            return true;
        }
        else if (c == "}") {
            _isTypingKeywordContext = false;
            return true;
        }
        else if (c == "&") {
            _isTypingPresenterContext = !_isTypingPresenterContext;
            if (_isTypingPresenterContext) Debug.Log("Typing!");
            else Debug.Log("Not typing!");
            return true;
        }
        return false;
    }

    private GameObject CreateText(char letter, TMP_FontAsset font, Color color, bool isSideBox = false)
    {
        GameObject obj = isSideBox ? Instantiate(textPrefab, _sideBoxParent) : Instantiate(textPrefab, _dialogueBoxParent);
        TextMeshProUGUI textComp = obj.GetComponent<TextMeshProUGUI>();
        textComp.text = letter.ToString();
        textComp.font = font;
        textComp.color = color;
        if (isSideBox) obj.transform.localScale = new Vector3(0.8f, 0.8f);
        return obj;
    }

    private void ClearGrid() 
    { 
        foreach (Transform child in _dialogueBoxParent) Destroy(child.gameObject); 
    }

    private void PrepareDictionary()
    {
        if (_jsonWordFile != null)
        {
            WordDataWrapper data = JsonUtility.FromJson<WordDataWrapper>(_jsonWordFile.text);

            if (data != null && data.words != null)
            {
                foreach (var w in data.words)
                {
                    knownWords.Add(w.Trim().ToLower());
                }
            }
        }
    }

    private string CleanWord(string word)
    {
        char[] punctuations = { ',', '.', '?', '!' };
        string clean = word;
        if (clean.Length > 0 && punctuations.Contains(clean[clean.Length - 1]))
            clean = clean.Substring(0, clean.Length - 1);
        return clean;
    }

    public List<GameObject> GetKeywordSentence()
    {
        return _keywordSentence;
    }

    public string GetContextSentence()
    {
        if (_wroteFullText)
        {
            return _contextSentece;
        }
        return "The presenter said something. I should've listened to him...";
    }
    public bool WroteFullText()
    {
        return _wroteFullText;
    }

    public void TypeKeywordContext(
        string keyword
    )
    {
        string[] words = _keywordContext.Split(' ');
        _currentColumnContextBox = 0;
        
        foreach (string word in words)
        {   
            if (string.IsNullOrEmpty(word)) continue;
            if (IsSpecialChar(word)) continue;
            
            string clean = CleanWord(word);
            string cleanLower = clean.ToLower();

            bool isKeyword = keyword != null && cleanLower == keyword.ToLower();
            bool isUnderstandable = knownWords.Contains(cleanLower);

            TMP_FontAsset fontToUse = isUnderstandable ? fontAsset : alienFontAsset;

            Color textColor = isKeyword ? keywordHighlightColor : _normalTextColor;
            

            if (word.Length > (_charsPerLineSideBox - _currentColumnContextBox))
            {
                if (_currentColumnContextBox != 0)
                {
                    int spacesToFill = _charsPerLineSideBox - _currentColumnContextBox;
                    for (int i = 0; i < spacesToFill; i++) {
                        CreateText(' ', fontAsset, _normalTextColor, isSideBox: true);
                    }
                    _currentColumnContextBox = 0;
                }
            }

            for (int i = 0; i < word.Length; i++)
            {
                CreateText(word[i], fontToUse, textColor, isSideBox: true);
                _currentColumnContextBox++;
                if (_currentColumnContextBox >= _charsPerLineSideBox) _currentColumnContextBox = 0;   
            }

            if (_currentColumnContextBox > 0 && _currentColumnContextBox < _charsPerLineSideBox)
            {
                CreateText(' ', fontAsset, _normalTextColor, isSideBox: true);
                _currentColumnContextBox++;
                if (_currentColumnContextBox >= _charsPerLineSideBox) _currentColumnContextBox = 0;
            }
        }
    }
}