using System;

namespace TileAuto
{


    public class TileUpdatedEventArgs : EventArgs
    {
        public TileUpdatedEventArgs(int tileId, int tileValue)
        {
            TileID = tileId;
            TileValue = tileValue;

        }

        public int TileID { get; }
        public int TileValue { get; }

    }
}
