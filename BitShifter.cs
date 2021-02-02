using System.Collections.Generic;

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


        public static (ushort, int score) SlideLeftB(ushort test, Direction direction)
        {
            ushort row = test;
            int slideScore = 0;
            int slideRt = 0;
            int slideLt = 12;
            ushort mask = 0xF000;
            ushort result = 0;
            for (int i = 0; i < 3; i++)
            {
                //get the 2 bytes to compare from the row
                ushort temp = (ushort)(row & (mask >> slideRt));
                byte a = (byte)(temp >> (12 - slideRt));
                slideRt += 4;
                temp = (ushort)(row & (mask >> slideRt));
                byte b = (byte)(temp >> (12 - slideRt));
                //step over a zero
                if (a != 0 && b == 0)
                {
                    //update the row, make the b byte= to the a byte
                    //no need for a mask as b byte was already zero
                    row = (ushort)(row | (ushort)(a << (12 - slideRt)));
                    a = 0;
                }
                //check for a match
                if (a != 0 && a == b)
                {

                    //set the  b byte to zero on the row to prevent it being matched again
                    row = (ushort)(row & (ushort)~(mask >> slideRt));
                    a += 1;
                    slideScore += 1 << a;
                }
                if (a != 0)
                {
                    //update the result
                    result = (ushort)(result | (a << slideLt));
                    slideLt -= 4;
                }
            }
            //Finally,update the result with the last byte in 'row'
            result = (ushort)(result | (ushort)(row & 0x000F) << slideLt);
            return (result, slideScore);
        }


        public static (ushort, int score) SlideRightB(ushort test, Direction direction)
        {
            ushort row = test;
            int slideScore = 0;
            int slideRt = 0;
            int slideLt = 0;
            ushort mask = 0x000F;
            ushort result = 0;
            for (int i = 3; i > 0; i--)
            {
                //get the 2 bytes to compare from the row
                ushort temp = (ushort)(row & (mask << slideLt));
                byte a = (byte)(temp >> slideLt);
                slideLt += 4;
                temp = (ushort)(row & (mask << slideLt));
                byte b = (byte)(temp >> slideLt);
                //step over a zero
                if (a != 0 && b == 0)
                {
                    //update the row, make the b byte= to the a byte
                    //no need for a mask as b byte was already zero
                    row = (ushort)(row | (ushort)(a << slideLt));
                    a = 0;
                }
                //check for a match, ignore 0==0
                if (a != 0 && a == b)
                {
                    //set the  b byte to zero on the row to prevent it being matched again
                    row = (ushort)(row & (ushort)~(mask << slideLt));
                    a += 1;
                    slideScore += 1 << a;
                }
                if (a != 0)
                {
                    //update the result
                    result = (ushort)(result | (a << slideRt));
                    slideRt += 4;
                }
            }

            //Finally,update the result with the last byte in 'row'
            result = (ushort)(result | (ushort)(row & 0xF000) >> (12 - slideRt));
            return (result, slideScore);
        }


        public static IEnumerable<byte> GetNibblesFromRow(int row, ulong target)
        {
            int shiftCount = row * 16;
            for (int i = 0; i < 4; i++)
            {
                ulong mask = 0xF000000000000000 >> shiftCount;
                yield return (byte)((target & mask) >> (60 - shiftCount));
                shiftCount += 4;
            }

        }

        public static ushort GetRowAsShort(int row, ulong target)
        {
            int shiftCount = row * 16;
            ulong mask = 0xFFFF000000000000 >> shiftCount;
            return (ushort)((target & mask) >> (48 - shiftCount));

        }


        private static bool IsMatchPossibleOnRows(ulong target)
        {
            ushort mask = 0xF000;
            for (int i = 0; i < 4; i++)
            {
                var row = GetRowAsShort(i, target);
                int slideRt = 0;
                for (int n = 0; n < 3; n++)
                {

                    //get the 2 bytes to compare from the row
                    ushort temp = (ushort)(row & (mask >> slideRt));
                    byte a = (byte)(temp >> (12 - slideRt));
                    slideRt += 4;
                    temp = (ushort)(row & (mask >> slideRt));
                    byte b = (byte)(temp >> (12 - slideRt));
                    if (a == b)
                    {
                        return true;
                    }
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

        public static IEnumerable<int> IndexSpacesB(ulong board)
        {
            //avoid using an array
            ulong mask = 0x8000000000000000;
            board = ((board & 0x7777777777777777) + 0x7777777777777777) | board;
            board = ~board & 0x8888888888888888;
            for (int i = 0; i < 16; i++)
            {
                if ((board & mask) != 0)
                {
                    yield return i;
                }
                mask >>= 4;
            }

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