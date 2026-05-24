using System;

public static class GameEvents
{
    public static Action<string> OnWordleRequired;
    public static Action<string> OnWordleSuccess;
    public static Action<string, string> OnDialogueNodeChanged; // Speaker, Text


    // --- WYDARZENIA STATYSTYK ---

    public static Action<string, int, int> OnStatsChanged; // Paragon change, Renegade change

    // Wywoływane, gdy poziom zrozumienia (Understanding) rośnie
    public static Action<int> OnUnderstandingUpdated;


    // --- POMOCNICZE METODY DO WYWOŁYWANIA (Triggering) ---

    public static void TriggerWordleRequired(string word) => OnWordleRequired?.Invoke(word);
    
    public static void TriggerWordleSuccess(string word) => OnWordleSuccess?.Invoke(word);

    public static void TriggerStatsChanged(string characterName, int p, int r) => OnStatsChanged?.Invoke(characterName, p, r);
}