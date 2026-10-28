using System;

public sealed class CreateMatchUseCase
{
    readonly MapCatalog catalog;
    readonly MapSelection selection;
    readonly GameFactory factory;

    public CreateMatchUseCase(MapCatalog catalog, MapSelection selection, GameFactory factory)
    {
        this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        this.selection = selection ?? throw new ArgumentNullException(nameof(selection));
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    // 呼び出すたびに独立した対戦を作る。選択した定義自体は変更しない。
    public GameState Execute() => factory.Create(catalog.Get(selection.GetSettings().MapId));
}
