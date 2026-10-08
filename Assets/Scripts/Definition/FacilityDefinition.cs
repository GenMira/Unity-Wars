using UnityEngine;

[CreateAssetMenu(menuName = "Unity Wars/Facility Definition")]
public class FacilityDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private int maxDurability = 200;

    public string Id => id;
    public string DisplayName => displayName;
    public int MaxDurability => maxDurability;
}