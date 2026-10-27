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
}