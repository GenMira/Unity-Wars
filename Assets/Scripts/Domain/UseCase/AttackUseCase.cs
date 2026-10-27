public class AttackUseCase
{
    readonly GameState game;

    public AttackUseCase(GameState game)
    {
        this.game = game;
    }

    public void Execute(int attackType,string attackerId, string receiverId)
    {
        BoardState board = game.BoardState;

        Troop attacker = board.GetTroopById(attackerId);
        Troop defender = board.GetTroopById(receiverId);

        int attackerDamage = attacker.Attack(attackType);
        int defenderDamage = defender.Attack(attackType);

        attacker.recieveDamage(defenderDamage);
        defender.recieveDamage(attackerDamage);
    }
}