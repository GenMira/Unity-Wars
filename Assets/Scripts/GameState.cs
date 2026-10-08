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

public enum PlayerSide
{
    Red,
    Blue
}

public enum WarState
{
    Prepareing,
    Playing,
    Finished
}

public enum TurnPhase
{
    Starting,
    Acting,
    Ending
}

public enum Unit
{
    solider,
    artillery,
    armoredCar,
    selfPropelledGun,
    lightTank,
    heavyTank
}

public class GameState
{
    public WarState WarState{get; private set;}
    public TurnState TurnState{get; private set;}
    public PlayerState RedState{get; private set;}
    public PlayerState BlueState{get; private set;}
    public BoardState BoardState{get; private set;}
    [SerializeField] private UnitDefinition[] unitDefinitions;
    [SerializeField] private FacilityDefinition[] facilityDefinitions;



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

    private UnitDefinition GetUnitDefinition(Unit id)
    {
        foreach (UnitDefinition definition in unitDefinitions)
        {
            if (definition != null && definition.Id == id)
                return definition;
        }

        throw new InvalidOperationException(
            $"UnitDefinitionが登録されていません: {id}");
    }
    private FacilityDefinition GetFacilityDefinition(string id)
    {
        foreach (FacilityDefinition definition in facilityDefinitions)
        {
            if (definition != null && definition.Id == id)
                return definition;
        }

        throw new InvalidOperationException(
            $"FacilityDefinitionが登録されていません: {id}");
    }


    public GameState(Tilemap tile)
    {
        WarState = WarState.Prepareing;
        TurnState = new TurnState();
        RedState = new PlayerState(PlayerSide.Red);
        BlueState = new PlayerState(PlayerSide.Blue);
        BoardState = new BoardState(tile);
    }

    public void MountUnit(string mountId,string viecleId)
    {
        UnitState mountUnit = BoardState.GetUnitById(mountId);
        UnitState viecleUnit = BoardState.GetUnitById(viecleId);

        viecleUnit.Mount(mountUnit);
        BoardState.RemoveUnit(mountUnit);

    }

    public void DismountUnit(string mountId,string viecleId,Vector3Int direction)
    {
        UnitState viecleUnit = BoardState.GetUnitById(viecleId);
        UnitState mountUnit = viecleUnit.GetMountedUnitById(mountId);

        Vector3Int pos = viecleUnit.Position+direction;
        BoardState.AddUnit(mountUnit.Definition,viecleUnit.Owner,pos);
        viecleUnit.Dismount(mountUnit);
    }

    public void AddUnit(Unit id,PlayerSide player,Vector3Int pos)
    {
        UnitDefinition def = GetUnitDefinition(id);
        BoardState.AddUnit(def,player,pos);
    }

    public void ChangePhase()
    {
        TurnState.ChangePhase();
        if(TurnState.Phase == TurnPhase.Starting)
        {
            TurnState.ChangeTurn();
            for(int i =0;i < BoardState.Units.Count; i++)
            {
                BoardState.Units[i].Reset();
            }
        }
    }




}

public class PlayerState
{
    public PlayerSide Side {get;}
    public int Money {get; private set;}
    
    public PlayerState(PlayerSide playerSide)
    {
        Side = playerSide;
        Money = 0;
    }

    public void AddMoney(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        Money = Money + amount;
    }

    public bool SpendMoney(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        if (Money < amount)
            return false;
        
        Money -=amount;
        return true;
        
    }
}

public class TurnState{
    public int Turn{get;private set;}
    public TurnPhase Phase{get;private set;}
    public PlayerSide Player{get;private set;}

    public TurnState()
    {
        Turn = 0;
        Player = PlayerSide.Red;
        Phase = TurnPhase.Starting;
    }

    public void ChangePhase()
    {
        switch (Phase)
        {
            case TurnPhase.Starting:
                Phase = TurnPhase.Acting;
                break;
            case TurnPhase.Acting:
                Phase = TurnPhase.Ending;
                break;
            case TurnPhase.Ending:
                Phase = TurnPhase.Starting;
                break;
        }
    }

    public void ChangeTurn()
    {
        Turn+=1;

        if (Player == PlayerSide.Red)
        {
            Player = PlayerSide.Blue;
        }
        else
        {
            Player = PlayerSide.Red;
        }
    }
}

public class BoardState
{
    public Tilemap Map{get;private set;}
    public List<UnitState> Units{get;}
    public List<FacilityState> Facilities{get;}
    public event Action UnitUpdated;

    public BoardState(Tilemap tmap)
    {
        Map = tmap;
    }

    public void AddUnit(UnitDefinition def,PlayerSide player,Vector3Int position)
    {
        UnitState newUnit = new UnitState(UuidGenerator.GenerateCompact(),def,player,position);
        Units.Add(newUnit);
        UnitUpdated?.Invoke();
    }

    public void AddUnit(UnitState unit,Vector3Int position)
    {
        unit.SetUnitPosition(position);
        Units.Add(unit);
        UnitUpdated?.Invoke();
    }

    public void RemoveUnit(UnitState unit)
    {
        Units.Remove(unit);
        UnitUpdated?.Invoke();
    }

    public void MoveUnit(string unitId,Vector3Int direction)
    {
        UnitState unit = GetUnitById(unitId);
        unit.MoveUnit(direction);        
        UnitUpdated?.Invoke();

    }

    public UnitState GetUnitById(string unitId)
    {
        for(int i = 0; i < Units.Count; i++)
        {
            if(Units[i].Id == unitId)
            {
                return Units[i];
            }
        }
        throw new InvalidOperationException(
            $"UnitStateが登録されていません: {unitId}");
        
    }

}

public class UnitState
{
    public string Id{get;private set;}
    public Vector3Int Position{get;private set;}
    public UnitDefinition Definition { get; }

    public int Health{get;private set;}
    public int MovementRange{get;private set;}
    public PlayerSide Owner{get;private set;}
    public bool IsAlreadyMoved{get;private set;}
    public List<UnitState> MountedUnit{get;private set;}


    public UnitState(
        string unitId,
        UnitDefinition definition,
        PlayerSide owner,
        Vector3Int position)
    {
        if (definition == null)
            throw new System.ArgumentNullException(nameof(definition));

        Id = unitId;
        Definition = definition;
        Owner = owner;
        Position = position;
        Health = definition.MaxHealth;
        MovementRange = definition.MovementRange;
        IsAlreadyMoved = false;
    }


    public bool TakeDamage(int dmg)
    {
        if (dmg < 0)
            throw new ArgumentOutOfRangeException(nameof(dmg));

        Health = Math.Max(0, Health - dmg);
        return Health == 0;
    }

    public void MarkMoved()
    {
        if (!IsAlreadyMoved)
        {
            IsAlreadyMoved = true;
        }
        else
        {
            throw new InvalidOperationException(nameof(IsAlreadyMoved));
        }
            
    }

    public void Mount(UnitState unit)
    {
        if(MountedUnit.Count < Definition.MountableAmount){
            if (Definition.MountableType.Contains(unit.Definition.Id))
            {
                MountedUnit.Add(unit);
            }
            else
            {
                throw new InvalidOperationException(nameof(unit.Definition.Id));
            }
        }
        else
        {
            throw new InvalidOperationException(nameof(unit));
        }
    }
    public void Dismount(UnitState dismountedUnit)
    {
        MountedUnit.Remove(dismountedUnit);
    }

    public void MoveUnit(Vector3Int vector)
    {
        if (MovementRange >= 0)
        {
            Position += vector;
            MovementRange --;            
        }

    }

    public void SetUnitPosition(Vector3Int position)
    {
        Position = position;
    }

    public UnitState GetMountedUnitById(string id)
    {
        for(int i=0 ; i < MountedUnit.Count; i++)
        {
            if (MountedUnit[i].Id == id)
            {
                return MountedUnit[i];
            }
        }
        throw new InvalidOperationException(
            $"UnitStateが登録されていません: {id}");
    }

    public void Reset()
    {
        MovementRange = Definition.MovementRange;
        IsAlreadyMoved = false;
    }
}

public class FacilityState
{
    public string FacilityId{get;private set;}
    public Vector3Int Position{get;private set;}
    public int Durability{get;private set;}
    public FacilityDefinition Definition{get;private set;}
    public PlayerSide Owner{get;private set;}

    public FacilityState(
        string facilityId,
        FacilityDefinition definition,
        Vector3Int position,
        PlayerSide owner
    )
    {
        FacilityId = facilityId;
        Definition = definition;
        Durability = Definition.MaxDurability;
        Owner = owner;
        Position = position;
        
    }

    public void ApplyOccupation(int dmg)
    {
        if (dmg < 0)
            throw new ArgumentOutOfRangeException(nameof(dmg));

        if (Durability > dmg)
        {
            Durability -= dmg;
        }
        else
        {
            Durability = 0;
            ChangeOwner();
        }

    }

    private void ChangeOwner()
    {
        if(Owner == PlayerSide.Red)
        {
            Owner = PlayerSide.Blue;
        }
        else
        {
            Owner = PlayerSide.Red;
        }
    }
}
