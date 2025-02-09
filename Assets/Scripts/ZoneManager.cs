using UnityEngine;

public class ZoneManager : MonoBehaviour
{
    public ZoneDefinition[] zones;
    private ZoneDefinition currentZone;
    private ZoneDefinition lastReportedZone;

    void Start()
    {
        // Ensure we pick a valid zone at startup
        currentZone = GetZoneForScore(Move.platformsJumped);
        lastReportedZone = currentZone;
    }

    void Update()
    {
        int playerScore = Move.platformsJumped;
        currentZone = GetZoneForScore(playerScore);

        if (currentZone != lastReportedZone)
        {
            lastReportedZone = currentZone;
        }
    }

    private ZoneDefinition GetZoneForScore(int score)
    {
        foreach (var zone in zones)
        {
            if (score >= zone.startScore && score < zone.endScore)
            {
                return zone;
            }
        }
        return zones[zones.Length - 1];
    }

    public ZoneDefinition GetCurrentZone()
    {
        return currentZone;
    }
}