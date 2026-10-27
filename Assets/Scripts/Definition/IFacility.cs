using R3;
public interface IFacility
{
    string Id{get;}
    FacilityType Type{get;}
    int MaxDurability{get;}
    ReadOnlyReactiveProperty<int> CurrentDurability{get;}

    void ApplyOccupation(int dmg, PlayerSide attackSide);

}