using System;
using System.Collections.Generic;
using System.Linq;

public static class MapFlowChecks
{
    static int passed;
    static KeyValuePair<HexPosition, AreaType> Cell(int q, int r, AreaType type = AreaType.field)
        => new KeyValuePair<HexPosition, AreaType>(new HexPosition(q, r), type);
    static BattleField Terrain() => new BattleField(new[] { Cell(0, 0), Cell(1, 0), Cell(0, 1, AreaType.forest) });
    static MapDefinition Map(string id = "test") => new MapDefinition(id, id, Terrain(),
        new[] { new InitialTroopDefinition(TroopType.solider, PlayerSide.Red, new HexPosition(0, 0)) },
        new[] { new InitialFacilityDefinition(FacilityType.city, null, new HexPosition(1, 0), 200) }, 500, 1000);
    static void Require(bool condition) { if (!condition) throw new Exception("Assertion failed"); }
    static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new Exception("Expected " + typeof(T).Name);
    }
    static void Check(string name, Action check)
    {
        check();
        passed++;
        Console.WriteLine("PASS " + name);
    }
    public static void Main()
    {
        Check("hex dictionary equality and six neighbours away from origin", () => {
            var origin = new HexPosition(12, -7);
            Require(origin.GetAdjacent().Distinct().Count() == 6);
            Require(origin.GetAdjacent().All(p => (p - origin).abs() == 1));
            Require(new Dictionary<HexPosition, int> { [origin] = 1 }[new HexPosition(12, -7)] == 1);
        });
        Check("terrain copies source and detects outside cells", () => {
            var source = new Dictionary<HexPosition, AreaType> { [new HexPosition(0, 0)] = AreaType.field };
            var terrain = new BattleField(source);
            source.Clear();
            Require(terrain.Contains(new HexPosition(0, 0)));
            Require(!terrain.Contains(new HexPosition(2, 2)));
            Throws<NotSupportedException>(() => ((IDictionary<HexPosition, AreaType>)terrain.Cells).Clear());
        });
        Check("empty duplicate and undefined terrain rejected", () => {
            Throws<ArgumentException>(() => new BattleField(Array.Empty<KeyValuePair<HexPosition, AreaType>>()));
            Throws<ArgumentException>(() => new BattleField(new[] { Cell(0, 0), Cell(0, 0) }));
            Throws<ArgumentException>(() => new BattleField(new[] { Cell(0, 0, (AreaType)99) }));
        });
        Check("map rejects missing identity and negative funds", () => {
            Throws<ArgumentException>(() => new MapDefinition("", "test", Terrain(), Array.Empty<InitialTroopDefinition>(), Array.Empty<InitialFacilityDefinition>()));
            Throws<ArgumentOutOfRangeException>(() => new MapDefinition("test", "test", Terrain(), Array.Empty<InitialTroopDefinition>(), Array.Empty<InitialFacilityDefinition>(), -1));
        });
        Check("map copies initial placement arrays", () => {
            var source = new[] { new InitialTroopDefinition(TroopType.solider, PlayerSide.Red, new HexPosition(0, 0)) };
            var map = new MapDefinition("copy", "copy", Terrain(), source, Array.Empty<InitialFacilityDefinition>());
            source[0] = null;
            Require(map.InitialTroops[0] != null);
        });
        Check("duplicate and outside initial troops rejected", () => {
            var troop = Map().InitialTroops[0];
            Throws<ArgumentException>(() => new MapDefinition("dup", "dup", Terrain(), new[] { troop, troop }, Array.Empty<InitialFacilityDefinition>()));
            Throws<ArgumentException>(() => new MapDefinition("outside", "outside", Terrain(), new[] {
                new InitialTroopDefinition(TroopType.solider, PlayerSide.Red, new HexPosition(9, 9))
            }, Array.Empty<InitialFacilityDefinition>()));
        });
        Check("facilities need valid distinct land positions", () => {
            var facility = Map().InitialFacilities[0];
            Throws<ArgumentException>(() => new MapDefinition("dup", "dup", Terrain(), Array.Empty<InitialTroopDefinition>(), new[] { facility, facility }));
            Throws<ArgumentException>(() => new MapDefinition("forest", "forest", Terrain(), Array.Empty<InitialTroopDefinition>(), new[] {
                new InitialFacilityDefinition(FacilityType.city, null, new HexPosition(0, 1), 200)
            }));
        });
        Check("catalog rejects duplicate ids and unknown selection leaves prior selection", () => {
            var map = Map();
            Throws<ArgumentException>(() => new MapCatalog(new[] { map, map }));
            var selection = new MapSelection();
            var useCase = new SelectMapUseCase(new MapCatalog(new[] { map }), selection);
            useCase.Execute(map.MapId);
            Throws<ArgumentException>(() => useCase.Execute("missing"));
            Require(selection.GetSettings().MapId == map.MapId);
        });
        Check("create before selection rejected", () => {
            var create = new CreateMatchUseCase(new MapCatalog(new[] { Map() }), new MapSelection(), new GameFactory(new TroopFactory()));
            Throws<InvalidOperationException>(() => create.Execute());
        });
        Check("two matches share immutable terrain but independent units facilities and funds", () => {
            var map = Map();
            var factory = new GameFactory(new TroopFactory());
            var first = factory.Create(map);
            var second = factory.Create(map);
            Require(first.RedState.Money == 500 && first.BlueState.Money == 1000);
            Require(first.BoardState.ContainsPosition(new HexPosition(0, 1)));
            Require(!first.BoardState.ContainsPosition(new HexPosition(3, 3)));
            var troop = first.BoardState.GetOnBoardTroops().Single();
            troop.RecieveDamage(30);
            first.RedState.SpendMoney(100);
            first.BoardState.Facilities.Values.Single().ApplyOccupation(20, PlayerSide.Red);
            Require(second.BoardState.GetOnBoardTroops().Single().CurrentHealth.CurrentValue == 100);
            Require(second.BoardState.Facilities.Values.Single().CurrentDurability.CurrentValue == 200);
            Require(second.BoardState.Facilities.Values.Single().Owner == null);
            Require(second.RedState.Money == 500);
            Require(map.InitialFacilities.Single().MaxDurability == 200);
        });
        Check("selecting another map changes next match", () => {
            var other = new MapDefinition("other", "other", new BattleField(new[] { Cell(7, 4) }),
                Array.Empty<InitialTroopDefinition>(), Array.Empty<InitialFacilityDefinition>());
            var catalog = new MapCatalog(new[] { Map(), other });
            var selection = new MapSelection();
            var select = new SelectMapUseCase(catalog, selection);
            var create = new CreateMatchUseCase(catalog, selection, new GameFactory(new TroopFactory()));
            select.Execute("test");
            Require(create.Execute().BoardState.ContainsPosition(new HexPosition(0, 0)));
            select.Execute("other");
            Require(create.Execute().BoardState.ContainsPosition(new HexPosition(7, 4)));
            Require(!create.Execute().BoardState.ContainsPosition(new HexPosition(0, 0)));
        });
        Check("unsupported initial unit and impassable terrain rejected", () => {
            var factory = new GameFactory(new TroopFactory());
            Throws<InvalidOperationException>(() => factory.Create(new MapDefinition("unsupported", "unsupported", Terrain(),
                new[] { new InitialTroopDefinition(TroopType.lightTank, PlayerSide.Red, new HexPosition(0, 0)) }, Array.Empty<InitialFacilityDefinition>())));
            Throws<InvalidOperationException>(() => factory.Create(new MapDefinition("sea", "sea", new BattleField(new[] { Cell(0, 0, AreaType.sea) }),
                new[] { new InitialTroopDefinition(TroopType.solider, PlayerSide.Red, new HexPosition(0, 0)) }, Array.Empty<InitialFacilityDefinition>())));
        });
        Check("starting a match preserves initial ids for later production", () => {
            var game = new GameFactory(new TroopFactory()).Create(Map());
            new TurnService(game).StartGame();
            game.BoardState.AddTroop(new Solider(PlayerSide.Red), new HexPosition(0, 1));
            Require(game.BoardState.GetOnBoardTroops().Count == 2);
        });
        Check("board refuses outside placement", () => {
            var game = new GameFactory(new TroopFactory()).Create(Map());
            Throws<InvalidOperationException>(() => game.BoardState.AddTroop(new Solider(PlayerSide.Red), new HexPosition(99, 99)));
            Require(game.BoardState.GetOnBoardTroops().Count == 1);
        });
        Console.WriteLine($"{passed} map flow checks passed.");
    }
}
