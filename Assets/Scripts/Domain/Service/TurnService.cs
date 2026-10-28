public class TurnService
{
    private GameState game;

    public TurnService(GameState game)
    {
        this.game = game;
    }

    public void StartGame()
    {
        game.BoardState.Reset();
        
    }


    public void ChangePhase()
    {
        game.TurnState.ChangePhase();
        if(game.TurnState.Phase == TurnPhase.Starting)
        {
            game.BoardState.Reset();
        }
    }
}
