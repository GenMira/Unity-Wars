using System;
using System.Collections.Generic;

public static class Data
{

    public static int GetTroopPrice(TroopType type) =>type switch
    {
        TroopType.solider => 500,
    };

    public static AreaType[] GetInvadableAreaType(TroopType type) => type switch
    {
        TroopType.solider => new AreaType[]{
            AreaType.field,
            AreaType.forest,
            AreaType.mountain},
        TroopType.artillery => new AreaType[]{
            AreaType.field,
            AreaType.forest,
        },
        TroopType.armoredCar => new AreaType[]{
            AreaType.field,
            AreaType.forest},
        TroopType.heavyTank => new AreaType[]{
            AreaType.field,
        },
        TroopType.lightTank => new AreaType[]{
            AreaType.field,
        },
        TroopType.selfPropelledGun => new AreaType[]
        {
            AreaType.field,
            AreaType.forest,
        },
        TroopType.ranger => new AreaType[]
        {
            AreaType.field,
            AreaType.forest,
            AreaType.mountain,
        }

    };

}
