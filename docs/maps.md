# マップ定義と対戦への受け渡し

## データの流れ

`MapDefinitionAsset → MapDefinition / BattleField → MapCatalog → SelectMapUseCase → MapSelection → CreateMatchUseCase → GameState / BoardState`

- AssetはUnityで編集する初期データ。地形はaxial座標のq・rとAreaTypeを配列で指定する。
- BattleFieldは座標をキーとする読み取り専用の地形。登録されていない座標は盤面外。
- MapDefinitionは地形、初期部隊、初期施設、陣営ごとの初期資金を保持する。Unityの型は含まない。
- GameFactoryは毎回新しい施設・部隊・資金状態を作る。不変な地形だけを共有するので、再戦に前の損傷や配置を持ち越さない。
- 初期施設のIs Neutralを有効にすると中立になる。施設所有者のnullは中立を表す。
- 初期資金は設定値そのもの。ターン収入の加算やPlayingへの遷移は、このマップ生成処理では行わない。

## Unityでの設定

1. Projectで `Create > Unity Wars > Map Definition` を選び、Map IdとDisplay Nameを設定する。
2. Cellsに地形を登録する。Initial Troops / Initial Facilities / Initial Moneyは必要に応じて指定する。
3. `Assets/Maps/SampleHexMap.asset`を編集例として利用できる。7マスの動作確認用であり、対戦バランスの調整用マップではない。
4. 起動シーンまたはStageSelectシーンのルートGameObjectに`MapSelectionLifetimeScope`を追加し、Mapsにアセットを登録する。このGameObjectはシーン遷移後も生存する。
5. 選択画面のPresenterへ、親スコープの`SelectMapUseCase`を注入する。`Maps`から一覧を表示し、決定時に`Execute(mapId)`を呼んでからBattleシーンをロードする。
6. Battleシーンに`GameLifeTimeScope`を配置する。`FindParent()`が生存中の`MapSelectionLifetimeScope`を探し、選択済みマップから対戦専用の`GameState`を生成する。各Serviceへはその同じGameStateが注入される。
7. 再戦はBattleシーンを読み直す。別のマップを選ぶ場合はStageSelectへ戻り、SelectMapUseCaseで選択を更新してからBattleを開く。

選択画面内でも専用LifetimeScopeを設ける場合は、Parentを`MapSelectionLifetimeScope`に設定する。既存の選択結果を初期値で上書きしない。
起動シーンなしでBattleを直接再生すると、親スコープまたは選択情報がないため開始できない。マップの自動選択は行わない。
選択UI、シーンロード、Tilemap描画はこの変更には含まない。Viewは`BoardState.Terrain`を列挙して描画できる。施設は`BoardState.Facilities`から取得する。Tilemapのoffset座標とDomainのaxial座標の変換はView側に置く。

## 検証

読み込み時に、空マップ、地形の座標重複、未定義enum、負の資金、初期配置の重複・盤面外、陸上施設の平野以外への配置を拒否する。
対戦生成時に、未実装兵種と侵入不可地形への初期部隊配置も拒否する。現在生成できる兵種は歩兵のみ。
基地数、基地間の到達可能性、初期生産可否などの対戦成立条件は、地形を試作できるよう現段階では強制していない。

```sh
python3 tests/run_map_checks.py
```

.NET 10 SDKとリポジトリ内のR3 DLLを使い、実際のDomain/Applicationソースを一時プロジェクトへリンクして検証する。Unity生成csprojは変更しない。
