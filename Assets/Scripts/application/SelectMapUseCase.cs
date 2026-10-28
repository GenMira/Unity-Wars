using System;
using System.Collections.Generic;

public sealed class SelectMapUseCase
{
    readonly MapCatalog catalog;
    readonly MapSelection selection;
    public IReadOnlyList<MapDefinition> Maps => catalog.Maps;

    public SelectMapUseCase(MapCatalog catalog, MapSelection selection)
    {
        this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        this.selection = selection ?? throw new ArgumentNullException(nameof(selection));
    }

    public void Execute(string mapId)
    {
        catalog.Get(mapId);
        selection.Select(new MatchStartSettings(mapId));
    }
}
