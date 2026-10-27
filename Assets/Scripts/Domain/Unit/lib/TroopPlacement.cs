public abstract class TroopPlacement
{
    private TroopPlacement() { }

    public sealed class OnBoard : TroopPlacement
    {
        public HexPosition Position { get; }

        public OnBoard(HexPosition position)
        {
            Position = position;
        }
    }

    public sealed class Embarked : TroopPlacement
    {
        public string CarrierId { get; }

        public Embarked(string carrierId)
        {
            CarrierId = carrierId;
        }
    }
}