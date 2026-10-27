using UnityEngine;
using R3;

public interface ITroop
{
    string Id{get;}
    TroopType Type{get;}
    PlayerSide Owner{get;}

    int MaxHealth{get;}
    ReadOnlyReactiveProperty<int>  CurrentHealth{get;}

    int MaxMovementRange{get;}
    int CurrentMovementRange{get;}
    int Cost{get;}
    TroopType[] MountableType{get;}
    int MountableAmount{get;}
    bool IsAlreadyMoved{get;}

    bool CheckMovability();
    void Reset();
    int Attack(int type);
    int GetAttackRange(int type);
    void recieveDamage(int damage);

}