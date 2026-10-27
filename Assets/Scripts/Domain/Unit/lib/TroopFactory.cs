using System.Collections;

public class TroopFactory
{
    public Troop CreateTroop(TroopType type, PlayerSide owner) => type switch
    {
        TroopType.solider => new Solider(owner),
    };

}

// public enum TroopType
// {
//     solider,
//     artillery,
//     armoredCar,
//     selfPropelledGun,
//     lightTank,
//     heavyTank
// }