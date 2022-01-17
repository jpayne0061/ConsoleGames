using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAnimations
{
    class Program
    {
        const int MAX_LINES =      1000;
        const string HIGH_SCORE_FILE_NAME = "high_scores.txt";
        const int NUM_TOP_SCORES = 5;
        const char PLAYER_CHAR = '*';
        const int BOUNDARY_START = 3;
        const int BOUNDARY_END =   15;

        static char[] GAME_STATE_LINE = new char[52];
        static int PLAYER_POSITION = (BOUNDARY_START + BOUNDARY_END) / 2;
        static int SCORE = 0;
        static int CURRENT_LINE_COUNT = 0;
        static int GAME_SPEED = 300;
        static int GAME_SPEED_INCREASE = 20;
        static int GAME_SPEED_INCREASE_INTERVAL = 40;

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (!File.Exists(HIGH_SCORE_FILE_NAME))
            {
                File.WriteAllText(HIGH_SCORE_FILE_NAME, string.Empty);
            }

            StartGame();
        }

        static void StartGame()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Stay in the Lines! Hit 'Enter' to start the game");
            Console.ResetColor();

            var ci = Console.ReadKey();

            while (ci.Key != ConsoleKey.Enter)
            {
                ci = Console.ReadKey();
            }

            Console.ResetColor();

            Task.Factory.StartNew(() => Gammit());

            PlayerControlLoop();
        }

        static void PlayerControlLoop()
        {
            while (true)
            {
                if (CURRENT_LINE_COUNT > MAX_LINES)
                {
                    ScorePlayer();
                    break;
                }

                ConsoleKeyInfo ci = Console.ReadKey();

                if (ci.Key == ConsoleKey.LeftArrow)
                {
                    GAME_STATE_LINE[PLAYER_POSITION] = ' ';
                    PLAYER_POSITION -= 1;
                }

                if (ci.Key == ConsoleKey.RightArrow)
                {
                    GAME_STATE_LINE[PLAYER_POSITION] = ' ';
                    PLAYER_POSITION += 1;
                }

                if (PLAYER_POSITION > 18)
                {
                    PLAYER_POSITION = 18;
                }

                if (PLAYER_POSITION < 1)
                {
                    PLAYER_POSITION = 0;
                }

                GAME_STATE_LINE[PLAYER_POSITION] = PLAYER_CHAR;
            }

        }

        static void InitializeGameStateLine(int beginBoundary, int endBoundary)
        {
            for (int i = 0; i < GAME_STATE_LINE.Length - 10; i++)
            {
                if (i == beginBoundary || i == endBoundary)
                {
                    GAME_STATE_LINE[i] = '\\';
                }
                else if (i < 42)
                {
                    GAME_STATE_LINE[i] = ' ';
                }
            }

            GAME_STATE_LINE[PLAYER_POSITION] = '*';

            GAME_STATE_LINE[42] = 'S';
            GAME_STATE_LINE[43] = 'C';
            GAME_STATE_LINE[44] = 'O';
            GAME_STATE_LINE[45] = 'R';
            GAME_STATE_LINE[46] = 'E';
            GAME_STATE_LINE[47] = ':';
            GAME_STATE_LINE[48] = ' ';
            GAME_STATE_LINE[49] = '0';
        }

        static void Gammit()
        {
            int beginBoundary = BOUNDARY_START;
            int endBoundary = BOUNDARY_END;

            InitializeGameStateLine(beginBoundary, endBoundary);

            Func<int, int> gammitDirection = Right;

            Random r = new Random();

            while (true)
            {
                
                if(CURRENT_LINE_COUNT > MAX_LINES)
                {
                    break;
                }

                if(CURRENT_LINE_COUNT % GAME_SPEED_INCREASE_INTERVAL == 0)
                {
                    GAME_SPEED -= GAME_SPEED_INCREASE;
                }

                if (PLAYER_POSITION >= beginBoundary && PLAYER_POSITION <= endBoundary)
                {
                    SCORE += 1;
                }
                else
                {
                    GameOver(beginBoundary, endBoundary);
                    break;
                }

                char boundary_char = gammitDirection == Left ? '/' : '\\';

                WriteGameLine(beginBoundary, endBoundary);

                if (endBoundary > 25)
                {
                    gammitDirection =  Left;
                }
                else if(beginBoundary < 2)
                {
                    gammitDirection = Right;
                }
                else if(r.Next(2) == 1 && CURRENT_LINE_COUNT % 8 == 0)
                {
                    gammitDirection = gammitDirection == Left ? Right : Left;
                }

                GAME_STATE_LINE[endBoundary] = ' ';
                GAME_STATE_LINE[beginBoundary] = ' ';

                endBoundary  = gammitDirection(endBoundary);
                beginBoundary = gammitDirection(beginBoundary);

                boundary_char = gammitDirection == Left ? '/' : '\\';


                GAME_STATE_LINE[endBoundary] = boundary_char;
                GAME_STATE_LINE[beginBoundary] = boundary_char;

                string score = SCORE.ToString();

                GAME_STATE_LINE[49] = score[0];
                GAME_STATE_LINE[50] = score.Length >= 2 ? score[1] : ' ';
                GAME_STATE_LINE[51] = score.Length >= 3 ? score[2] : ' ';

                GAME_STATE_LINE[PLAYER_POSITION] = PLAYER_CHAR;

                Thread.Sleep(GAME_SPEED);
                
                CURRENT_LINE_COUNT++;
            }

            Console.ResetColor();
        }

        static void GameOver(int beginBoundaryIndex, int endBoundaryIndex)
        {

            for (int i = 0; i < 10; i++)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                WriteGameLine(beginBoundaryIndex, endBoundaryIndex, endGame: true);
                Thread.Sleep(100);
                Console.SetCursorPosition(Console.CursorLeft - 52, Console.CursorTop);
                Console.WriteLine("                                                    ");
                Thread.Sleep(100);
            }

            ScorePlayer();
        }

        static void ScorePlayer()
        {
            List<string> highScores = File.ReadAllLines(HIGH_SCORE_FILE_NAME).ToList();

            bool isHighScore = IsHighScore(highScores);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            if (isHighScore)
            {
                Console.WriteLine("You got a high score! Enter your name and hit 'ENTER'");

                string name = Console.ReadLine();

                name = name.Length > 10 ? name.Substring(0, 10) : name.PadRight(10);

                highScores.Add(name + $": {SCORE}");
            }

            highScores = highScores.OrderByDescending(x => int.Parse(x.Split(':')[1].Trim())).Take(NUM_TOP_SCORES).ToList();

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("        HIGH SCORES      ");
            Console.ResetColor();

            foreach (var item in highScores)
            {
                Console.WriteLine("        " + item);
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine($"Your score: {SCORE}");

            File.WriteAllLines(HIGH_SCORE_FILE_NAME, highScores);

            Console.ReadLine();
        }

        static bool IsHighScore(IEnumerable<string> highScores)
        {
            int[] scores = highScores.Select(x => int.Parse(x.Split(':')[1].Trim())).OrderBy(x => x).ToArray();
                
            if(scores != null && scores.Length > NUM_TOP_SCORES - 1)
            {
                return SCORE > scores[0];
            }

            return true;
        }

        static void WriteGameLine(int beginBoundaryIndex, int endBoundaryIndex, bool endGame = false)
        {
            lock(GAME_STATE_LINE)
            {
                int playerPosition = PLAYER_POSITION; //player position used by other thread. store and operate on copy

                string gameStateLine = new string(GAME_STATE_LINE);

                int[] gamePieces = (new int[] { beginBoundaryIndex, endBoundaryIndex, playerPosition })
                                    .OrderBy(x => x).ToArray();

                Console.Write(gameStateLine.Substring(0, playerPosition));
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(PLAYER_CHAR);
                Console.ResetColor();

                if(!endGame)
                {
                    Console.WriteLine(gameStateLine.Substring(playerPosition + 1));
                }
                else
                {
                    Console.Write(gameStateLine.Substring(playerPosition + 1));
                }

                return;
            }
        }

        static int Right(int x)
        {
            return x + 1;
        }

        static int Left(int x)
        {
            return x - 1;
        }

    }
}
