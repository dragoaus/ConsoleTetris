using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.VisualBasic.CompilerServices;

namespace ConsoleTetris
{
    internal class Program
    {
        // Tetromino blocks
        private static string[] tetrominos = new string[7];
        
        private static int fieldWidth = 12;
        private static int fieldHeight = 18;
        private static int[] playingField = null;

        static int screenWidth = 80;
        static int screenHeight = 30;
        private static char[] screen = new char[screenWidth * screenHeight];

        static int currentPiece = 0;
        static int currentRotation = 0;
        static int currentX = (fieldWidth-3) / 2;
        static int currentY = 0;

        static bool rotateHold = false;

        static int speed = 20;
        static int speedCounter = 0;
        static bool forceDown = false;

        static List<int> lines = new List<int>();

        static int pieceCount = 0;
        private static int score = 0;
        static void Main(string[] args)
        {
            Console.CursorVisible = false;

            InitializeTetrominos();
            InitializePlayingField();
            GenerateNewTetromino();
            bool isGameOver = false;

            // main loop
            while (!isGameOver)
            {

                // Thread.CurrentThread.Join(50);
                speedCounter++;
                forceDown = (speedCounter == speed);

                // Draw Field into the screen
                DrawField();

                // Draw Current Piece into the screen
                DrawCurrentPiece();

                // Get User input
                UserInput();

                // Check if the piece can go stepdown
                if (forceDown)
                {
                    if (DoesPieceFit(currentPiece, currentRotation, currentX , currentY + 1))
                    {
                        currentY++;
                    }
                    else
                    {
                        for (int px = 0; px < 4; px++)
                        {
                            for (int py = 0; py < 4; py++)
                            {
                                if (tetrominos[currentPiece][Rotate(px,py,currentRotation)] == 'X')
                                {
                                    playingField[(currentY+py) * fieldWidth + (currentX + px)] = currentPiece + 1;
                                }
                            }
                        }

                        pieceCount++;
                        if (pieceCount %10 ==0)
                        {
                            if (speed >= 10)
                            {
                                speed--;
                            }
                        }

                        for (int py = 0; py < 4; py++)
                        {
                            if (currentY+py < fieldHeight-1)
                            {
                                bool line = true;
                                for (int px = 1; px < fieldWidth-1; px++)
                                {
                                    line &= playingField[(currentY + py) * fieldWidth + px] !=0;
                                }

                                if (line)
                                {
                                    for (int px = 1; px < fieldWidth-1; px++)
                                    {
                                        playingField[(currentY + py) * fieldWidth + px] = 8;
                                        
                                    }
                                    lines.Add(currentY + py);
                                }
                            }
                        }

                        score += 25;
                        if (lines.Any())
                        {
                            score += (1 << lines.Count) * 100;
                        }

                        //Checking is there any full lines
                        if (lines.Any())
                        {
                            DrawField();
                            DrawScreen();
                            Thread.CurrentThread.Join(500);
                            foreach (var element in lines)
                            {
                                for (int px = 1; px < fieldWidth - 1; px++)
                                {
                                    for (int py = element; py > 0; py--)
                                    {
                                        playingField[py * fieldWidth + px] = playingField[(py - 1) * fieldWidth + px];
                                    }
                                    playingField[px] = 0;
                                }
                            }

                            lines = new List<int>();
                        }
                        

                        // Generates new Playing Tetromino
                        GenerateNewTetromino();

                        isGameOver = !DoesPieceFit(currentPiece, currentRotation, currentX, currentY );
                    }
                    speedCounter = 0;
                }


                WriteScoreboard();
                DrawScreen();
            }

        }

        /// <summary>
        /// Writes Scoreboard to the screen
        /// </summary>
        private static void WriteScoreboard()
        {
            string scoreboard = $"Score: {score}";
            for (int i = 0; i < scoreboard.Length; i++)
            {
                screen[screenWidth * 1 + i] = scoreboard[i];
            }
        }

        /// <summary>
        /// generate new random Tetromino
        /// </summary>
        private static void GenerateNewTetromino()
        {
            currentX = (fieldWidth - 3) / 2;
            currentY = 0;
            currentRotation = 0;
            currentPiece = new Random().Next(0, 7);
        }


        /// <summary>
        /// Draw Tetromino on the screen
        /// </summary>
        private static void DrawCurrentPiece()
        {
            for (int px = 0; px < 4; px++)
            {
                for (int py = 0; py < 4; py++)
                {
                    if (tetrominos[currentPiece][Rotate(px, py, currentRotation)] == 'X')
                    {
                        screen[(currentY + py + 2) * screenWidth + (currentX + px + 2)] = (char)(currentPiece + 65);
                    }
                }
            }
        }

        /// <summary>
        /// Draw playingField on screen
        /// </summary>
        private static void DrawField()
        {
            for (int x = 0; x < fieldWidth; x++)
            {
                for (int y = 0; y < fieldHeight; y++)
                {
                    screen[(y + 2) * screenWidth + (x + 2)] = " ABCDEFG=#"[playingField[y * fieldWidth + x]];
                }
            }
        }

        /// <summary>
        /// Draws screen on console
        /// </summary>
        private static void DrawScreen()
        {
            for (int y = 0; y < screenHeight; y++)
            {
                for (int x = 0; x < screenWidth; x++)
                {
                    Console.Write(screen[y * screenWidth + x]);
                }

                Console.WriteLine();
                Console.Write("  ");
            }

            Console.SetCursorPosition(0, 0);
        }


        /// <summary>
        /// User Keyboard input
        /// </summary>
        private static void UserInput()
        {
            if (Console.KeyAvailable)
            {

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.A or ConsoleKey.LeftArrow:
                        if (DoesPieceFit(currentPiece, currentRotation, currentX - 1 , currentY))
                        {
                            currentX = currentX - 1;
                            rotateHold = false;
                        }
                        break;

                    case ConsoleKey.D or ConsoleKey.RightArrow:
                        if (DoesPieceFit(currentPiece, currentRotation, currentX + 1, currentY))
                        {
                            currentX = currentX + 1;
                            rotateHold = false;
                        }
                        break;

                    case ConsoleKey.S or ConsoleKey.DownArrow:
                        if (DoesPieceFit(currentPiece, currentRotation, currentX, currentY + 1))
                        {
                            currentY = currentY + 1;
                            rotateHold = false;
                        }
                        break;

                    case ConsoleKey.W or ConsoleKey.UpArrow:
                        if (!rotateHold && DoesPieceFit(currentPiece, currentRotation+1, currentX, currentY))
                        {
                            currentRotation = currentRotation + 1;
                            rotateHold = true;
                        }
                        else
                        {
                            rotateHold = false;
                        }
                        break;
                }
            }
            
        }


        /// <summary>
        /// Initialize Tetromino 
        /// </summary>
        public static void InitializeTetrominos()
        {
            tetrominos[0] =     "..X." 
                              + "..X."
                              + "..X."
                              + "..X.";
            tetrominos[1] =     "..X."
                              + ".XX."
                              + "..X."
                              + "....";
            tetrominos[2] =     "...."
                              + ".XX."
                              + ".XX."
                              + "....";
            tetrominos[3] =     ".X.."
                              + ".XX."
                              + "..X."
                              + "....";
            tetrominos[4] =     "..X."
                              + ".XX."
                              + ".X.."
                              + "....";
            tetrominos[5] =     "...."
                              + ".XX."
                              + "..X."
                              + "..X.";
            tetrominos[6] =     "...."
                              + ".XX."
                              + ".X.."
                              + ".X..";
        }

        public static void InitializePlayingField()
        {
            playingField = new int[fieldWidth * fieldHeight];

            for (int x = 0; x < fieldWidth; x++)
            {
                for (int y = 0; y < fieldHeight; y++)
                {
                    playingField[y * fieldWidth + x] = (x == 0 || x == fieldWidth - 1 ||  y == fieldHeight-1) ? 9:0;
                }
            }
        }

        /// <summary>
        /// Rotation of tetromino
        /// </summary>
        /// <param name="px"></param>
        /// <param name="py"></param>
        /// <param name="r">values can be 0,1,2,3 </param>
        /// <returns></returns>
        public static int Rotate(int px, int py, int r)
        {
            switch (r % 4)
            {
                case 0:
                    return py*4 + px; // 0 degrees
                case 1:
                    return 12 + py - 4*px; // 90 degrees
                case 2:
                    return 15 - py*4 - px; // 180 degrees
                case 3:
                    return 3 - py + 4*px; // 270 degrees
            }

            return 0;
        }

        /// <summary>
        /// Check does the tetromino piece fit on the playing field
        /// </summary>
        /// <param name="tetromino"></param>
        /// <param name="rotation"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <returns></returns>
        public static bool DoesPieceFit(int tetromino, int rotation, int posX, int posY)
        {
            for (int px = 0; px < 4; px++)
            {
                for (int py = 0; py < 4; py++)
                {
                    // Get index into piece
                    int pi = Rotate(px, py, rotation);
                    // Get index into field
                    int fi = (posY+py) * fieldWidth + (posX+px);

                    if ( (posX+px) >=0 && (posX+px) < fieldWidth)
                    {
                        if ( (posY+py) >=0 && (posY+py) < fieldHeight)
                        {
                            if (tetrominos[tetromino][pi] == 'X' && playingField[fi] != 0)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
        
    }
}