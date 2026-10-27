using System;

public class PlayerState
{
    public PlayerSide Side {get;}
    public int Money {get; private set;}
    
    public PlayerState(PlayerSide playerSide)
    {
        Side = playerSide;
        Money = 0;
    }

    public void AddMoney(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        Money = Money + amount;
    }

    public bool SpendMoney(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        if (Money < amount)
            return false;
        
        Money -=amount;
        return true;
        
    }
}
