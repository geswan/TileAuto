using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TileAuto
{
    public class TileCollection : IEnumerable<int>
    {
        private readonly int[] tiles = new int[16];
        private readonly int[] backUp = new int[16];
        private readonly Random random = new Random();
        //indexer-enables tileCollection[(x,y)] access to tiles
        public int this[(int X, int Y) xy]
        {
            get => tiles[xy.Y * 4 + xy.X];
            set => tiles[xy.Y * 4 + xy.X] = value;
          
        }
        //indexer-enables tileCollection[i] access to tiles
        public int this[int i]
        {
            get => tiles[i];
            set => tiles[i] = value;
        }

        public TileCollection Clone()
        {
            var clone = new TileCollection();
            for(int i=0;i<16;i++)
            {
                clone[i] = tiles[i];
            }
            
            return clone;
        }

        public void Clear()
        {
          
          
                for (int i = 0; i < tiles.Length; ++i)
                {
                    tiles[i] = 0;
                backUp[i] = 0;
                }    
        }


        public void SaveBoard()
        {
            tiles.CopyTo(backUp, 0);
        }
        public void RestoreBoard()
        {
            backUp.CopyTo(tiles, 0);
        }

        public IEnumerable<int> GetEmptyTileIndices()
        {

            return tiles.Select((v, index) => index).Where((i) => tiles[i] == 0);
        }

        public bool CheckForAWin()
        {// a value of 11 gives a score of 2 to Power 11=2048
            return tiles.Contains(11);
        }
        public int GetNewTileId()
        {
            int[] emptyTileIndices = GetEmptyTileIndices().ToArray();
            if (emptyTileIndices.Length == 0) return -1;//no spaces
            Shuffle(emptyTileIndices);
            //just take it off the top as the list has been shuffled
            return emptyTileIndices[0];
        }

        public int GetRandomTileValue()
        {
            double temp = random.NextDouble();
            //10:90 selection ratio between 2 and 1 equates to tile scores of 2*2 and 2
            return temp < 0.1 ? 2 : 1;
        }

        public bool GetIsCollectionFull()
        {
            return !tiles.Where((v) => v == 0).Any();
        }


        // Fisher–Yates shuffle method
        private void Shuffle(int[] list)
        {
            int i = list.Length - 1;
            while (i > 0)//if i was allowed to be 0 here,[0] would swap with [0]
            {
                //k is chosen from a decreasing number of elements
                int k = random.Next(i + 1);//range is 0-i inclusive
                var temp = list[i];
                list[i] = list[k];
                list[k] = temp;
                i--;
            }
            // why this works
            // for a 3 element array, prob of number at [2] not being swapped is 1/3*1
            //for[1] it's 2/3 *1/2=1/3. It's the same for element[0]
            //so the probability of being swapped is the same for each element
            //where the element gets swapped to is random
        }
        //enables the use of foreach and Linq
        public IEnumerator<int> GetEnumerator()
        {
            //need to cast to a generic type
            return tiles.Cast<int>().GetEnumerator();
            //no need to call the Cast method if using a generic type 
            // like List<int>. Just write  return tiles.GetEnumerator();
            //foreach (int i in tiles)
            //{
            //    yield return i;
            //}
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
