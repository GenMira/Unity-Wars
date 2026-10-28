public class TurnState{
    public int Turn{get;private set;}
    public TurnPhase Phase{get;private set;}
    public PlayerSide Player{get;private set;}

    public TurnState()
    {
        Turn = 0;
        Player = PlayerSide.Red;
        Phase = TurnPhase.Starting;
    }

    public void ChangePhase()
    {
        switch (Phase)
        {
            case TurnPhase.Starting:
                Phase = TurnPhase.Acting;
                break;
            case TurnPhase.Acting:
                Phase = TurnPhase.Ending;
                break;
            case TurnPhase.Ending:
                Phase = TurnPhase.Starting;
                ChangeTurn();
                break;
        }
    }

    private void ChangeTurn()
    {
        Turn+=1;

        if (Player == PlayerSide.Red)
        {
            Player = PlayerSide.Blue;
        }
        else
        {
            Player = PlayerSide.Red;
        }
    }
}
