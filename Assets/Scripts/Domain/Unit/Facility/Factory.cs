public class Factory : Facility
{
    public Factory(PlayerSide owner, HexPosition position)
        : base(FacilityType.factory, position, 1000, owner)
    {
    }
}