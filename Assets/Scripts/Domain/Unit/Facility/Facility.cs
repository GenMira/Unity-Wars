using System;
using R3;

public class Facility : IFacility
{
    public string Id{get;private set;}
    public FacilityType Type{get;private set;}
    public HexPosition Position{get;private set;}
    public int MaxDurability{get;private set;}
    public PlayerSide Owner{get;private set;}

    public ReadOnlyReactiveProperty<int> CurrentDurability => _currentDurability;
    private readonly ReactiveProperty<int> _currentDurability;

    public Facility(
        FacilityType type,
        HexPosition position,
        int durability,
        PlayerSide owner
    )
    {
        Type = type;
        Id = NameSolver.FacilityName(Type);
        MaxDurability = durability;
        _currentDurability = new ReactiveProperty<int>(durability);
        Owner = owner;
        Position = position;
    }

    public void ApplyOccupation(int dmg, PlayerSide attackSide)
    {
        if (dmg < 0)
            throw new ArgumentOutOfRangeException(nameof(dmg));

        if (_currentDurability.Value > dmg)
        {
            _currentDurability.Value -= dmg;
        }
        else
        {
            _currentDurability.Value = 0;
            ChangeOwner(attackSide);
        }

    }

    private void ChangeOwner(PlayerSide newOwner)
    {
        Owner = newOwner;
    }

}
