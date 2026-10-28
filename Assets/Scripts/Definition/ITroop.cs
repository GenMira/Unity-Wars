using R3;

public interface ITroop
{
    string Id{get;}
    TroopType Type{get;}
    PlayerSide Owner{get;}

    int MaxHealth{get;}
    ReadOnlyReactiveProperty<int>  CurrentHealth{get;}

    int MaxMovementRange{get;}
    TroopType[] MountableType{get;}
    int MountableAmount{get;}
    bool IsAlreadyMoved{get;}

    void Reset();
    int Attack(int type);
    int GetAttackRange(int type);
    void RecieveDamage(int damage);

}
