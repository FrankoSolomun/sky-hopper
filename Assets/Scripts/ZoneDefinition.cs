using UnityEngine;

[CreateAssetMenu(fileName = "ZoneDefinition", menuName = "Zones/ZoneDefinition")]
public class ZoneDefinition : ScriptableObject
{
    public string zoneName;
    public GameObject[] zoneBackgrounds;
    public GameObject[] zonePlatformPrefabs;
    public int startScore;
    public int endScore;
}