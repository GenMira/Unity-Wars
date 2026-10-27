using System;
using System.Collections.Generic;

public static class Data
{

    public static int GetTroopPrice(TroopType type) =>type switch
    {
        TroopType.solider => 500,
    };

}