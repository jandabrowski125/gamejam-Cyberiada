using UnityEngine;
using System.Collections.Generic;

public class StatsManager : MonoBehaviour 
{
    private Dictionary<string, int> playerStats = new Dictionary<string, int>();

    void OnEnable() {
        GameEvents.OnStatsChanged += UpdateStats;
    }

    void OnDisable() {
        GameEvents.OnStatsChanged -= UpdateStats;
    }

    void UpdateStats(string characterName, int p, int r) {
        if (p != 0)
        {
            UpdateParagon(characterName, p);
            return;
        }
        else if (r != 0)
        {
            UpdateRenegade(characterName, r);
            return;
        }
    }

    private void UpdateParagon(string characterName, int p)
    {
        if (string.IsNullOrEmpty(characterName + "_paragon")) return;

        if (playerStats.ContainsKey(characterName + "_paragon")) {
            playerStats[characterName + "_paragon"] += p;
        } else {
            playerStats.Add(characterName + "_paragon", p);
        }
    }

    private void UpdateRenegade(string characterName, int r)
    {
        if (string.IsNullOrEmpty(characterName + "_renegade")) return;

        if (playerStats.ContainsKey(characterName + "_renegade")) {
            playerStats[characterName + "_renegade"] += r;
        } else {
            playerStats.Add(characterName + "_renegade", r);
        }
    }

    public Dictionary<string, int> GetFinalStats() {
        return playerStats;
    }
}