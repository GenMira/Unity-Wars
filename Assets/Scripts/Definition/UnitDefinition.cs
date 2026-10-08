using UnityEngine;

[CreateAssetMenu(menuName = "Unity Wars/Unit Definition")]
public class UnitDefinition : ScriptableObject
{
    [SerializeField] private Unit id;
    [SerializeField] private string displayName;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackPower = 20;
    [SerializeField] private int movementRange = 3;
    [SerializeField] private int cost = 1000;
    [SerializeField] private Unit[] mountableType;
    [SerializeField] private int mountableAmount;



    public Unit Id => id;
    public string DisplayName => displayName;
    public int MaxHealth => maxHealth;
    public int AttackPower => attackPower;
    public int MovementRange => movementRange;
    public int Cost => cost;
    public Unit[] MountableType => mountableType;
    public int MountableAmount => mountableAmount;
}