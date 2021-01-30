using System;

namespace TileAuto
{
    [Flags]
    public enum Direction
    {
        Up,
        Left,
        Down,
        Right,
        UnKnown
    }
    public class GameEngine
    {
        private TileCollectionB tileCollection = new TileCollectionB();
        public bool IsWinner = false;
        public TileCollectionB GetBoard()
        {
            return tileCollection.Clone();
        }
        public void Reset()
        {
            tileCollection.Clear();
            IsWinner = false;
        }

        public void Reset(TileCollectionB parentCollection)
        {
            tileCollection = parentCollection.Clone();
            IsWinner = false;
        }
        //returns false if the move cannot be continued
        public bool CompleteMove()
        {
            if (!tileCollection.HasBoardChanged())
            {
                return true;//do nothing
            }
            int tileId = tileCollection.GetNewTileId();
            var newtileValue = tileCollection.GetRandomTileValue();
            tileCollection[tileId] = newtileValue;
            UpdateAllTiles(false);
            if (tileCollection.CheckForAWin())
            {
                IsWinner = true;
                return false;
            }
            //if all tiles are occupied, check that there is still a possible match
            if (tileCollection.GetIsCollectionFull())
            { //It's game over if there's not a match.
                return IsAMatchPossible(tileCollection);
            }
            return true;
        }

        public bool HasBoardChanged()
        {
            return tileCollection.HasBoardChanged();
        }

        private static bool IsAMatchPossible(TileCollectionB board)
        {
            ulong tiles = board.GetTiles();
            return BitShifter.IsMatchPossible(tiles);
        }

        public void AddNewTilesToCollection(int count)
        {
            for (int i = 0; i < count; i++)
            {
                int newTileId = tileCollection.GetNewTileId();
                var newtileValue = tileCollection.GetRandomTileValue();
                tileCollection[newTileId] = newtileValue;
            }
        }
        public bool UpdateAllTiles(bool isRaiseEvent)
        {
            tileCollection.SaveBoard();
            if (!isRaiseEvent)
            {
                return true;
            }

            int i = 0;
            foreach (int v in tileCollection)
            {
                int value = v;
                int id = i;
                var args = new TileUpdatedEventArgs(id, value);
                RaiseTileUpdatedEvent(args);
                i++;
            }

            return true;
        }

        public int SlideBoard(Direction direction)
        {
            ulong target = tileCollection.GetTiles();
            (int score, ulong slidBoard) = BoardSlider.SlideBoard(direction, target);
            tileCollection.SetBoard(slidBoard);
            return score;
        }

        public void SetBoardTiles(ulong tiles)
        {
            tileCollection.SetBoard(tiles);
        }

        public ulong GetBoardTiles()
        {
            return tileCollection.GetTiles();
        }
        public delegate void TileUpdatedHandler(object sender, TileUpdatedEventArgs e);
        public event TileUpdatedHandler TileUpdatedEvent;
        private void RaiseTileUpdatedEvent(TileUpdatedEventArgs args)
        {
            TileUpdatedEvent?.Invoke(this, args);
        }
    }
}
