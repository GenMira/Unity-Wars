using VContainer;
using VContainer.Unity;
public class GameLifeTimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Pure C#クラスの登録
        builder.Register<GameState>(Lifetime.Singleton);

        builder.Register<BattleService>(Lifetime.Singleton);
        builder.Register<MoveService>(Lifetime.Singleton);
        builder.Register<TurnService>(Lifetime.Singleton);
        builder.Register<ProductService>(Lifetime.Singleton);

    }
}