using UnityEngine;

public class StatsManager : MonoBehaviour {
    public int currentParagon;
    public int currentRenegade;

    void OnEnable() {
        GameEvents.OnStatsChanged += UpdateStats;
    }

    void UpdateStats(int p, int r) {
        currentParagon += p;
        currentRenegade += r;
    }
}