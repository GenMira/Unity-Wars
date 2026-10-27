using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class BoardState
{
    readonly Dictionary<string, IFacility> facilities;
    readonly Dictionary<string, Troop> troops;
    readonly Dictionary<string, TroopPlacement> placements;

    readonly Dictionary<string,AreaType> battleField;

    public BoardState()
    {
        facilities = new Dictionary<string, IFacility>();
        battleField = new Dictionary<string, AreaType>();
        troops = new Dictionary<string, Troop>();
        placements = new Dictionary<string, TroopPlacement>();
    }

    // admin troop
    public void AddTroop(Troop troop, HexPosition position)
    {
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

    public HexPosition MoveTroop(Troop troop, HexPosition direction)
    {
        if (placements.TryGetValue(troop.Id, out TroopPlacement placement))
        {
            if (placement is TroopPlacement.OnBoard onBoard)
            {
                placements[troop.Id] = new TroopPlacement.OnBoard(onBoard.Position + direction);
                // UnitUpdated?.Invoke();
                return onBoard.Position + direction;

            }
            else
            {
                throw new InvalidOperationException(
                    $"搭乗中の部隊は移動できません: {troop.Id}");
            }
        }
        throw new InvalidOperationException(
            $"部隊が見つかりません: {troop.Id}");
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

    public void DismountTroop(Troop troop, HexPosition direction)
    {
        if (placements.TryGetValue(troop.Id, out TroopPlacement placement))
        {
            if (placement is TroopPlacement.Embarked embarked)
            {
                placements[troop.Id] = new TroopPlacement.OnBoard(direction);
            }
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
        }
        else
        {
            throw new InvalidOperationException(
                $"部隊が見つかりません: {troopId}");            
        }

    }

    public List<String> GetEmbarked(string troopId)
    {
        if (placements.TryGetValue(troopId, out TroopPlacement placement))
        {
            return placements.Where(x=>x.Value.ToString() == troopId)
            .Select(x => x.Key)
            .ToList();
        }
        throw new InvalidOperationException(
            $"部隊が見つかりません: {troopId}");
    }

    public bool IsTroopAlreadyExist(HexPosition pos)
    {
        return placements.Values.Any(placement => 
        placement is TroopPlacement.OnBoard onBoard 
        && onBoard.Position.Equals(pos));
    }

}