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
    };

}