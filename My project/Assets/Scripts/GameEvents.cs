using System;

public static class GameEvents
{
    public static Action<string> OnWordleRequired;
    public static Action<string> OnWordleSuccess; // Zmieniamy int na string
    // --- WYDARZENIA DIALOGÓW ---

    // Wywoływane, gdy zmienia się węzeł dialogu
    // Możesz tu przekazać cały obiekt Twojego Node'a
    public static Action<string, string> OnDialogueNodeChanged; // Speaker, Text


    // --- WYDARZENIA STATYSTYK ---

    // Wywoływane po kliknięciu odpowiedzi Paragon/Renegade
    public static Action<int, int> OnStatsChanged; // Paragon change, Renegade change

    // Wywoływane, gdy poziom zrozumienia (Understanding) rośnie
    public static Action<int> OnUnderstandingUpdated; // Nowa wartość UP


    // --- POMOCNICZE METODY DO WYWOŁYWANIA (Triggering) ---

    public static void TriggerWordleRequired(string word) => OnWordleRequired?.Invoke(word);
    
    public static void TriggerWordleSuccess(string word) => OnWordleSuccess?.Invoke(word);

    public static void TriggerStatsChanged(int p, int r) => OnStatsChanged?.Invoke(p, r);
}