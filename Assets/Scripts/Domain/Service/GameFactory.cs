using System;
using System.Collections.Generic;

public sealed class GameFactory
{
    readonly TroopFactory troopFactory;

    public GameFactory(TroopFactory troopFactory)
    {
        this.troopFactory = troopFactory ?? throw new ArgumentNullException(nameof(troopFactory));
    }

    public GameState Create(MapDefinition map)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));

        Dictionary<string, Facility> facilities = new();
        foreach (InitialFacilityDefinition initial in map.InitialFacilities)
        {
            Facility facility = new Facility(initial.Type, initial.Position,
                initial.MaxDurability, initial.Owner);
            facilities.Add(facility.Id, facility);
        }
        BoardState board = new BoardState(map.Terrain, facilities);
        return new GameState(board, map.RedInitialMoney, map.BlueInitialMoney);
    }
}
