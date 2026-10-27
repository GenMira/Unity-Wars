using System.Collections.Generic;
public sealed class MoveTroopUseCase
{
    readonly GameState game;
    private Stack<HexPosition> moveHistory = new Stack<HexPosition>();

    public MoveTroopUseCase(GameState game)
    {
        this.game = game;
    }

    public void StartMove(string troopId)
    {
        moveHistory.Clear();
        move(troopId, game.BoardState.GetTroopPosition(troopId));
    }
    public HexPosition move(string troopId, HexPosition direction)
    {
        HexPosition newPosition = game.BoardState.MoveTroop(game.BoardState.GetTroopById(troopId), direction);
        moveHistory.Push(newPosition);
        return newPosition;
    }

    public void undoMove(string troopId)
    {
        if (moveHistory.Count > 0)
        {
            HexPosition lastMove = moveHistory.Pop();
            game.BoardState.MoveTroop(game.BoardState.GetTroopById(troopId), lastMove);
        }
    }

    public void EndMove(string troopId)
    {
        moveHistory.Clear();
    }
}