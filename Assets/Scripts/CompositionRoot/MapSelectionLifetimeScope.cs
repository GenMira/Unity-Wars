using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>起動画面に1つ配置し、選択画面と対戦画面の親として生存させる。</summary>
public sealed class MapSelectionLifetimeScope : LifetimeScope
{
    [SerializeField] MapDefinitionAsset[] maps = Array.Empty<MapDefinitionAsset>();

    protected override void Awake()
    {
        foreach (MapSelectionLifetimeScope existing in FindObjectsByType<MapSelectionLifetimeScope>(FindObjectsSortMode.None))
        {
            // 再び選択画面へ戻った際は、既に構築済みの親スコープを再利用する。
            if (existing != this && existing.Container != null)
            {
                Destroy(gameObject);
                return;
            }
        }
        DontDestroyOnLoad(gameObject);
        base.Awake();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        List<MapDefinition> definitions = new();
        foreach (MapDefinitionAsset asset in maps)
        {
            if (asset == null) throw new InvalidOperationException("マップアセットが未設定です。");
            try { definitions.Add(asset.ToDefinition()); }
            catch (Exception error)
            {
                throw new InvalidOperationException($"マップを読み込めません: {asset.name}", error);
            }
        }
        builder.RegisterInstance(new MapCatalog(definitions));
        builder.Register<MapSelection>(Lifetime.Singleton);
        builder.Register<SelectMapUseCase>(Lifetime.Singleton);
        builder.Register<TroopFactory>(Lifetime.Singleton);
        builder.Register<GameFactory>(Lifetime.Singleton);
        builder.Register<CreateMatchUseCase>(Lifetime.Singleton);
    }
}
