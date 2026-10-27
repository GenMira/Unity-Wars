using UnityEngine;
using System;
using System.Collections.Generic;

public class BoardState
{
    readonly Dictionary<string, IFacility> facilities;
    readonly Dictionary<string, Troop> troops = new();
    readonly Dictionary<string, TroopPlacement> placements = new();

    public BoardState()
    {
        facilities = new Dictionary<string, IFacility>();
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

    public void RemoveTroop(Troop troop)
    {
        troops.Remove(troop.Id);
        placements.Remove(troop.Id);
    }

    public HexPosition MoveTroop(Troop troop, HexPosition direction)
    {
        if(direction.normalized() == false)
        {
            throw new InvalidOperationException(
                $"移動方向は正規化されていません: {direction.q},{direction.r}");
        }

        if (placements.TryGetValue(troop.Id, out TroopPlacement placement))
        {
            if (placement is TroopPlacement.OnBoard onBoard)
            {
                if (troop.CheckMovability())
                {
                    placements[troop.Id] = new TroopPlacement.OnBoard(onBoard.Position + direction);
                    // UnitUpdated?.Invoke();
                    return onBoard.Position + direction;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"これ以上部隊は移動できません: {troop.Id}");
                }

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
        throw new InvalidOperationException(
            $"部隊が見つかりません: {troopId}");
    }

}