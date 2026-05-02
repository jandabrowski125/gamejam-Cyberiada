using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueWriter _dialogueWriter;
    [SerializeField] private DialogueLoader _loader;
    [SerializeField] private ButtonCreator _buttonCreator;
    [SerializeField] private EndingManager _endingManager;
    [SerializeField] private StatsManager _statsManager;

    [Header("Character System (Sprite2D)")]
    [SerializeField] private CharacterDatabase _characterDB;
    [SerializeField] private SpriteRenderer _characterPortraitRenderer;
    [SerializeField] private SpriteRenderer _backgroundImageRenderer;

    [Header("Audio")]
    [SerializeField] private AudioSource _backgroundAudioSource;
    private string lastNodeId;

    //Event subscriptions
    private void OnEnable()
    {
        GameEvents.OnWordleRequired += HandleWordleRequested;
        GameEvents.OnWordleSuccess += HandleMinigameResult;
    }

    private void OnDisable()
    {
        GameEvents.OnWordleRequired -= HandleWordleRequested;
        GameEvents.OnWordleSuccess -= HandleMinigameResult;
    }

    //Check if any refereces are null.
    private void Start()
    {
        HelperFunctions.IsAnyNull(
            "DialogueManager - References",
            _dialogueWriter,
            _loader,
            _buttonCreator,
            _endingManager,
            _statsManager
        );

        HelperFunctions.IsAnyNull(
            "DialogueManager - Audio",
            _backgroundAudioSource
        );

        HelperFunctions.IsAnyNull(
            "DialogueManager - Sprites",
            _characterDB,
            _characterPortraitRenderer,
            _backgroundImageRenderer
        );
    }

    /// <summary>
    /// Orders the DialogueWriter to write dialogue specified <see langword="in"/> node of <paramref name="nodeId"/>.
    /// </summary>
    /// <param name="nodeId"></param>
    public void Write(string nodeId)
    {
        lastNodeId = nodeId;
        DialogueNode node = _loader.GetNode(nodeId);

        if (node != null)
        {
            UpdatePortrait(node.speaker);

            if (!_backgroundAudioSource.isPlaying) _backgroundAudioSource.Play();

            bool isPresenter = node.speaker.Equals("Presenter", System.StringComparison.OrdinalIgnoreCase);

            //if presenter speaks, forceUnderstandable = true
            _dialogueWriter.Write(
                nodeId,
                forceUnderstandable: isPresenter, 
                toLearn: node.wordle_solution
                );
            _characterDB.PlayVoice(node.speaker);

            if (isPresenter)
            {
                if (node.choices.Count == 0)
                {
                    _buttonCreator.ShowContinue(() => {
                        if (!string.IsNullOrEmpty(node.next_node)) Write(node.next_node);
                        else StartEnding();
                    });
                    return;
                }
                _buttonCreator.ShowChoices(node, (choice) => HandleChoice(choice, node));
            }
            else
            {
                _buttonCreator.ShowContinue(() => StartWordleChallenge());
            }
        }
    }

    /// <summary>
    /// Completes dialogue instantly. Called <see langword="by"/> an onClick <see langword="method"/>. 
    /// </summary>
    public void CompleteDialogueInstantly()
    {
        _dialogueWriter.CompleteInstantly();
    }

    /// <summary>
    /// Cleans up the space and starts the ending phase of the game.
    /// </summary>
    private void StartEnding()
    {
        _backgroundAudioSource.Stop();
        _dialogueWriter.Hide();
        _buttonCreator.ClearButtons();
        _endingManager.StartEndingPhase(_statsManager.GetFinalStats());
    }

    /// <summary>
    /// Updates the character rendered <see langword="as"/> the speaker, based <see langword="on"/> the <paramref name="speakerName"/> parameter. 
    /// </summary>
    /// <param name="speakerName">Name of the speaker</param>
    private void UpdatePortrait(string speakerName)
    {
        Sprite speakerSprite = _characterDB.GetSprite(speakerName);
        Sprite backgroundSprite = _characterDB.GetBackground(speakerName);

        if (speakerSprite != null && backgroundSprite != null)
        {
            _characterPortraitRenderer.sprite = speakerSprite; 
            _characterPortraitRenderer.gameObject.SetActive(true);
            _characterPortraitRenderer.color = Color.white;

            _backgroundImageRenderer.sprite = backgroundSprite;
            _backgroundImageRenderer.gameObject.SetActive(true);
            _backgroundImageRenderer.color = Color.white;
        }
        else
        {
            _characterPortraitRenderer.gameObject.SetActive(false);
            _backgroundImageRenderer.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Requests the Wordle minigame to be started. \n
    /// </summary>
    /// <remarks>
    /// Retrieves the solution word <see langword="from"/> the previously displayed dialogue node. 
    /// </remarks>
    private void StartWordleChallenge()
    {
        DialogueNode node = _loader.GetNode(lastNodeId);
        if (node != null && !string.IsNullOrEmpty(node.wordle_solution))
        {
            GameEvents.TriggerWordleRequired(node.wordle_solution);
        }
    }

    /// <summary>
    /// Handles the result of the minigame.
    /// </summary>
    /// <remarks>
    /// If <paramref name="foundKeyword"/> <see langword="is"/> empty <see langword="or"/> <see langword="null"/>, evaluates to a loss, else win.
    /// </remarks>
    /// <param name="foundKeyword">Found word</param>
    private void HandleMinigameResult(string foundKeyword)
    {
        if (string.IsNullOrEmpty(lastNodeId)) return;
        DialogueNode node = _loader.GetNode(lastNodeId);

        _backgroundAudioSource.UnPause();

        if (!string.IsNullOrEmpty(foundKeyword))
        {
            _dialogueWriter.AddKnownWord(foundKeyword);
            _dialogueWriter.Write(
                lastNodeId,
                keyword: foundKeyword,
                forceUnderstandable: true,
                toLearn: node.wordle_solution
                );
        }
        else
        {
            _dialogueWriter.Write(
                lastNodeId,
                keyword: null,
                forceSkip: true
                );
        }

        _buttonCreator.ShowChoices(node, (choice) => HandleChoice(choice, node));
    }

    /// <summary>
    /// Handles the user's choice.
    /// </summary>
    /// <param name="choice">Chosen answer</param>
    /// <param name="currentNode">Current dialogue node</param>
    private void HandleChoice(DialogueChoice choice, DialogueNode currentNode)
    {
        GameEvents.TriggerStatsChanged(currentNode.speaker, choice.plus_paragon, choice.plus_renegade);

        bool isPresenter = currentNode.speaker.Equals("Presenter", System.StringComparison.OrdinalIgnoreCase);

        if (!string.IsNullOrEmpty(choice.follow_up))
        {
            //to sie chyba nigdy nie odpala xd ale moze sie przydac kiedys jak bedziemy mogli odpowiadac przybyszowi
            _dialogueWriter.WriteRaw(
                choice.follow_up, 
                currentNode.speaker, 
                forceUnderstandable: isPresenter
                );
            _characterDB.PlayVoice(currentNode.speaker);

            _buttonCreator.ShowContinue(() => {
                if (!string.IsNullOrEmpty(currentNode.next_node)) Write(currentNode.next_node);
                else StartEnding();
            });
        }
        else
        {
            if (!string.IsNullOrEmpty(currentNode.next_node)) Write(currentNode.next_node);
            else StartEnding();
        }
    }

    //nie wiem po co ta funkcja w ogole
    public void WriteEnding(string text, string speakerName)
    {
        // 1. Najpierw ustawiamy wizualia (Postać + Tło)
        UpdatePortrait(speakerName);

        // 2. Potem każemy Writerowi wypisać tekst finałowy
        _dialogueWriter.WriteEnding(text);
        
        // 3. Opcjonalnie: odpal głos (jeśli masz przygotowane klipy pod finał)
        _characterDB.PlayVoice(speakerName);
    }

    /// <summary>
    /// Handler of the GameEvents.OnWordleRequired <see langword="event"/>. Opens the Wordle minigame.
    /// </summary>
    /// <param name="solution">Keyword <see langword="for"/> Wordle.</param>
    private void HandleWordleRequested(string solution)
    {
        if (_backgroundAudioSource.isPlaying) _backgroundAudioSource.Pause();
        _dialogueWriter.Hide();
        _buttonCreator.ClearButtons();
    }
}