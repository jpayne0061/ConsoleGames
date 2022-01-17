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
        const int MAX_LINES = 50;
        const string HIGH_SCORE_FILE_NAME = "high_scores.txt";
        const int NUM_TOP_SCORES = 5;
        const char PLAYER_CHAR = '*';

        static char[] GAME_STATE_LINE = new char[52];
        static int PLAYER_POSITION = 5;
        static int SCORE = 0;
        static int CURRENT_LINE_COUNT = 0;

        static void Main(string[] args)
        {
            if (!File.Exists(HIGH_SCORE_FILE_NAME))
            {
                File.WriteAllText(HIGH_SCORE_FILE_NAME, string.Empty);
            }

            StartGame();
        }

        static void StartGame()
        {
            Console.WriteLine("Hit 'Enter' to start the game");

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

        static void Gammit()
        {
            int beginBoundary = 3;
            int endBoundary = 8;

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

            GAME_STATE_LINE[5] = '*';

            GAME_STATE_LINE[42] = 'S';
            GAME_STATE_LINE[43] = 'C';
            GAME_STATE_LINE[44] = 'O';
            GAME_STATE_LINE[45] = 'R';
            GAME_STATE_LINE[46] = 'E';
            GAME_STATE_LINE[47] = ':';
            GAME_STATE_LINE[48] = ' ';
            GAME_STATE_LINE[49] = '0';

            Func<int, int> currentOperator = Increment;

            while (true)
            {
                CURRENT_LINE_COUNT++;

                if(CURRENT_LINE_COUNT > MAX_LINES)
                {
                    break;
                }

                if (PLAYER_POSITION >= beginBoundary && PLAYER_POSITION <= endBoundary)
                {
                    SCORE += 1;
                }

                char boundary_char = currentOperator == Decrement ? '/' : '\\';

                if (beginBoundary == 1 || beginBoundary == 14)
                {
                    boundary_char = '|';
                }

                WriteGameLine(beginBoundary, endBoundary, boundary_char);

                if (endBoundary > 18)
                {
                    currentOperator =  Decrement;
                }
                else if(beginBoundary < 2)
                {
                    currentOperator = Increment;
                }

                GAME_STATE_LINE[endBoundary] = ' ';
                GAME_STATE_LINE[beginBoundary] = ' ';

                endBoundary  = currentOperator(endBoundary);
                beginBoundary = currentOperator(beginBoundary);

                boundary_char = currentOperator == Decrement ? '/' : '\\';


                GAME_STATE_LINE[endBoundary] = boundary_char;
                GAME_STATE_LINE[beginBoundary] = boundary_char;

                string score = SCORE.ToString();

                GAME_STATE_LINE[49] = score[0];
                GAME_STATE_LINE[50] = score.Length >= 2 ? score[1] : ' ';
                GAME_STATE_LINE[51] = score.Length >= 3 ? score[2] : ' ';

                GAME_STATE_LINE[PLAYER_POSITION] = PLAYER_CHAR;

                Thread.Sleep(100);
            }

            Console.ResetColor();
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

        static void WriteGameLine(int beginBoundaryIndex, int endBoundaryIndex, char boundaryChar)
        {
            lock(GAME_STATE_LINE)
            {
                int playerPosition = PLAYER_POSITION;

                string letters = new string(GAME_STATE_LINE);

                if (beginBoundaryIndex == 1 || beginBoundaryIndex == 14)
                {
                    letters = letters.Replace('\\', '|').Replace('/', '|');
                }

                int[] gamePieces = (new int[] { beginBoundaryIndex, endBoundaryIndex, playerPosition })
                                    .OrderBy(x => x).ToArray();

                if (PLAYER_POSITION >= beginBoundaryIndex && playerPosition <= endBoundaryIndex)
                {
                    Console.Write(letters.Substring(0, beginBoundaryIndex));

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(boundaryChar);
                    Console.ResetColor();

                    Console.Write(letters.Substring(beginBoundaryIndex + 1, Math.Max(playerPosition - beginBoundaryIndex - 1, 0)));
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(PLAYER_CHAR);
                    Console.ResetColor();

                    Console.Write(letters.Substring(PLAYER_POSITION + 1, Math.Max(Math.Min(endBoundaryIndex - playerPosition - 1, 3), 0)));
                    Console.ResetColor();

                    if (PLAYER_POSITION != endBoundaryIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write(boundaryChar);
                        Console.ResetColor();
                    }

                    Console.WriteLine(letters.Substring(endBoundaryIndex + 1));
                    Console.ResetColor();
                }
                else
                {
                    Console.Write(letters.Substring(0, playerPosition));
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(PLAYER_CHAR);
                    Console.ResetColor();
                    Console.WriteLine(letters.Substring(playerPosition + 1));
                }
            }
        }

        static int Increment(int x)
        {
            return x + 1;
        }

        static int Decrement(int x)
        {
            return x - 1;
        }

    }
}
