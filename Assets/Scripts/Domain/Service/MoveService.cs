using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using R3;
public sealed class MoveService
{
    readonly GameState game;
    private Stack<HexPosition> moveHistory = new Stack<HexPosition>();
    private HexPosition route;

    public MoveService(GameState game)
    {
        this.game = game;
    }

    public ReadOnlyReactiveProperty<int> StartMove(string troopId)
    {
        route = new HexPosition(0,0);
        moveHistory = new Stack<HexPosition>();
        moveHistory.Push(new HexPosition(0,0));
        return game.BoardState.GetTroopById(troopId).CurrentMovementRange;
    }
    public void Nove(HexPosition direction)
    {
        if(direction.normalized()){
            moveHistory.Push(direction);
            route+=direction;
        }
    }

    public void UndoMove()
    {
        if (moveHistory.Count > 0)
        {
            HexPosition lastMove = moveHistory.Pop();
            route-=lastMove;
        }
    }

    public void EndMove(string troopId)
    {
        game.BoardState.MoveTroop(game.BoardState.GetTroopById(troopId),route);
        moveHistory.Clear();
        route = new HexPosition(0,0);
    }
}