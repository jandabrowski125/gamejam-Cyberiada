using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class EndingManager : MonoBehaviour
{
    [Header("References")]
    public TextAsset endingsJson;
    public CharacterDatabase characterDB;
    public EndingDialogueManager endingDialogueManager;

    [Header("UI Layout")]
    public GameObject charButtonPrefab;
    public Transform buttonsParent;
    public GameObject selectionCanvas; 

    private EndingList endingList;
    private Dictionary<string, int> playerStats;
    private int remainingCharacters = 0; // Licznik przycisków

    void Awake()
    {
        endingList = JsonUtility.FromJson<EndingList>(endingsJson.text);
    }

    public void StartEndingPhase(Dictionary<string, int> stats)
    {
        playerStats = stats;
        selectionCanvas.SetActive(true);
        GenerateCharacterButtons();
    }

    private void GenerateCharacterButtons()
    {
        foreach (Transform child in buttonsParent) Destroy(child.gameObject);

        if (endingList == null || endingList.endings == null) return;

        remainingCharacters = endingList.endings.Count; // Ustawiamy licznik na start

        foreach (var data in endingList.endings)
        {
            GameObject btnObj = Instantiate(charButtonPrefab, buttonsParent);

            Image portrait = btnObj.GetComponent<Image>(); 
            if (portrait != null && characterDB != null) portrait.sprite = characterDB.GetSprite(data.name);

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                string charName = data.name;
                GameObject buttonToDestroy = btnObj; // Kopia referencji dla lambdy

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    // Przekazujemy referencję do przycisku, ale jeszcze go NIE NISZCZYMY
                    OnCharacterSelected(charName, buttonToDestroy); 
                });
            }
        }
    }

    // ZMIANA: Metoda przyjmuje teraz również przycisk, w który kliknęliśmy
    private void OnCharacterSelected(string characterName, GameObject buttonClicked)
    {
        EndingData data = endingList.endings.First(e => e.name == characterName);
        
        string statKey = characterName + "_paragon";
        playerStats.TryGetValue(statKey, out int score);
        int totalParagon = playerStats.Values.Where((v, i) => playerStats.Keys.ElementAt(i).EndsWith("_paragon")).Sum();

        bool success = score >= data.paragon_requirements && totalParagon >= data.overall_paragon_requirement;

        // --- KLUCZOWA ZMIANA ---
        // Niszczymy przycisk TYLKO wtedy, gdy postać nas odrzuca (!success)
        if (!success)
        {
            remainingCharacters--; 
            Destroy(buttonClicked); 
        }

        selectionCanvas.SetActive(false);
        endingDialogueManager.StartEndingDialogue(data, success, endingList.credits);
    }

    public void ReturnToSelection()
    {
        if (remainingCharacters <= 0)
        {
            // Odrzucili nas wszyscy - włączamy globalny smutny koniec
            endingDialogueManager.StartUnhappyEnding(endingList.unhappy_ending, endingList.credits);
        }
        else
        {
            // Wciąż ktoś został (albo ktoś nas zaakceptował, ale wróciliśmy z okna dialogowego), pokazujemy menu
            selectionCanvas.SetActive(true);
        }
    }
}