public class Solider : ITroop
{
    public Solider()
    {
        id = "solider";
        type = Unit.solider;
        displayName = "Solider";
        maxHealth = 100;
        currentHealth = 100;
        movementRange = 3;
        cost = 100;
        mountableType = new Unit[];
        mountableAmount = 1;
    }
}