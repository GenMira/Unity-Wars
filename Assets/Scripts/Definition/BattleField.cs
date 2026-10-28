using System.Collections.Generic;

public class BattleField
{
    public Dictionary<HexPosition, AreaType> default = new Dictionary<HexPosition, AreaType>{
        {0,0},AreaType.field,
    }

    public Battlefield(Dictionary<HexPosition, AreaType> dict)
    {
        
    }
}