using System.Collections.Generic;

public class BattleUseCase
{
    readonly BattleService battleService;

    public BattleUseCase(BattleService battleService){
        this.battleService = battleService;        
    }

    public void Execute(int attackType,string attackerId, string defenderId)
    {
        battleService.Attack(attackType,attackerId, defenderId);
    }

    public List<Troop> GetAttackable(int attackType,string attackerId)
    {
        return battleService.GetAttackableTroop(attackType,attackerId);
    }
}