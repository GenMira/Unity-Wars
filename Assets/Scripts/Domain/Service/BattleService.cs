public class BattleService
{
    readonly GameState game;

    public BattleService(GameState game)
    {
        this.game = game;
    }

    public void Attack(int attackType,string attackerId, string defenderId)
    {
        BoardState board = game.BoardState;

        Troop attacker = board.GetTroopById(attackerId);
        Troop defender = board.GetTroopById(defenderId);

        int attackerDamage = attacker.Attack(attackType);
        int defenderDamage = defender.Attack(0);

        attacker.recieveDamage(defenderDamage);
        defender.recieveDamage(attackerDamage);

        if(attacker.CurrentHealth.CurrentValue == 0)
        {
            game.BoardState.LostTroop(attackerId);
        }
        if(defender.CurrentHealth.CurrentValue == 0)
        {
            game.BoardState.LostTroop(defenderId);
        }

    }
}