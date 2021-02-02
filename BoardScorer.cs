using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TileAuto
{
    public class BoardScorer
    {
        public double ScoreW = 1;
        public double MonoW = 6;
        public double CornerW = 4;
        public double SpaceW = 4;

        private readonly Point[] vectors = new Point[]
          {
              new Point(0,-1),//row up
              new Point(0,1),//row down
              new Point(1,0),//col right
              new Point(-1,0) //coll left,
          };

        public int ScoreBoard(ulong board)
        {
            int score = 0;
            for (int i = 0; i < 16; i++)
            {
                int value = BitShifter.GetNibbleFromIndex(i, board);
                if (value > 1)
                {
                    score += (value) * (1 << value);
                }

            }
            int mono = 0;
            int cornerTiles = 0;
            for (int i = 0; i < 16; i++)
            {
                var edges = GetEdgesFromId(i).ToArray();
                if (edges.Length == 2)
                {

                    cornerTiles += BitShifter.GetNibbleFromIndex(i, board);
                }
                     foreach (var e in edges)
                    {
                        mono += Math.Abs(BitShifter.GetNibbleFromIndex(i, board) - BitShifter.GetNibbleFromIndex(e, board));
                    }

            }
            int spaces = BitShifter.CountSpaces(board);
            var result = (int)(score * ScoreW - mono * MonoW + cornerTiles * CornerW + spaces * SpaceW);
            return result > 0 ? result : 0;
        }

        public IEnumerable<int> GetEdgesFromId(int id)
        {
            Point pos = new Point(id % 4, id / 4);

            foreach (var vector in vectors)
            {
                int x = pos.X + vector.X;
                int y = pos.Y + vector.Y;
                if (x >= 0 && x < 4 && y >= 0 && y < 4)
                {
                    yield return (y * 4 + x);
                }
            }

        }

    }
}
