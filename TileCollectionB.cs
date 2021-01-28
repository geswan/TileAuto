using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TileAuto
{
    public class TileCollectionB : IEnumerable<int>
    {
        private ulong board = 0;
        private ulong backUp = 0;
        private readonly Random random = new Random();
        public TileCollectionB()
        {
            board = 0;
        }

        public TileCollectionB(ulong board, ulong backup = 0)
        {
            this.board = board;
            this.backUp = backup;
        }

        //indexer-enables tileCollection[(x,y)] access to tile values
        public int this[(int X, int Y) xy]
        {
            get => BitShifter.GetNibbleFromIndex(xy.Y * 4 + xy.X, board);
            set
            {
                board = BitShifter.SetNibbleFromIndex(xy.Y * 4 + xy.X, (byte)value, board);
            }
        }
        //indexer-enables tileCollection[i] access to tile values
        public int this[int i]
        {
            get => BitShifter.GetNibbleFromIndex(i, board);
            set
            {
                board = BitShifter.SetNibbleFromIndex(i, (byte)value, board);
            }
        }
        public ulong GetTiles()
        {
            return board;
        }

        public TileCollectionB Clone()
        {
            return new TileCollectionB(board, backUp);

        }

        public void Clear()
        {

            board = 0;
        }


        public void SaveBoard()
        {
            backUp = board;
        }
        public void SetBoard(ulong board)
        {
            this.board = board;
        }
        public void RestoreBoard()
        {
            board = backUp;
        }

        public IEnumerable<int> GetEmptyTileIndices()
        {
            return BitShifter.IndexSpaces(board);
        }

        public bool CheckForAWin()
        {
            return BitShifter.CheckForAWin(board);
        }

        public int GetNewTileId()
        {
            int[] emptyTileIndices = GetEmptyTileIndices().ToArray();
            if (emptyTileIndices.Length == 0) return -1;//no spaces
            int i = random.Next(0, emptyTileIndices.Length);
            return emptyTileIndices[i];
        }


        public int GetRandomTileValue()
        {
            double temp = random.NextDouble();
            //10:90 selection ratio between 2 and 1 equates to tile scores of 2*2 and 2
            return temp < 0.1 ? 2 : 1;
        }

        public bool GetIsCollectionFull()
        {
            return !BitShifter.CheckForASpace(board);
        }


        public bool HasBoardChanged()
        {
            return board != backUp;
        }

        //enables the use of foreach and Linq
        public IEnumerator<int> GetEnumerator()
        {

            for (int i = 0; i < 16; i++)
            {
                yield return BitShifter.GetNibbleFromIndex(i, board);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}