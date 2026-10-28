using System;
using System.Collections.Generic;

public class Solider : Troop
{
    private Dictionary<int,int> attackvalueDictionary = new Dictionary<int,int>(){
        {0, 10},
        {1, 20},
        {2, 30}
    };
    private Dictionary<int,int> attackRangeDictionary = new Dictionary<int,int>(){
        {0, 1},
        {1, 3},
        {2, 5}
    };

    public Solider(PlayerSide owner)
        : base(TroopType.solider, owner, 100, 2)
    {
    }

    public override int Attack(int type){
        if(attackvalueDictionary.ContainsKey(type)){
            return attackvalueDictionary[type];
        }
        else{
            throw new ArgumentException($"Invalid attack type: {type}");
        }
    }

    public override int GetAttackRange(int type){
        if(attackRangeDictionary.ContainsKey(type)){
            return attackRangeDictionary[type];
        }
        else{
            throw new ArgumentException($"Invalid attack type: {type}");
        }
    }


}
