using UnityEngine;
using System;
using UnityEditor.U2D.Aseprite;
using Unity.Mathematics;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;

public class GameState
{
    public WarState WarState{get; private set;}
    public TurnState TurnState{get; private set;}
    public PlayerState RedState{get; private set;}
    public PlayerState BlueState{get; private set;}
    public BoardState BoardState{get; private set;}

    public GameState()
    {
        WarState = WarState.Prepareing;
        TurnState = new TurnState();
        RedState = new PlayerState(PlayerSide.Red);
        BlueState = new PlayerState(PlayerSide.Blue);
        BoardState = new BoardState();
    }

    public void ChangeWarState()
    {
        switch (WarState)
        {
            case WarState.Prepareing:
                WarState = WarState.Playing;
                break;
            case WarState.Playing:
                WarState = WarState.Finished;
                break;
        }
    }


    public void MountTroop(string troopId,string viecleId)
    {
        Troop mountUnit = BoardState.GetTroopById(troopId);
        Troop viecleUnit = BoardState.GetTroopById(viecleId);

        BoardState.MountTroop(mountUnit, viecleUnit);
    }

    public void DismountTroop(string troopId,HexPosition direction)
    {
        Troop mountUnit = BoardState.GetTroopById(troopId);

        BoardState.DismountTroop(mountUnit, direction);
    }

    public void AddTroop(TroopType type,PlayerSide player,HexPosition pos)
    {
        Troop troop = TroopFactory.CreateTroop(type,player);
        BoardState.AddTroop(troop,pos);
    }

    public void ChangePhase()
    {
        TurnState.ChangePhase();
        if(TurnState.Phase == TurnPhase.Starting)
        {
            TurnState.ChangeTurn();
            BoardState.Reset();
        }
    }




}

