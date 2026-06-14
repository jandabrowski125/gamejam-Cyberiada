using System.Collections.Generic;
using UnityEngine;

public class WordleProgressTracker : MonoBehaviour
{
    private const string PresenterCharacterName = "Presenter";

    [SerializeField] private DialogueLoader _dialogueLoader;

    private readonly HashSet<string> _solvedWordles = new(System.StringComparer.OrdinalIgnoreCase);

    private void OnEnable()
    {
        GameEvents.OnWordleSuccess += OnWordleSolved;
    }

    private void OnDisable()
    {
        GameEvents.OnWordleSuccess -= OnWordleSolved;
    }

    private void OnWordleSolved(string word)
    {
        if (!string.IsNullOrEmpty(word))
            _solvedWordles.Add(word);
    }

    public bool HasSolvedAllWordles()
    {
        return _dialogueLoader != null && _dialogueLoader.AreAllWordlesSolved(_solvedWordles);
    }

    public static string GetPresenterCharacterName() => PresenterCharacterName;
}
