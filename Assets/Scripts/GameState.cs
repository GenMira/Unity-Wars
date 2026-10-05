using UnityEngine;

public class GameState
{
    public string WarState { get; set; }
    public struct TurnState{
        int turn;
        string TurnPlayer;
    }
    public struct PlayerState
    {
        int money;

    }
    public struct BoardState
    {
        //HexMap hexMap;
        UnitState[] unitStates;
        FacilityState[] facilityStates;
    }

    public struct UnitState
    {
        string unitId;
        Vector2 position;
        int health;
        bool isTurnEnded;
        MountedUnit mountedUnit;
    }

    public struct MountedUnit
    {
        string unitId;
        int health;
    }

    public struct FacilityState
    {
        string facilityId;
        Vector2 position;
        int durability;
        string owner;
    }

}
