namespace TileAuto
{
    public interface IMoveSelector
    {
        Direction GetBestMove(ulong board);
    }
}