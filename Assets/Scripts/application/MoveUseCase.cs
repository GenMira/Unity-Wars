using System.Collections.Generic;

public class MoveUseCase
{
    readonly MoveService moveService;
    private Stack<HexPosition> moveHistory = new Stack<HexPosition>();
    private HexPosition route;
    private string moveTroopId;

    public MoveUseCase(MoveService moveService)
    {
        this.moveService = moveService;
    }
    public void StartMove(string moveTroopId)
    {
        route = new HexPosition(0,0);
        moveHistory = new Stack<HexPosition>();
        moveHistory.Push(new HexPosition(0,0));
        this.moveTroopId = moveTroopId;
        moveService.StartMove(moveTroopId);
    }
    public void EndMove()
    {
        moveService.Move(moveTroopId,route);
    }

    public void Move(HexPosition direction)
    {
        if (direction.normalized())
        {
            route+=direction;
            if (moveService.CanMove(route))
            {
                moveHistory.Push(direction);
            }
            else
            {
                route-=direction;
            }
            
        }
    }
    public void UndoMove()
    {
        if (moveHistory.Count > 1)
        {
            HexPosition lastmove = moveHistory.Pop();
            route-=lastmove;
            moveService.UndoMove();
        }
    }
}