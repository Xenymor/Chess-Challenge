using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace Tuner
{
    internal class PositionEvaluation
    {
        public string fen { get; set; }
        public int eval { get; set; }
    }

    internal class Program
    {
        private static readonly float TEST_VALUE = .001f;
        private static readonly float STEP_VALUE = .0005f;
        static readonly string FILE_NAMES = "abcdefgh";
        private static readonly int BATCH_SIZE = 100;
        private static string parameterFilePath = "parametersSaveFile.json";

        static void Main(string[] args)
        {
            ChessChallenge.Chess.Board tempBoard = new ChessChallenge.Chess.Board();
            tempBoard.LoadStartPosition();
            Board board = new Board(tempBoard);
            Random random = new Random();
            int iterations = 0;
            MyBot bot = new MyBot();
            float[] parameters = { 21.537073f, 28.860802f, -5.7034264f, -21.147152f, 0, 0, 0, -9.241379f, 309.2178f, 410.36957f, 735.34515f, 647.4354f, 10000 };
            float[] parameterChanges = new float[parameters.Length];
            string filePath = "positionTable1.json";

            //Generate new File
            
            /*List<Move[]> allGames = getAllGames();

            Console.WriteLine("Finished Parsing");

            int gameMultiplier = 3;
            PositionEvaluation[] toSave = new PositionEvaluation[allGames.Count*gameMultiplier];
            generateEvaluationArray(board, random, allGames, toSave, gameMultiplier);

            string jsonString = JsonSerializer.Serialize(toSave);
            File.WriteAllText(filePath, jsonString);*/
            
            

            //Train

            PositionEvaluation[] allPositions = JsonSerializer.Deserialize<PositionEvaluation[]>(File.ReadAllText(filePath));
            PositionEvaluation[] trainingPositions = new PositionEvaluation[allPositions.Length - 400];
            PositionEvaluation[] testPositions = new PositionEvaluation[400];
            
            if (File.Exists(parameterFilePath))
            {
                parameters = JsonSerializer.Deserialize<float[]>(File.ReadAllText(parameterFilePath));
            }

            for (int i = 0; i < allPositions.Length; i++)
            {
                if (i < 400)
                {
                    testPositions[i] = allPositions[i];
                }
                else
                {
                    trainingPositions[i - 400] = allPositions[i];
                }
            }

            while (iterations < 100_000_000)
            {
                float oldEval, trueEval;
                int counter = 0;
                for (int i = 0; i < BATCH_SIZE; i++) {
                    if ((iterations % trainingPositions.Length)+i > trainingPositions.Length) {
                        break;
                    }

                    prepareBoardAndGetEval(board, (iterations % trainingPositions.Length)+i, bot, parameters, trainingPositions, out oldEval, out trueEval);

                    float dist = oldEval - trueEval;
                    float oldCost = dist * dist;

                    createChangeValues(board, bot, parameters, parameterChanges, trueEval, oldCost);

                    counter++;
                }

                if (counter != 0)
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i] += parameterChanges[i]/counter;
                    parameterChanges[i] = 0;
                }

                if (iterations % 40_000 == 0)
                {
                    OutputCurrentStandings(board, bot, parameters, testPositions);
                    string jsonString = JsonSerializer.Serialize(parameters);
                    File.WriteAllText(parameterFilePath, jsonString);
                }
                iterations++;
            }
            OutputCurrentStandings(board, bot, parameters, testPositions);
        }

        private static void OutputCurrentStandings(Board board, MyBot bot, float[] parameters, PositionEvaluation[] testPositions)
        {
            float distSum = 0;
            int positions = testPositions.Length;
            for (int i = 0; i < testPositions.Length; i++)
            {
                board.board.LoadPosition(testPositions[i].fen);
                if (bot.TunerEvaluate(board, parameters) == int.MinValue)
                    Console.WriteLine("Error");
                distSum += Math.Abs(testPositions[i].eval - Math.Max(bot.TunerEvaluate(board, parameters), int.MinValue+1));
            }
            string msg = "\n";
            for (int i = 0; i < parameters.Length; i++)
            {
                msg += parameters[i].ToString() + ", ";
            }
            Console.WriteLine(msg + "\n" + "Cost: " + ((float)distSum / positions));
        }

        private static void createChangeValues(Board board, MyBot bot, float[] parameters, float[] parameterChanges, float trueEval, float oldDistance)
        {
            float newEval;
            float newDistance;
            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i] += TEST_VALUE;
                newEval = bot.TunerEvaluate(board, parameters);
                parameters[i] -= TEST_VALUE;
                newDistance = (newEval - trueEval);
                newDistance *= newDistance;
                if (newDistance < oldDistance)
                {
                    parameterChanges[i] += STEP_VALUE;
                }
                else if (newDistance > oldDistance)
                {
                    parameterChanges[i] -= STEP_VALUE;
                }
            }
        }

        private static void prepareBoardAndGetEval(Board board, int iterations, MyBot bot, float[] parameters, PositionEvaluation[] trainingPositions, out float oldEval, out float trueEval)
        {
            PositionEvaluation position = trainingPositions[iterations % trainingPositions.Length];
            board.board.LoadPosition(position.fen);

            oldEval = bot.TunerEvaluate(board, parameters);
            trueEval = position.eval;
            if (!board.IsWhiteToMove)
            {
                trueEval = -trueEval;
            }
        }

        private static void generateEvaluationArray(Board board, Random random, List<Move[]> allGames, PositionEvaluation[] toSave, int gameMultiplier)
        {
            bool broke;
            int r;
            Move[] moves;
            for (int i = 0; i < allGames.Count*gameMultiplier; i++)
            {
                broke = false;
                board.board.LoadStartPosition();
                Move[] game = allGames[i/gameMultiplier];
                r = random.Next(game.Length);
                for (int j = 0; j < r; j++)
                {
                    board.MakeMove(game[j]);
                }
                moves = board.GetLegalMoves(true);
                while (moves.Length != 0 || board.IsInCheck())
                {
                    if (moves.Length == 0)
                    {
                        moves = board.GetLegalMoves();
                    }
                    if (moves.Length == 0)
                    {
                        broke = true;
                        break;
                    }
                    board.MakeMove(moves[random.Next(moves.Length - 1)]);
                    moves = board.GetLegalMoves(true);
                }
                int eval;
                if (!broke)
                {
                    eval = getTrueEval(board);
                }
                else if(board.IsInCheckmate()) {
                    eval = 30_000 * (board.IsWhiteToMove ? -1 : 1);
                }
                else
                {
                    eval = 0;
                }
                toSave[i] = new PositionEvaluation
                {
                    eval = eval,
                    fen = board.GetFenString()
                };
                if (i%100 == 0)
                    Console.WriteLine("Game: " + i + "/" + allGames.Count*gameMultiplier);
            }
        }

        static List<Move[]> getAllGames()
        {
            ChessChallenge.Chess.Board tempBoard = new ChessChallenge.Chess.Board();
            tempBoard.LoadStartPosition();
            Board board = new Board(tempBoard);

            // Replace "your-pgn-file.pgn" with the actual path to your PGN file
            string pgnFilePath = "C:\\Users\\timon\\Documents\\Programmieren\\C#\\Chess-Challenge\\Tuner\\lichess_db_standard_rated_2013-01.pgn";//Magnus Carlsen-black.pgn";

            // Read the PGN file content
            string pgnContent = File.ReadAllText(pgnFilePath);

            // Define a regular expression pattern to match games
            string gamePattern = @"\[(.*?)\]((?:(?!\[)[\s\S])*)";

            // Use Regex to find all matches of games in the PGN content
            MatchCollection gameMatches = Regex.Matches(pgnContent, gamePattern);

            // Create a list to store the moves from each game
            List<Move[]> allGames = new List<Move[]>();

            // Iterate through the matched games
            foreach (Match gameMatch in gameMatches)
            {
                // Get the moves part of the game
                string gameMoves = gameMatch.Groups[2].Value.Trim();

                if (gameMoves.Contains("}") || gameMoves.Contains("{"))
                    continue;

                // Define a regular expression pattern to match moves within a game
                string movePattern = @"(?<=[^}]|^)([NBRQK]?[a-h]?[1-8]?x?[a-h][1-8](?:=[NBRQK])?[+#]?|O-O(?:-O)?)";

                // Use Regex to find all matches of moves in the gameMoves content
                MatchCollection moveMatches = Regex.Matches(gameMoves, movePattern);

                if (moveMatches.Count == 0)
                {
                    continue;
                }

                Move[] moves = new Move[moveMatches.Count];

                //Console.WriteLine("Finished Loading");

                board.board.LoadStartPosition();

                // Extract and add the moves to the list
                for (int i = 0; i < moveMatches.Count; i++)
                {
                    string moveString = moveMatches[i].Value;
                    Move[] allLegalMoves = board.GetLegalMoves();
                    bool isCapture = moveString.Contains("x");
                    bool isCastling = moveString.Contains("-");
                    bool isPawnMove = char.IsLower(moveString[0]);
                    bool isCheck = moveString.Contains("+");
                    bool isPromotion = moveString.Contains("=");
                    bool isSmallCastle = moveString.Length == 3;
                    bool isCheckMate = moveString.Contains("#");
                    if (isCheckMate)
                        isCheck = true;
                    PieceType moveType = PieceType.Pawn;
                    if (!isPawnMove)
                    {
                        moveType = getPieceType(moveString[0]);
                    }
                    Square targetSquare = new Square();
                    int startFile = -1;
                    int startRank = -1;
                    PieceType promotionType = PieceType.None;
                    if (!isCastling)
                    {
                        if (!isPawnMove)
                        {
                            if (moveString.Length == 3)
                            {
                                targetSquare = new Square(FILE_NAMES.IndexOf(moveString[1]), int.Parse(moveString[2].ToString()) - 1);
                            }
                            else if (isCheck)
                            {
                                if (moveString.Length == 4)
                                    targetSquare = new Square(FILE_NAMES.IndexOf(moveString[1]), int.Parse(moveString[2].ToString()) - 1);
                                else if (moveString.Length == 5 && isCapture)
                                    targetSquare = new Square(FILE_NAMES.IndexOf(moveString[2]), int.Parse(moveString[3].ToString()) - 1);
                            }
                            else if (isCapture && moveString.Length == 4)
                            {
                                targetSquare = new Square(FILE_NAMES.IndexOf(moveString[2]), int.Parse(moveString[3].ToString()) - 1);
                            }
                            if (char.IsLetter(moveString[0]) && char.IsLetter(moveString[1]) && char.IsLetter(moveString[2]))
                            {
                                if (!isCapture)
                                {
                                    targetSquare = new Square(FILE_NAMES.IndexOf(moveString[2]), int.Parse(moveString[3].ToString()) - 1);
                                }
                                else if (char.IsLetter(moveString[3]))
                                {
                                    targetSquare = new Square(FILE_NAMES.IndexOf(moveString[3]), int.Parse(moveString[4].ToString()) - 1);
                                }
                            }
                            if (!isCapture)
                            {
                                if (char.IsLetter(moveString[0]) && char.IsDigit(moveString[1]) && char.IsLetter(moveString[2]))
                                {
                                    targetSquare = new Square(FILE_NAMES.IndexOf(moveString[2]), int.Parse(moveString[3].ToString()) - 1);
                                    startRank = int.Parse(moveString[1].ToString()) - 1;
                                }
                                if (moveString.Length >= 4 && char.IsLetter(moveString[1]) && char.IsDigit(moveString[2]) && char.IsLetter(moveString[3]))
                                {
                                    targetSquare = new Square(FILE_NAMES.IndexOf(moveString[3]), int.Parse(moveString[4].ToString()) - 1);
                                    startFile = FILE_NAMES.IndexOf(char.ToLower(moveString[1]));
                                    startRank = int.Parse(moveString[2].ToString())-1;
                                }
                            }
                            else
                            {
                                if (char.IsLetter(moveString[0]) && char.IsDigit(moveString[1]) && char.IsLetter(moveString[3]))
                                {
                                    targetSquare = new Square(FILE_NAMES.IndexOf(moveString[3]), int.Parse(moveString[4].ToString()) - 1);
                                    startFile = int.Parse(moveString[1].ToString()) - 1;
                                }
                                if (moveString.Length >= 4 && char.IsLetter(moveString[1]) && char.IsDigit(moveString[2]) && char.IsLetter(moveString[4]))
                                {
                                    targetSquare = new Square(FILE_NAMES.IndexOf(moveString[4]), int.Parse(moveString[5].ToString()) - 1);
                                    startFile = FILE_NAMES.IndexOf(char.ToLower(moveString[1]));
                                    startRank = int.Parse(moveString[2].ToString()) - 1;
                                }
                            }
                        }
                        else
                        {
                            if (!isCapture)
                            {
                                targetSquare = new Square(FILE_NAMES.IndexOf(moveString[0]), int.Parse(moveString[1].ToString()) - 1);
                            }
                            else if (isCapture)
                            {
                                targetSquare = new Square(FILE_NAMES.IndexOf(moveString[2]), int.Parse(moveString[3].ToString()) - 1);
                                if (char.IsLetter(moveString[0]) && char.IsLetter(moveString[1]))
                                {
                                    startFile = FILE_NAMES.IndexOf(char.ToLower(moveString[0]));
                                }
                            }
                        }
                    }
                    if (!isPawnMove && char.IsLetter(moveString[0]) && char.IsLetter(moveString[1]) && char.IsLetter(moveString[2]))
                    {
                        startFile = FILE_NAMES.IndexOf(char.ToLower(moveString[1]));
                    }
                    else if (!isPawnMove && char.IsLetter(moveString[0]) && char.IsDigit(moveString[1]) && char.IsLetter(moveString[2]))
                    {
                        startRank = int.Parse(moveString[1].ToString()) - 1;
                    }
                    if (isPromotion && !isCheck)
                    {
                        promotionType = getPieceType(moveString[moveString.Length - 1]);
                    }
                    else if (isPromotion)
                    {
                        promotionType = getPieceType(moveString[moveString.Length - 2]);
                    }
                    for (int j = 0; j < allLegalMoves.Length; j++)
                    {
                        Move currentMove = allLegalMoves[j];
                        if (currentMove.TargetSquare.Equals(targetSquare))
                        {
                            if (startRank >= 0 && startRank != currentMove.StartSquare.Rank)
                            {
                                continue;
                            }
                            if (startFile >= 0 && startFile != currentMove.StartSquare.File)
                            {
                                continue;
                            }
                            if (isCapture != currentMove.IsCapture)
                            {
                                continue;
                            }
                            if (moveType != currentMove.MovePieceType)
                            {
                                continue;
                            }
                            if (promotionType != currentMove.PromotionPieceType)
                            {
                                continue;
                            }
                        }
                        else if (!isCastling)
                        {
                            continue;
                        }
                        if (isCastling != currentMove.IsCastles)
                        {
                            continue;
                        }
                        else if (isCastling)
                        {
                            if (isSmallCastle && currentMove.TargetSquare.File != 6)
                            {
                                continue;
                            }
                            else if (!isSmallCastle && currentMove.TargetSquare.File != 2)
                            {
                                continue;
                            }
                        }
                        moves[i] = currentMove;
                        break;
                    }

                    if (moves[i].IsNull)
                    {
                        Console.WriteLine("Error");
                        Console.WriteLine(moveString + " : " + targetSquare.ToString());
                        throw new Exception();
                    }
                    else
                    {
                        board.MakeMove(moves[i]);
                    }
                }

                allGames.Add(moves);
            }
            return allGames;
        }

        static PieceType getPieceType(char pieceChar)
        {
            switch (pieceChar)
            {
                case 'R':
                    return PieceType.Rook;
                case 'N':
                    return PieceType.Knight;
                case 'B':
                    return PieceType.Bishop;
                case 'Q':
                    return PieceType.Queen;
                case 'K':
                    return PieceType.King;
                case 'P':
                    return PieceType.Pawn;
                default:
                    return PieceType.None;
            }
        }

        private static int getTrueEval(Board board)
        {
            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "C:\\Users\\timon\\Documents\\Programmieren\\C#\\Chess-Challenge\\Tuner\\stockfish-windows-x86-64-avx2.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            string setupString = "position fen " + board.GetFenString();
            p.StandardInput.WriteLine(setupString);

            // Process for 10 milliseconds
            string processString = "go movetime 10";

            // Process 20 deep
            //string processString = "go depth 2";

            p.StandardInput.WriteLine(processString);

            //Console.WriteLine("Started Stockfish");

            Thread.Sleep(20);

            //Console.WriteLine("Stopped Sleeping");

            p.StandardInput.WriteLine("eval");

            Thread.Sleep(20);

            p.StandardInput.WriteLine("quit");

            string result = "";

            while (!p.StandardOutput.EndOfStream)
            {
                result += p.StandardOutput.ReadLine() + "\n";
            }

            //Console.WriteLine(result);

            MatchCollection matches = Regex.Matches(result, "(-|\\+)?\\d+\\.\\d+");
            if (matches.Count == 0)
            {
                throw new Exception();
            }
            string evalText = matches[matches.Count - 1].Value;

            //Console.WriteLine("Got Stockfish Eval: " + float.Parse(evalText));

            p.Close();
            return (int)Math.Round(float.Parse(evalText));
        }
    }
}
