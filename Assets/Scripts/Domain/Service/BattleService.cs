using System;
using System.Collections.Generic;
using System.Linq;


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

        if(attacker.Owner == defender.Owner)
        {
            throw new InvalidOperationException();
        }

        int attackerDamage = attacker.Attack(attackType);
        //防御側は必ずid:0の行動で反撃する
        int defenderDamage = defender.Attack(0);

        attacker.RecieveDamage(defenderDamage);
        defender.RecieveDamage(attackerDamage);

        if(attacker.CurrentHealth.CurrentValue == 0)
        {
            game.BoardState.LostTroop(attackerId);
        }
        if(defender.CurrentHealth.CurrentValue == 0)
        {
            game.BoardState.LostTroop(defenderId);
        }

    }

    public List<Troop> GetAttackableTroop(int attackType,string attackerId)
    {
        BoardState board =game.BoardState;

        Troop attacker = board.GetTroopById(attackerId);
        HexPosition origin = board.GetTroopPosition(attackerId);
        int range = attacker.GetAttackRange(attackType);
        List<Troop> targets = new List<Troop>();

        foreach(Troop target in board.GetOnBoardTroops())
        {
            if (target.Owner == attacker.Owner)
            continue;

            HexPosition targetPosition = board.GetTroopPosition(target.Id);

            if ((targetPosition - origin).abs() <= range)
            {
                targets.Add(target);
            }
        }

        return targets;
    }
}