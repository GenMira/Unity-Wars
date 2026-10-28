using System.Collections;

public class TroopFactory
{
    public bool CanCreate(TroopType type) => type == TroopType.solider;

    public Troop CreateTroop(TroopType type, PlayerSide owner) => type switch
    {
        TroopType.solider => new Solider(owner),
    };

}
