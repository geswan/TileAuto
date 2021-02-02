namespace TileAuto
{
    public class MoveLookAhead : IMoveSelector
    {
        private readonly Direction[] directions = new Direction[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right };
        private readonly BoardScorer boardScorer = new BoardScorer();

        public Direction GetBestMove(ulong board)
        {
            (int score, Direction direction) bestMove = (int.MinValue, Direction.Down);
            foreach (Direction direction in directions)
            {
                ulong clonedBoard = board;

                (int score, ulong nextBoard) = TestPossibleDirection(direction, clonedBoard);
                if (BitShifter.CheckForAWin(nextBoard))
                {
                    return direction;
                }
                if (nextBoard != clonedBoard)
                {
                    int moveScore = TestFollowingMove(nextBoard);
                    if (moveScore >= bestMove.score) bestMove = (moveScore, direction);
                }
            }
            return bestMove.direction;
        }

        private (int score, ulong board) TestPossibleDirection(Direction direction, ulong board)
        {
            return BoardSlider.SlideBoard(direction, board);
        }

        private int TestFollowingMove(ulong board)
        {

            var emptyTiles = BitShifter.IndexSpacesB(board);
            int evTotal = 0;
            foreach (int index in emptyTiles)
            {
                ulong testBoard = BitShifter.SetNibbleFromIndex(index, 2, board);
                (int score2, _, _) = TestAllPossibleMoves(testBoard);
                int evalA = (int)(0.1 * score2);
                testBoard = BitShifter.SetNibbleFromIndex(index, 1, board);
                (int score1, _, _) = TestAllPossibleMoves(testBoard);
                int evalB = (int)(0.9 * score1);
                evTotal += evalB + evalA;
            }
            return evTotal;
        }

        private (int score, Direction direction, ulong board) TestAllPossibleMoves(ulong board)
        {
            (int score, Direction direction, ulong bestBoard) bestMove = (int.MinValue, Direction.Down, board);
            foreach (Direction direction in directions)
            {
                (_, ulong testedBoard) = TestPossibleDirection(direction, board);
                int score = boardScorer.ScoreBoard(testedBoard);
                if (testedBoard != board && score >= bestMove.score) bestMove = (score, direction, testedBoard);
            }
            return bestMove;
        }

    }
}
