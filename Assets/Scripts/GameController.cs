using UnityEngine;

public class GameController
{
    [SerializeField] private GameState gameState;
    public GameController(GameState gameState)
    {
        this.gameState = gameState
            ?? throw new System.ArgumentNullException(nameof(gameState));
    }

}
