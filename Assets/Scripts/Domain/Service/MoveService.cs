using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using R3;
using System;
using System.Diagnostics;
using System.Linq;
using Unity.Collections;
public sealed class MoveService
{
    readonly GameState game;
    HexPosition troopPosition;
    Troop movingTroop;
    int currentMovementRange;


    public MoveService(GameState game)
    {
        this.game = game;        
    }

    public void StartMove(string troopId)
    {
        movingTroop = game.BoardState.GetTroopById(troopId);
        if (!movingTroop.IsAlreadyMoved)
        {
            troopPosition = game.BoardState.GetTroopPosition(troopId);
            currentMovementRange = movingTroop.MaxMovementRange;
        }
        else
        {
            throw new InvalidOperationException();
        }
        
    }

    public void Move(string troopId,HexPosition route)
    {
        game.BoardState.MoveTroop(troopId,route);        
    }

    public bool CanMove(HexPosition pos)
    {
        if (currentMovementRange <= 0)
        {
            return false;
        }
        if (game.BoardState.IsTroopAlreadyExist(troopPosition + pos))
        {
            return false;
        }

        currentMovementRange--;

        return true;
        
    }
    public void UndoMove()
    {
        currentMovementRange++;
    }

    public void Mount(string troopId,string viecleId)
    {
        BoardState board = game.BoardState;
        Troop mountUnit = board.GetTroopById(troopId);
        Troop viecleUnit = board.GetTroopById(viecleId);

        board.MountTroop(mountUnit, viecleUnit);
    }

    public List<HexPosition> GetDismountable(string troopId)
    {
        BoardState board = game.BoardState;
        TroopType type = board.GetTroopById(troopId).Type;
        String carrierId = board.GetCarrier(troopId);

        List<HexPosition> adjacentCarrier = board.GetTroopPosition(carrierId).GetAdjacent();
        AreaType[] invadableTypes = Data.GetInvadableAreaType(type);
        
        return board.GetAreatypes(adjacentCarrier)
            .Where(x => invadableTypes.Contains(x.Value))
            .Where(x => !board.IsTroopAlreadyExist(x.Key))
            .Select(x => x.Key)
            .ToList();

    }
    public void DisMount(string troopId,HexPosition direction)
    {
        if (direction.normalized())
        {
            game.BoardState.DismountTroop(troopId,direction);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}