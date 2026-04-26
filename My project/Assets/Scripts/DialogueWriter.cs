using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DialogueWriter : MonoBehaviour
{
    [Header("References")]
    public DialogueLoader loader;
    public GameObject textPrefab;
    public Transform parent; 
    public GameObject dialogueUI;
    public TextAsset wordFile;

    [Header("Fonts")]
    public TMP_FontAsset fontAsset;
    public TMP_FontAsset alienFontAsset;

    [Header("Typing Settings")]
    public int charsPerLine = 24;
    public float typingSpeed = 0.05f;
    public Color keywordHighlightColor = Color.cyan;

    [Header("Animation Settings")]
    public float animationDuration = 0.5f;
    public float offScreenX = -10; 
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("UI Rect")]
    public RectTransform speechPanelRect;

    private HashSet<string> knownWords = new HashSet<string>();
    private int currentColumn = 0;
    
    // ZMIANA: Śledzimy OBA procesy oddzielnie
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
            Debug.LogError("<color=red>DialogueWriter: Przypisz 'Dialogue UI' w Inspektorze!</color>");
            return;
        }

        if (canvasGroup == null) canvasGroup = dialogueUI.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = dialogueUI.GetComponentInChildren<CanvasGroup>();
        
        if (panelRect == null) panelRect = dialogueUI.GetComponent<RectTransform>();

        if (canvasGroup == null || panelRect == null)
        {
            Debug.LogError($"<color=yellow>DialogueWriter: Brakuje komponentów na {dialogueUI.name}. Sprawdź CanvasGroup i RectTransform!</color>");
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

    public void Hide()
    {
        StopAllDialogue();
        HideInstantly(); 
    }

    public void Write(string nodeId, string keyword = null, bool forceSkip = false, bool forceUnderstandable = false)
    {
        DialogueNode node = loader.GetNode(nodeId);
        if (node == null) return;

        dialogueUI.SetActive(true);
        StopAllDialogue(); // Zatrzymujemy stare dialogi
        ClearGrid();

        bool skipTypewriter = !string.IsNullOrEmpty(keyword) || forceSkip;
        sequenceCoroutine = StartCoroutine(DialogueSequence(node.text_original, keyword, skipTypewriter, forceUnderstandable));
    }

    public void WriteEnding(string text)
    {
        bool forceUnderstandable = true;
        bool skipTypewriter = true;

        dialogueUI.SetActive(true);
        StopAllDialogue(); // Zatrzymujemy stare dialogi
        ClearGrid();

        sequenceCoroutine = StartCoroutine(DialogueSequence(text, null, skipTypewriter, forceUnderstandable));
    }

    public void WriteRaw(string text, string speaker, string keyword = null, bool forceSkip = false, bool forceUnderstandable = false)
    {
        dialogueUI.SetActive(true);
        StopAllDialogue(); // Zatrzymujemy stare dialogi
        ClearGrid();

        bool skipTypewriter = !string.IsNullOrEmpty(keyword) || forceSkip;
        sequenceCoroutine = StartCoroutine(DialogueSequence(text, keyword, skipTypewriter, forceUnderstandable));
    }

    private IEnumerator DialogueSequence(string text, string keyword, bool skipTypewriter, bool forceUnderstandable)
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
        typingCoroutine = StartCoroutine(TypeTextRoutine(text, keyword, skipTypewriter, forceUnderstandable));
        yield return typingCoroutine;
    }

    private IEnumerator TypeTextRoutine(string fullText, string keyword, bool skipTypewriter, bool forceUnderstandable)
    {
        string[] words = fullText.Split(' ');
        currentColumn = 0;

        foreach (string word in words)
        {
            if (string.IsNullOrEmpty(word)) continue;

            string clean = CleanWord(word);
            string cleanLower = clean.ToLower();
            
            bool isKeyword = (keyword != null && cleanLower == keyword.ToLower());
            bool isUnderstandable = forceUnderstandable || isKeyword || knownWords.Contains(cleanLower);

            TMP_FontAsset fontToUse = isUnderstandable ? fontAsset : alienFontAsset;
            Color textColor = isKeyword ? keywordHighlightColor : Color.white;

            if (word.Length > (charsPerLine - currentColumn))
            {
                if (currentColumn != 0)
                {
                    int spacesToFill = charsPerLine - currentColumn;
                    for (int i = 0; i < spacesToFill; i++) CreateText(' ', fontAsset, Color.white);
                    currentColumn = 0;
                }
            }

            for (int i = 0; i < word.Length; i++)
            {
                CreateText(word[i], fontToUse, textColor);
                currentColumn++;
                if (currentColumn >= charsPerLine) currentColumn = 0;
                
                if (!skipTypewriter) yield return new WaitForSeconds(typingSpeed);
            }

            if (currentColumn > 0 && currentColumn < charsPerLine)
            {
                CreateText(' ', fontAsset, Color.white);
                currentColumn++;
                if (currentColumn >= charsPerLine) currentColumn = 0;
            }

            if (!skipTypewriter) yield return new WaitForSeconds(typingSpeed);
        }
    }

    void CreateText(char letter, TMP_FontAsset font, Color color)
    {
        GameObject obj = Instantiate(textPrefab, parent);
        TextMeshProUGUI textComp = obj.GetComponent<TextMeshProUGUI>();
        textComp.text = letter.ToString();
        textComp.font = font;
        textComp.color = color;
    }

    private void ClearGrid() 
    { 
        foreach (Transform child in parent) Destroy(child.gameObject); 
    }

    private void PrepareDictionary()
    {
        if (wordFile != null)
        {
            var words = wordFile.text
                .Replace(",", " ")
                .Split(new char[] { ' ', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            
            foreach(var w in words) knownWords.Add(w.Trim().ToLower());
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
}