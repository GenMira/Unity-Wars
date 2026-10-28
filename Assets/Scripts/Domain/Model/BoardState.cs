using System;
using System.Collections.Generic;
using System.Linq;
using R3;

public class BoardState
{
    readonly Dictionary<string, Facility> facilities;
    readonly Dictionary<string, Troop> troops;
    readonly Dictionary<string, TroopPlacement> placements;

    readonly BattleField battleField;
    public IReadOnlyDictionary<HexPosition, AreaType> Terrain => battleField.Cells;
    public IReadOnlyDictionary<string, Facility> Facilities { get; }
    readonly Subject<string> troopLosted = new();

    public BoardState(
        Dictionary<HexPosition,AreaType> battlefield,
        Dictionary<string, Facility> facilities
    ) : this(new BattleField(battlefield), facilities) { }

    public BoardState(BattleField battlefield, IDictionary<string, Facility> facilities)
    {
        this.battleField = battlefield ?? throw new ArgumentNullException(nameof(battlefield));
        if (facilities == null){
            throw new ArgumentNullException(nameof(facilities));
        }
        
        this.facilities = new Dictionary<string, Facility>(facilities);
        Facilities = new System.Collections.ObjectModel.ReadOnlyDictionary<string, Facility>(this.facilities);
        troops = new Dictionary<string, Troop>();
        placements = new Dictionary<string, TroopPlacement>();
    }

    // admin troop
    public void AddTroop(Troop troop, HexPosition position)
    {
        if (troop == null) throw new ArgumentNullException(nameof(troop));
        if (!battleField.Contains(position))
            throw new InvalidOperationException($"盤面外には部隊を配置できません: {position}");
        if (troops.ContainsKey(troop.Id) ||
            placements.ContainsKey(troop.Id))
        {
            throw new InvalidOperationException(
                $"部隊IDがすでに登録されています: {troop.Id}");
        }

        foreach (TroopPlacement placement in placements.Values)
        {
            if (placement is TroopPlacement.OnBoard onBoard &&
                onBoard.Position.Equals(position))
            {
                throw new InvalidOperationException(
                    "配置先には別の部隊がいます。");
            }
        }

        troops.Add(troop.Id, troop);
        placements.Add(
            troop.Id,
            new TroopPlacement.OnBoard(position));

    }

    public void MoveTroop(string troopId, HexPosition direction)
    {
        if (placements.TryGetValue(troopId, out TroopPlacement placement))
        {
            if (placement is TroopPlacement.OnBoard onBoard)
            {
                placements[troopId] = new TroopPlacement.OnBoard(onBoard.Position + direction);
                // UnitUpdated?.Invoke();
                troops[troopId].DoneMove();
            }
            else
            {
                throw new InvalidOperationException(
                    $"搭乗中の部隊は移動できません: {troopId}");
            }
        }
        else{
            throw new InvalidOperationException(
            $"部隊が見つかりません: {troopId}");
        }
    }

    public void MountTroop(Troop troop, Troop carrier)
    {
        if (placements.TryGetValue(troop.Id, out TroopPlacement placement))
        {
            if (placement is TroopPlacement.OnBoard onBoard)
            {
                placements[troop.Id] = new TroopPlacement.Embarked(carrier.Id);
            }
        }
    }

    //idは降車させる部隊のid
    public void DismountTroop(string troopId, HexPosition direction)
    {
        if (placements.TryGetValue(troopId, out TroopPlacement troopPlacement))
        {
            if (troopPlacement is TroopPlacement.Embarked embarked)
            {
                placements.TryGetValue(
                    embarked.CarrierId,
                    out TroopPlacement vieclePosition);

                if(vieclePosition is not TroopPlacement.OnBoard vielcePositionOnBoard)
                {
                    return;
                }

                placements[troopId] = new TroopPlacement.OnBoard(vielcePositionOnBoard.Position+direction);
            }
            else{
                throw new InvalidOperationException(
                $"場所が見つかりません: {troopPlacement.ToString()}");               
            }

        }
        else
        {
            throw new InvalidOperationException(
            $"部隊が見つかりません: {troopId}");
        }
    }

    // lib
    public Troop GetTroopById(string troopId)
    {
        if (troops.TryGetValue(troopId, out Troop troop))
        {
            return troop;
        }
        throw new InvalidOperationException(
            $"Troopが登録されていません: {troopId}");
    }

    public void Reset()
    {
        foreach (Troop troop in troops.Values)
        {
            troop.Reset();
        }
    }

    public HexPosition GetTroopPosition(string troopId)
    {
        if (placements.TryGetValue(troopId, out TroopPlacement placement))
        {
            if (placement is TroopPlacement.OnBoard onBoard)
            {
                return onBoard.Position;
            }
            else
            {
                throw new InvalidOperationException(
                    $"搭乗中の部隊は位置を取得できません: {troopId}");
            }
        }
        else
        {
            throw new InvalidOperationException(
                $"部隊が見つかりません: {troopId}");            
        }
    }

    public void LostTroop(string troopId)
    {
        if (placements.TryGetValue(troopId, out TroopPlacement placement))
        {
            foreach(string embarkedTroopId in GetEmbarked(troopId))
            {
                troops.Remove(embarkedTroopId);
                placements.Remove(embarkedTroopId);
            }
            troops.Remove(troopId);
            placements.Remove(troopId);
            troopLosted.OnNext(troopId);

        }
        else
        {
            throw new InvalidOperationException(
                $"部隊が見つかりません: {troopId}");            
        }

    }

    public List<String> GetEmbarked(string carrierId)
    {
        if (placements.TryGetValue(carrierId, out TroopPlacement placement))
        {
            return placements
                .Where(x =>
                    x.Value is TroopPlacement.Embarked embarked &&
                    embarked.CarrierId == carrierId)
                .Select(x => x.Key)
                .ToList();
        }
        else{
            throw new InvalidOperationException(
            $"部隊が見つかりません: {carrierId}");
        }
    }

    public String GetCarrier(string troopId)
    {
        if(placements.TryGetValue(troopId,out TroopPlacement placement))
        {
            if(placements[troopId] is TroopPlacement.Embarked embarked)
            {
                return embarked.CarrierId;
            }
            else
            {
                throw new InvalidOperationException(
                $"部隊が見つかりません: {troopId}");
            }
        }
        else
        {
            throw new InvalidOperationException(
            $"部隊が見つかりません: {troopId}");
        }
    }

    public bool IsTroopAlreadyExist(HexPosition pos)
    {
        return placements.Values.Any(placement => 
        placement is TroopPlacement.OnBoard onBoard 
        && onBoard.Position.Equals(pos));
    }

    public List<Troop> GetOnBoardTroops()
    {
        return placements.Where(x => x.Value is TroopPlacement.OnBoard)
            .Select(x => troops[x.Key])
            .ToList();
    }

    public void Dispose()
    {
        troopLosted.Dispose();
    }

    public Dictionary<HexPosition,AreaType> GetAreatypes(List<HexPosition> positions)
    {
        return battleField.Cells
            .Where(x => positions.Contains(x.Key))
            .ToDictionary(x => x.Key, x => x.Value);
    }

    public bool ContainsPosition(HexPosition position) => battleField.Contains(position);

    public bool TryGetAreaType(HexPosition position, out AreaType areaType)
        => battleField.TryGetAreaType(position, out areaType);
}
