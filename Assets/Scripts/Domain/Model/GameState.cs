using System;

public class GameState
{
    public WarState WarState{get; private set;}
    public TurnState TurnState{get; private set;}
    public PlayerState RedState{get; private set;}
    public PlayerState BlueState{get; private set;}
    public BoardState BoardState{get; private set;}

    public GameState(BoardState boardState, int redInitialMoney = 0, int blueInitialMoney = 0)
    {
        if (boardState == null) throw new ArgumentNullException(nameof(boardState));
        if (redInitialMoney < 0) throw new ArgumentOutOfRangeException(nameof(redInitialMoney));
        if (blueInitialMoney < 0) throw new ArgumentOutOfRangeException(nameof(blueInitialMoney));

        WarState = WarState.Prepareing;
        TurnState = new TurnState();
        RedState = new PlayerState(PlayerSide.Red);
        BlueState = new PlayerState(PlayerSide.Blue);
        RedState.AddMoney(redInitialMoney);
        BlueState.AddMoney(blueInitialMoney);
        BoardState = boardState;
    }

    public void ChangeWarState()
    {
        switch (WarState)
        {
            case WarState.Prepareing:
                WarState = WarState.Playing;
                break;
            case WarState.Playing:
                WarState = WarState.Finished;
                break;
        }
    }


}

