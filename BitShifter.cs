using System;
using System.Collections.Generic;
using System.Linq;

namespace TileAuto
{
    public static class BitShifter
    {
        public static bool CheckForAWin(ulong target)
        {
            //a winning tile has a value of 11 as 2^11=2048
            ulong mask = 0xBBBBBBBBBBBBBBBB;
            var masked = target ^ mask;//XOR sets any matching 0xB nibble to 0
            return CheckForASpace(masked);
        }

        private static ushort CleanRow(ushort row, Direction direction)
        {
            bool isShiftRt = direction == Direction.Right || direction == Direction.Down;
            ushort result = 0;
            ushort mask = 0xF;
            int shiftCount = 0;
            for (int i = 0; i < 4; i++)
            {
                ushort temp = ((ushort)(row & mask));
                result = (ushort)(result | (temp >> shiftCount));
                if (temp == 0)
                {
                    shiftCount += 4;
                }
                mask = (ushort)(mask << 4);
            }
            if (!isShiftRt && shiftCount != 0)
            {
                //pad the result with zeros on the right
                result = (ushort)(result << (shiftCount));
            }
            return result;
        }


        public static (ushort, int score) SlideLeft(IEnumerable<byte> bytes, Direction direction)
        {
            byte[] slideArray = bytes.ToArray();
            int slideScore = 0;
            for (int index = 0; index < 3; index++)
            {
                //step over empty tile
                if ((slideArray[index] != 0) && slideArray[index + 1] == 0)
                {
                    slideArray[index + 1] = slideArray[index];
                    slideArray[index] = 0;
                }
                if (slideArray[index] != 0 && slideArray[index] == slideArray[index + 1])
                {
                    slideArray[index + 1] = 0;
                    slideArray[index] += 1;
                    int newValue = slideArray[index];
                    slideScore += (int)1 << newValue;
                }
            }
            ushort rowAsShort = SetNibblesAsShort(slideArray);
            ushort cleanedRow = CleanRow(rowAsShort, direction);
            return (cleanedRow, slideScore);
        }

        public static (ushort, int score) SlideRight(IEnumerable<byte> bytes, Direction direction)
        {
            byte[] slideArray = bytes.ToArray();
            int slideScore = 0;
            for (int index = 3; index > 0; index--)
            {
                //step over empty tile
                if ((slideArray[index] != 0) && slideArray[index - 1] == 0)
                {
                    slideArray[index - 1] = slideArray[index];
                    slideArray[index] = 0;
                }
                if (slideArray[index] != 0 && slideArray[index] == slideArray[index - 1])
                {
                    slideArray[index - 1] = 0;
                    slideArray[index] += 1;
                    int newValue = slideArray[index];
                    slideScore += (int)1 << newValue;
                }
            }
            ushort rowAsShort = SetNibblesAsShort(slideArray);
            ushort cleanedRow = CleanRow(rowAsShort, direction);
            return (cleanedRow, slideScore);
        }
        private static ushort SetNibblesAsShort(byte[] bytes)
        {
            ushort target = 0;
            int shiftCount = 0;
            for (int i = 3; i >= 0; i--)
            {
                target = (ushort)(target | ((ushort)(bytes[i] << shiftCount)));
                shiftCount += 4;
            }
            return target;
        }

        //private static byte SumNibles(byte a, byte b)
        //{
        //    if (b == 0)
        //        return a;

        //    return SumNibles((byte)(a ^ b), (byte)((a & b) << 1));
        //}

        public static IEnumerable<byte> GetNibblesFromRow(int row, ulong target)
        {
            // byte[] bytes = new byte[4];
            int shiftCount = row * 16;
            for (int i = 0; i < 4; i++)
            {
                ulong mask = 0xF000000000000000 >> shiftCount;
                yield return (byte)((target & mask) >> (60 - shiftCount));
                shiftCount += 4;
            }

        }

        private static bool IsMatchPossibleOnRows(ulong target)
        {
            byte[] bytes = new byte[4];
            for (int row = 0; row < 4; row++)
            {
                int shiftCount = row * 16;
                for (int i = 0; i < 4; i++)
                {
                    ulong mask = 0xF000000000000000 >> shiftCount;
                    bytes[i] = (byte)((target & mask) >> (60 - shiftCount));
                    shiftCount += 4;
                }
                for (int i = 0; i < 3; i++)
                {
                    if (bytes[i] == bytes[i + 1])
                        return true;
                }

            }
            return false;
        }


        public static bool IsMatchPossible(ulong target)
        {
            if (IsMatchPossibleOnRows(target)) return true;
            ulong transposed = TransposeRowsToCols(target);
            return IsMatchPossibleOnRows(transposed);
        }

        public static ulong SetNibbleFromIndex(int index, byte nibble, ulong target)
        {
            int shiftCount = index * 4;
            ulong mask = ~(0xF000000000000000 >> shiftCount);
            var cleared = target & mask;
            var x = ((ulong)nibble) << (60 - shiftCount);
            return cleared | x;
        }


        public static byte GetNibbleFromIndex(int index, ulong target)
        {

            int shiftCount = index * 4;
            ulong mask = (0xF000000000000000 >> shiftCount);
            var selected = target & mask;
            var x = selected >> (60 - shiftCount);
            return (byte)x;

        }

        public static int CountSpaces(ulong x)
        {        //clear bit 3 of the nibbles then add bits 0111 to each nibble 
                 //that results in bit 3 being set  if nibble bits 210 have any value>0
                 //OR in x which will set bit 3 if it was already set in the nibble so now all bits have been tested
            x = ((x & 0x7777777777777777) + 0x7777777777777777) | x;
            x = ~x & 0x8888888888888888;//invert x and mask all bits except bit3 of the nibble
            //Count the number of set bits
            int n;
            for (n = 0; x != 0; n++)
            {
                x &= x - 1;//clears a bit
            }
            return n;
        }

        public static bool CheckForASpace(ulong x)
        {
            ulong board = x;
            //mask out bit 3 of every nibble then  cause an overflow into bit 3 if any bit of bits 0,1,2 is set
            //then OR in the original status of bit 3. All bits have now been tested to see if they are set
            board = ((board & 0x7777777777777777) + 0x7777777777777777) | board;
            //bit 3 is now set if the nibble has a value. Invert the board and mask all bits apart from nibble bit 3
            board = ~board & 0x8888888888888888;
            //Now if bit 3 is set in any nibble, there is a space. All other bits have been cleared so just check for board !=0
            return (board != 0);
        }

        public static int[] IndexSpaces(ulong board)
        {
            List<int> spaces = new List<int>();
            ulong mask = 0x8000000000000000;
            board = ((board & 0x7777777777777777) + 0x7777777777777777) | board;
            board = ~board & 0x8888888888888888;
            for (int i = 0; i < 16; i++)
            {
                if ((board & mask) != 0)
                {
                    spaces.Add(i);
                }
                mask >>= 4;
            }
            return spaces.ToArray();
        }

        public static ulong TransposeRowsToCols(ulong board)
        {
            ulong noMove = board & 0xF0000F0000F0000F;//no move 0,5,10,15
            ulong moveRt3 = board & 0x0F0000F0000F0000;//moveRt3 nibbles 1,6,11
            ulong moveRt6 = board & 0x00F0000F00000000;//moveRT6: 2,7
            ulong moveRt9 = board & 0x000F000000000000;//moveRt9:  3
            ulong posVeMoves = noMove | moveRt3 >> 12 | moveRt6 >> 24 | moveRt9 >> 36;
            ulong moveLt3 = board & 0x0000F0000F0000F0;//moveLt3: 4,9,14 
            ulong moveLt6 = board & 0x00000000F0000F00;//moveLt6: 8,13
            ulong moveLt9 = board & 0x000000000000F000;//moveLt9: 12
            ulong negVeMoves = moveLt3 << 12 | moveLt6 << 24 | moveLt9 << 36;
            return posVeMoves | negVeMoves;
        }

    }
}