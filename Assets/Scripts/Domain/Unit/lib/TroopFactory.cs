public class TroopFactory
{
    public static Troop CreateTroop(TroopType type, PlayerSide owner)
    {
        switch(type)
        {
            case TroopType.solider:
                return new Solider(owner);
            default:
                throw new System.Exception($"Unknown troop type: {type}");
        }
    }
}