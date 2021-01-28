using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TileAuto
{
    public class MinMax
    {
        private Direction[] directions = (Direction[])Enum.GetValues(typeof(Direction));
        public double ScoreW = 0;
        public double MonoW = 0;
        public double CornerW = 0;
        private GameEngine gameEngine = new GameEngine();
        public Direction GetBestMoveDirection(TileCollectionB board)
        {
           // TileCollectionB clone = board.Clone();
            gameEngine.Reset(board);
            (int score, Direction direction) bestMove = (int.MinValue, Direction.Down);
            foreach (Direction direction in directions)
            {
                 TestMove(direction);
                 if (gameEngine.HasBoardChanged())
                {
                    gameEngine.UpdateAllTiles(false);
                    int expectedScore = ScoreBestFollowingMove();
                    if (expectedScore >= bestMove.score)
                    {
                        bestMove.score = expectedScore;
                        bestMove.direction = direction;
                    }
                  
                }
                gameEngine.Reset(board);


            }
            return bestMove.direction;
        }
        private int ScoreBestFollowingMove()
        {
            var tileCollection = gameEngine.GetBoard();
            var emptyTiles = tileCollection.GetEmptyTileIndices().ToArray();
            int evTotal = int.MinValue;
           
            foreach (int index in emptyTiles)
            {

                gameEngine.GetBoard()[index] = 2;//4
                gameEngine.UpdateAllTiles(false);
                int evalA = (int)(0.1 * ScoreFollowingMove());
                gameEngine.Reset(tileCollection);
                gameEngine.GetBoard()[index] = 1;
                gameEngine.UpdateAllTiles(false);
                int evalB = (int)(0.9 * ScoreFollowingMove());
                evTotal+= evalB + evalA;
                gameEngine.Reset(tileCollection);
            }
            return evTotal;
        }



        private int ScoreFollowingMove()
        {
            (int score, Direction direction) bestMove = (int.MinValue, Direction.Down);
            var tileCollection = gameEngine.GetBoard();
            foreach (Direction direction in directions)
            {
                int slideScore = TestMove(direction);
                //if nothing changed
                int moveScore = gameEngine.HasBoardChanged() ? ScoreBoard():int.MinValue;
                if (moveScore > bestMove.score) bestMove = (moveScore, direction);
                gameEngine.Reset(tileCollection);
            }
            return bestMove.score;
        }


        public void SetWeightings(double score, double mono, double corner)
        {
            ScoreW = score;
            MonoW = mono;
            CornerW = corner;
        }



        public int TestMove(Direction direction)
        {
            return direction switch
            {
                Direction.Down => SlideAllColumns(direction),
                Direction.Up => SlideAllColumns(direction),
                Direction.Left => SlideAllRows(direction),
                Direction.Right => SlideAllRows(direction),
                _ => throw new NotImplementedException()
            };
        }

        public int ScoreBoard()
        {
            var tileCollection = gameEngine.GetBoard();
            int score = 0;
            for (int i = 0; i < 16; i++)
            {
                int value = tileCollection[i];
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
                    cornerTiles += tileCollection[i];
                }

                foreach (var e in edges)
                {
                     mono += Math.Abs(tileCollection[i] - tileCollection[e]);
                }

            }
            var result = (int)(score * ScoreW - mono * MonoW + cornerTiles * CornerW);
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
        private readonly List<Point> vectors = new List<Point>
          {
              new Point(0,-1),//row up
              new Point(0,1),//row down
              new Point(1,0),//col right
              new Point(-1,0) //coll left,
          };

        public int SlideAllColumns(Direction direction)
        {

         return   gameEngine.SlideAllColumns(direction);
         
        }

        public int SlideAllRows(Direction direction)
        {
            return gameEngine.SlideAllRows(direction);
        }

        //private int SlideRow(int row, Direction direction, TileCollectionB board)
        //{
        //    var list = board.Select(v => v).Where((v, i) => i / 4 == row && v != 0).ToList();
        //    var (rowAsList, score) = Slide(list, direction);
        //    int i = 0;
        //    //transfer to tileCollection
        //    foreach (int n in rowAsList)
        //    {
        //        board[(i, row)] = n;
        //        i++;
        //    }

        //    return score;
        //}

        //private (List<int>, int) Slide(List<int> list, Direction direction)
        //{
        //    bool isReversed = direction == Direction.Right || direction == Direction.Down;
        //    if (isReversed) list.Reverse();
        //    int slideScore = 0;
        //    //slide matching values together
        //    for (int index = 0; index < list.Count - 1; index++)
        //    {
        //        if (list[index] == list[index + 1])
        //        {

        //            list[index + 1] = 0;

        //            list[index] += 1;
        //            //slideScore += (int)Math.Pow(2, list[index]);
        //            slideScore += list[index];
        //        }
        //    }
        //    //No spaces allowed between values so remove the blanks created by combining values
        //    var contiguousValues = list.Select((i) => i).Where((i) => i != 0).ToList();
        //    //add zeros to pad the row up to the row length
        //    contiguousValues.AddRange(Enumerable.Repeat(0, 4 - contiguousValues.Count));
        //    if (isReversed) contiguousValues.Reverse();
        //    return (contiguousValues, slideScore);
        //}
    }
}
