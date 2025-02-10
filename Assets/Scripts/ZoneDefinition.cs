using UnityEngine;

[CreateAssetMenu(fileName = "ZoneDefinition", menuName = "Zones/ZoneDefinition")]
public class ZoneDefinition : ScriptableObject
{
    public string zoneName;
    public GameObject[] zoneBackgrounds;

    public GameObject[] zonePlatformPrefabs;
    public GameObject[] zoneSpecialPlatformPrefabs;
    public int startScore;
    public int endScore;
}