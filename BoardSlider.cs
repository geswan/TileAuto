namespace TileAuto
{
    public static class BoardSlider
    {
        public static (int points, ulong board) SlideBoard(Direction direction, ulong target)
        {
            ulong testBoard = direction == Direction.Down || direction == Direction.Up ?
            BitShifter.TransposeRowsToCols(target) : target;
            int moveScore = 0;
            for (int i = 0; i < 4; i++)
            {
                (int score, ulong updatedBoard) = SlideRowB(direction, i, testBoard);
                testBoard = updatedBoard;
                moveScore += score;
            }
            ulong processedBoard = direction == Direction.Down || direction == Direction.Up ?
                   BitShifter.TransposeRowsToCols(testBoard) : testBoard;
            return (moveScore, processedBoard);
        }

        private static (int score, ulong board) SlideRowB(Direction direction, int rowIndex, ulong target)
        {
            var rowAsShort = BitShifter.GetRowAsShort(rowIndex, target);
            var (slidRow, score) = direction == Direction.Right || direction == Direction.Down ?
               BitShifter.SlideRightB(rowAsShort, direction) : BitShifter.SlideLeftB(rowAsShort, direction);
            int shiftCount = rowIndex * 16;
            ulong rowMask = 0xFFFF000000000000 >> shiftCount;
            ulong update = (ulong)slidRow << (48 - shiftCount);
            //OR in the updated row. (AND ~rowmask clears the old value)
            target = (target & ~rowMask) | update;
            return (score, target);
        }

    }
}