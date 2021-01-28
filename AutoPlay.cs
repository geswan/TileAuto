using System;
using System.Threading.Tasks;

namespace TileAuto
{
    public class AutoPlay
    {
        private readonly GameEngine gameEngine = new GameEngine();
        private readonly MinMaxB minMax = new MinMaxB();
        public void Play()
        {
            (int games, int total, int wins) = StartTrial(1000);
            Console.WriteLine($"Games {games} Wins {wins} Total {total}");
            Console.ReadLine();
        }

        public (int games, int total, int wins) StartTrial(int runs)
        {
            int games = 0;
            int total = 0;
            int wins = 0;
            while (games < runs)
            {
                gameEngine.Reset();
                gameEngine.AddNewTilesToCollection(2);
                bool isRunning = true;
                while (isRunning)
                {
                    var direction = minMax.GetBestMove(gameEngine.GetBoardTiles());
                    int score = PlayMove(direction);
                    isRunning = gameEngine.CompleteMove();
                    total += score;

                }
                if (gameEngine.IsWinner)
                {
                    wins++;
                }
                games++;
            }
            return (games, total, wins);
        }

        public int PlayMove(Direction direction)
        {
            return gameEngine.SlideBoard(direction);

        }

        public async Task PlayGameAsync()
        {
            Console.Clear();
            Console.WriteLine(" Press any key to end.");
            int firstCol = Console.CursorTop;
            int firstRow = Console.CursorLeft + 1;
            Console.CursorVisible = false;
            gameEngine.Reset();
            gameEngine.AddNewTilesToCollection(2);
            bool isRunning = true;
            int total = 0;
            ShowTiles(total, firstRow, firstCol);
            await Task.Delay(350);
            while (isRunning && !Console.KeyAvailable)
            {
                var direction = minMax.GetBestMove(gameEngine.GetBoard().GetTiles());
                int score = PlayMove(direction);
                isRunning = gameEngine.CompleteMove();
                total += score;
                ShowTiles(total, firstRow, firstCol);
                await Task.Delay(100);
            }
            if (isRunning) Console.ReadKey(true);
            Console.CursorVisible = true;
            string result = gameEngine.IsWinner ? " You Win!" : " Game Over";
            Console.WriteLine();
            Console.WriteLine(result);
        }
        private void ShowTiles(int score, int topRow, int topCol)
        {
            Console.SetCursorPosition(topRow, topCol);
            Console.WriteLine($"Score {score}");
            //    Console.Clear();
            topRow += 2;
            //   Console.WriteLine(score);
            //  Console.WriteLine();
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    int value = gameEngine.GetBoard()[(x, y)];
                    int points = value == 0 ? 0 : (1 << value);
                    string rightAligned5 = string.Format("{0,5}", points);
                    Console.SetCursorPosition(topCol + (x * rightAligned5.Length), topRow + y);
                    Console.Write(rightAligned5);
                }

            }

        }

        public void PlayGameManualMove()
        {
            Console.Clear();
            Console.WriteLine("Key W=Up S=Down A=Left D=Right Q=Quit.");
            int firstCol = Console.CursorTop;
            int firstRow = Console.CursorLeft + 1;
            Console.CursorVisible = false;
            gameEngine.Reset();
            gameEngine.AddNewTilesToCollection(2);
            bool isRunning = true;
            int total = 0;
            ShowTiles(total, firstRow, firstCol);
            while (isRunning)
            {
                Console.CursorVisible = false;//resizing window sets it
                var direction = ChooseMove();
                if (direction == Direction.UnKnown) break;
                int score = PlayMove(direction);
                isRunning = gameEngine.CompleteMove();
                total += score;
                ShowTiles(total, firstRow, firstCol);
            }
            Console.CursorVisible = true;
            string result = gameEngine.IsWinner ? " You Win!" : " Game Over";
            Console.WriteLine();
            Console.WriteLine(result);
        }
        private static Direction ChooseMove()
        {
            char choice = char.ToUpper(Console.ReadKey(true).KeyChar);
            return choice switch
            {
                'W' => Direction.Up,
                'S' => Direction.Down,
                'A' => Direction.Left,
                'D' => Direction.Right,
                'Q' => Direction.UnKnown,
                _ => Direction.Up
            };
        }

    }
}
