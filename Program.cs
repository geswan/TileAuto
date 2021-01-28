using System;
using System.Threading.Tasks;

namespace TileAuto
{
    public static class Program
    {
        public static async Task Main()
        {
            //Console.SetWindowSize(40, 15);
            AutoPlay autoPlay = new AutoPlay();
            //autoPlay.Play();
            //	autoPlay.PlayGameManualMove();
            await autoPlay.PlayGameAsync();
            Console.ReadLine();
        }
    }
}
