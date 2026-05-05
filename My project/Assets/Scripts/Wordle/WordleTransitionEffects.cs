using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI; // Wymagane dla LayoutRebuilder

public class WordleTransitionEffects : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private WordleManager _wordleManager;
    [SerializeField] private DialogueWriter _dialogueWriter;

    [Header("UI References")]
    [SerializeField] private RectTransform _wordleRect;
    [SerializeField] private CanvasGroup _wordleCanvasGroup;
    [SerializeField] private RectTransform _sideBoxRect;
    [SerializeField] private CanvasGroup _sideBoxCanvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float _animationDuration = 0.5f;
    [SerializeField] private float _delayBeforeSideBox = 4.0f;
    [SerializeField] private float _startXPosition = -10f;
    [SerializeField] private float _endXPosition = 20f;
    [SerializeField] private AnimationCurve _slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio")]
    [SerializeField] private float _maxVolume = 0.5f;
    [SerializeField] private float _audioFadeDuration = 0.8f;
    [SerializeField] private AudioSource _wordleMusic;
    [SerializeField] private AudioSource _audienceClapAudio;
    [SerializeField] private AudioMixerGroup _mixerGroup;


    private Coroutine _audioFadeCoroutine;

    private Vector2 _wordleTargetPos;
    private Vector2 _sideBoxTargetPos;

    private void OnEnable()
    {
        GameEvents.OnWordleRequired += OnWordleRequiredHandler;
        GameEvents.OnWordleSuccess += OnWordleSuccessHandler;
    }

    private void OnDisable()
    {
        GameEvents.OnWordleRequired -= OnWordleRequiredHandler;
        GameEvents.OnWordleSuccess -= OnWordleSuccessHandler;
    }

    private void Awake()
    {
        InitializePanels();
    }

    private void Start()
    {
        HelperFunctions.IsAnyNull(
            "WordleTransitionEffects - Dependencies",
            _dialogueWriter,
            _wordleManager
        );

        HelperFunctions.IsAnyNull(
            "WordleTransitionEffects - UI",
            _sideBoxRect,
            _sideBoxCanvasGroup,
            _wordleRect,
            _wordleCanvasGroup
        );
    }


    private void InitializePanels()
    {
        _wordleTargetPos = _wordleRect.anchoredPosition;
        
        ResetPanel(_wordleCanvasGroup);

        _sideBoxTargetPos = _sideBoxRect.anchoredPosition;
        ResetPanel(_sideBoxCanvasGroup);
    }

    private void ResetPanel(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void OnWordleRequiredHandler(string word)
    {
        StopAllCoroutines();
        StartFadeAudio(_maxVolume);
        
        _wordleManager.InitWordle(word);
        _audienceClapAudio.Play();
        
        StartCoroutine(FullIntroSequence());
    }

    private void OnWordleSuccessHandler(string word)
    {
        StopAllCoroutines();
        StartCoroutine(SlideOutSequence());
    }

    private IEnumerator FullIntroSequence()
    {
        _wordleRect.anchoredPosition = new Vector2(_startXPosition, _wordleTargetPos.y);
        yield return StartCoroutine(AnimatePanel(_wordleRect, _wordleCanvasGroup, _wordleTargetPos, 1f, true));

        yield return new WaitForSeconds(_delayBeforeSideBox);

        if (_dialogueWriter.WroteFullText())
        {
            List<GameObject> keywordLetters = _dialogueWriter.GetKeywordSentence();
            foreach (GameObject letter in keywordLetters)
            {
                letter.transform.localScale = new Vector3(0.8f, 0.8f);
                letter.transform.SetParent(_sideBoxRect, false);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_sideBoxRect);

            _sideBoxRect.anchoredPosition = new Vector2(_startXPosition, _sideBoxTargetPos.y);
            yield return StartCoroutine(AnimatePanel(_sideBoxRect, _sideBoxCanvasGroup, _sideBoxTargetPos, 1f, false));
        }
    }

    private IEnumerator SlideOutSequence()
    {
        StartFadeAudio(0f);

        Vector2 wordleEndPos = new Vector2(_endXPosition, _wordleTargetPos.y);
        Coroutine wordleOut = StartCoroutine(AnimatePanel(_wordleRect, _wordleCanvasGroup, wordleEndPos, 0f, false));

        if (_dialogueWriter.WroteFullText())
        {
            Vector2 sideBoxEndPos = new Vector2(_endXPosition, _sideBoxTargetPos.y);
            StartCoroutine(HideSideBox(_sideBoxRect, _sideBoxCanvasGroup, sideBoxEndPos, 0f, false));
        }

        yield return wordleOut;
    }

    private IEnumerator HideSideBox(RectTransform rect, CanvasGroup canvasGroup, Vector2 targetPos, float targetAlpha, bool enableInteraction)
    {
        StartCoroutine(AnimatePanel(rect, canvasGroup, targetPos, targetAlpha, enableInteraction));
        yield return new WaitForSeconds(2);
        foreach (Transform child in rect) Destroy(child.gameObject); 
    }

    /// <summary>
    /// Uniwersalna metoda do animacji RectTransform i CanvasGroup
    /// </summary>
    private IEnumerator AnimatePanel(RectTransform rect, CanvasGroup canvasGroup, Vector2 targetPos, float targetAlpha, bool enableInteraction)
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float elapsedTime = 0;
        Vector2 startPos = rect.anchoredPosition;
        float startAlpha = canvasGroup.alpha;

        while (elapsedTime < _animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = _slideCurve.Evaluate(elapsedTime / _animationDuration);

            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            yield return null;
        }

        rect.anchoredPosition = targetPos;
        canvasGroup.alpha = targetAlpha;

        canvasGroup.interactable = enableInteraction;
        canvasGroup.blocksRaycasts = enableInteraction;
    }

    private void StartFadeAudio(float target)
    {
        if (_audioFadeCoroutine != null) StopCoroutine(_audioFadeCoroutine);
        _audioFadeCoroutine = StartCoroutine(FadeAudio(target, _audioFadeDuration));
    }

    private IEnumerator FadeAudio(float targetVolume, float duration)
    {
        float startVolume = _wordleMusic.volume;
        float time = 0;

        if (targetVolume > 0 && !_wordleMusic.isPlaying) _wordleMusic.Play();

        while (time < duration)
        {
            time += Time.deltaTime;
            _wordleMusic.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        _wordleMusic.volume = targetVolume;
        if (targetVolume <= 0) _wordleMusic.Stop();
    }
}