# Tilemapからマップを作成する

Tile Paletteで盤面を描き、Map AuthoringのInspectorからMapDefinitionAssetを書き出す。
Tilemap／編集シーンが編集元、MapDefinitionAssetが対戦用の生成データになる。対戦時は従来どおりMapCatalogから選ぶ。
書き出したアセットの地形・施設を手修正すると、次の書き出しで置き換わる。

## 既存のBattle盤面を使う

1. `Assets/Scenes/Battle.unity`を開き、Hierarchyの`Grid/Tilemap`を選ぶ。
2. `Tools > Unity Wars > Use Selected Tilemap as Mixed Map Source`を実行する。
3. 追加されたMap Authoringで、新規作成用のMap Id、Display Name、初期資金を入力する。
4. `検証`を押す。成功したら`新しいMap Definitionを作成`でAssets内へ保存する。
5. 保存後はOutputへ出力先が自動設定される。Tile Paletteで変更したら`既存Map Definitionを更新`する。

`Assets/Maps/DefaultMapTileCatalog.asset`は自動で設定される。現在あるfield_0、town_0、Factory_0を登録済み。
市街地・工業地帯Tileは「平野＋中立施設」として出力する。混在モードで所有者を変える場合はカタログのIs Neutral／Ownerを設定する。
同じ施設Tileをマスごとに別陣営へ割り当てる場合は、次の分離モードを使う。
元のBattleシーンはメニュー操作を行うまで変更されない。編集専用にSceneを複製して保存してもよい。

## 新しい盤面を地形・施設のレイヤーで作る

1. `GameObject > Unity Wars > Map Authoring`を実行する。
2. 六角GridとTerrain／Neutral Facilities／Red Facilities／Blue Facilitiesが作られる。
3. Tile PaletteのActive Tilemapを切り替え、Terrainには地形Tile、施設用レイヤーには施設Tileを描く。
4. Map AuthoringへMap Id・Display Name・初期資金を入力して検証・書き出しする。

施設は下に地形がある場所へ置く。施設レイヤーで所有者が決まるため、同じ施設Tileを赤・青・中立で使い回せる。
施設レイヤーは未指定でもよい。地形は必須。空セルは盤面外として出力され、不規則な形や穴のあるマップも作れる。
現在の素材は平野・市街地・工場のみ。森林・山岳・海を使う場合は対応するTileアセットを用意し、カタログへ追加する。

## Tileの対応付け

`Create > Unity Wars > Map Tile Catalog`で作成する。Rulesへ次を登録する。

| フィールド | 内容 |
| --- | --- |
| Tile | 判定対象のTileアセット。名前やSpriteでは判定しない |
| Terrain | 地形種別。混在モードの施設Tileではその下の地形 |
| Has Facility | 施設Tileなら有効 |
| Facility Type / Max Durability | 施設種類と初期・最大耐久度 |
| Is Neutral / Owner | 混在モードの施設所有者。分離モードではレイヤーが優先 |

描画の見た目から兵科・施設を推測しない。カタログに未登録のTile、同じTileの重複登録はエラーになる。
森林など見た目が異なる複数Tileを同じTerrainへ対応付けることもできる。

## 更新と検証

- 新規作成はMap Authoringのメタデータを使用する。
- 既存更新はOutputのID・表示名・サムネイル・初期資金を維持し、地形・施設の配列だけを全置換する。
- 消したTileは生成データからも消える。出力順はq・rで固定するので、同じ盤面の再出力では内容が変わらない。
- エラーがある場合は保存しない。施設の重複、地形のない施設、陸上施設を平野以外へ置く操作などを検出する。
- エラー欄にTilemap名・セル座標・Tile名、またはDomain検証の座標と理由を表示する。
- 既存更新はUndo／Redoに対応する。Undo後の状態をファイルへ保存する場合は通常のSave Projectを行う。
- 再出力しても既存アセットのGUIDは変わらず、選択画面からの参照を維持する。
- 編集中の書き出し専用。Play ModeではInspectorの書き出し操作を無効にする。

## 座標と実行時の利用

対応設定はHexagon Grid、Cell SwizzleがXYZまたはYXZ、Tilemap OrientationがXY、TileのZが0。
分離レイヤーは同じGrid配下に置き、Transform・Tile Anchorも一致させる。異なる設定を検出した場合は書き出しを拒否する。
HexToWorldは画像のTile Anchorではなく六角Gridのセル中心を返す。自動作成するレイヤーのTile Anchorは0に設定する。
XYZとYXZで共通のoffsetセル座標から、Domainのaxial座標へ変換する。負の行でも変換できる。

描画・クリック処理も同じ`HexTilemapCoordinates`を利用する。

```csharp
HexPosition position = HexTilemapCoordinates.CellToHex(tilemap, cell);
Vector3Int cell = HexTilemapCoordinates.HexToCell(tilemap, position);
Vector3 center = HexTilemapCoordinates.HexToWorld(tilemap, position);
HexPosition clicked = HexTilemapCoordinates.WorldToHex(tilemap, worldPosition);
```

既存のBoardClickDetecter.CellClickedは引き続きUnityのセル座標を返す。Domainへ渡す前にCellToHexで変換する。
DomainはTilemapやUnityEditorに依存せず、生成済みMapDefinitionを使う。

## 自動確認

```sh
python3 tests/run_tilemap_checks.py
```

一時的な別Unityプロジェクトへ必要なソース・素材をコピーし、Unity 6000.3.25f1のバッチモードで確認する。
元のプロジェクトや開いているシーンは変更しない。別のUnity実行ファイルを使う場合は`--unity /path/to/Unity`を指定する。
座標の往復と画面上の隣接、混在・分離モード、失敗時の無変更、再出力、メタデータ・GUID維持、Undo／Redoを検証する。
