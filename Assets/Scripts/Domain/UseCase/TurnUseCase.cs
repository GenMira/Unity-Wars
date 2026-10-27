public class TurnUseCase
{
    private GameState game;

    public TurnUseCase(GameState game)
    {
        this.game = game;
    }

    public void ChangeTurn()
    {
        game.TurnState.ChangeTurn();
        game.BoardState.Reset();
    }

    public void ChangePhase()
    {
        game.TurnState.ChangePhase();
        if(game.TurnState.Phase == TurnPhase.Starting)
        {
            game.TurnState.ChangeTurn();
            game.BoardState.Reset();
        }
    }
}