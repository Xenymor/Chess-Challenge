using ChessChallenge.API;
using ChessChallenge.Application;
using ChessChallenge.Example;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Threading;
using System.Security;
using System.Text.Json;
using System.Collections.ObjectModel;

namespace Tuner
{
    internal class PositionEvaluation
    {
        public string fen { get; set; }
        public int eval { get; set; }
    }

    internal class Program
    {
        private static readonly float TEST_VALUE = .01f;
        private static readonly float STEP_VALUE = .001f;
        static readonly string FILE_NAMES = "abcdefgh";

        static void Main(string[] args)
        {
            ChessChallenge.Chess.Board tempBoard = new ChessChallenge.Chess.Board();
            tempBoard.LoadStartPosition();
            Board board = new Board(tempBoard);
            Random random = new Random();
            int iterations = 0;
            MyBot bot = new MyBot();
            int r;
            float[] parameters = { 17.804245f, -42.86006f, -1.0475401f, 0.7519452f };
            float[] parameterChanges = new float[parameters.Length];
            string filePath = "positionTable.json";

            /*List<Move[]> allGames = getAllGames();

            Console.WriteLine("Finished Parsing");

            PositionEvaluation[] toSave = new PositionEvaluation[allGames.Count];

            for (int i = 0; i < allGames.Count; i++)
            {
                board.board.LoadStartPosition();
                r = random.Next(allGames[i].Length);
                for (int j = 0; j < r; j++)
                {
                    board.MakeMove(allGames[i][j]);
                    if (j == r-1)
                    {
                        if (board.IsInCheck())
                        {
                            r++;
                        }
                    }
                }
                int eval = getTrueEval(board);
                toSave[i] = new PositionEvaluation
                {
                    eval = eval,
                    fen = board.GetFenString()
                };
                Console.WriteLine("Game: " + i + "/" + allGames.Count);
            }

            string jsonString = JsonSerializer.Serialize(toSave);
            File.WriteAllText(filePath, jsonString);*/

            PositionEvaluation[] allPositions = JsonSerializer.Deserialize<PositionEvaluation[]>(File.ReadAllText(filePath));

            while (iterations < 100_000_000)
            {
                PositionEvaluation position = allPositions[random.Next(allPositions.Length-1)];
                board.board.LoadPosition(position.fen);

                int oldEval = bot.TunerEvaluate(board, parameters);
                int trueEval = position.eval;
                if (!board.IsWhiteToMove)
                {
                    trueEval = -trueEval;
                }

                int oldDistance = Math.Abs(oldEval - trueEval);
                int newEval;
                int newDistance;
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i] += TEST_VALUE;
                    newEval = bot.TunerEvaluate(board, parameters);
                    parameters[i] -= TEST_VALUE;
                    newDistance = Math.Abs(newEval - trueEval);
                    if (newDistance < oldDistance) {
                        parameterChanges[i] = STEP_VALUE;
                    } else if (newDistance != oldDistance) {
                        parameterChanges[i] = -STEP_VALUE;
                    }
                }

                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i] += parameterChanges[i];
                }

                if (iterations % 1_000 == 0)
                {
                    string msg = "\n";
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        msg += parameters[i].ToString() + ", ";
                    }
                    Console.WriteLine(msg);
                }
                iterations++;
            }
            if (iterations % 10 == 0)
            {
                string msg = "\n";
                for (int i = 0; i < parameters.Length; i++)
                {
                    msg += parameters[i].ToString() + ", ";
                }
                Console.WriteLine(msg);
            }
        }

        static List<Move[]> getAllGames()
        {
            ChessChallenge.Chess.Board tempBoard = new ChessChallenge.Chess.Board();
            tempBoard.LoadStartPosition();
            Board board = new Board(tempBoard);

            // Replace "your-pgn-file.pgn" with the actual path to your PGN file
            string pgnFilePath = "C:\\Users\\timon\\Documents\\Programmieren\\C#\\Chess-Challenge\\Tuner\\Magnus Carlsen-black.pgn";

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

                // Define a regular expression pattern to match moves within a game
                string movePattern = @"([NBRQK]?[a-h]?[1-8]?x?[a-h][1-8](?:=[NBRQK])?[+#]?|O-O(?:-O)?)";

                // Use Regex to find all matches of moves in the gameMoves content
                MatchCollection moveMatches = Regex.Matches(gameMoves, movePattern);

                if (moveMatches.Count == 0)
                {
                    continue;
                }

                Move[] moves = new Move[moveMatches.Count];

                Console.WriteLine("Finished Loading");

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
                            }
                            else
                            {
                                if (char.IsLetter(moveString[0]) && char.IsDigit(moveString[1]) && char.IsLetter(moveString[3]))
                                {
                                    targetSquare = new Square(FILE_NAMES.IndexOf(moveString[3]), int.Parse(moveString[4].ToString()) - 1);
                                    startRank = int.Parse(moveString[1].ToString()) - 1;
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

            // Process for 5 seconds
            string processString = "go movetime 900";

            // Process 20 deep
            //string processString = "go depth 2";

            p.StandardInput.WriteLine(processString);

            //Console.WriteLine("Started Stockfish");

            Thread.Sleep(1000);

            //Console.WriteLine("Stopped Sleeping");

            p.StandardInput.WriteLine("eval");

            Thread.Sleep(40);

            p.StandardInput.WriteLine("quit");

            string result = "";

            while (!p.StandardOutput.EndOfStream)
            {
                result += p.StandardOutput.ReadLine() + "\n";
            }

            Console.WriteLine(result);

            MatchCollection matches = Regex.Matches(result, "(-|\\+)?\\d+\\.\\d+");
            if (matches.Count == 0)
            {
                throw new Exception();
            }
            string evalText = matches[matches.Count - 1].Value;

            Console.WriteLine("Got Stockfish Eval: " + float.Parse(evalText));

            p.Close();
            return (int)Math.Round(float.Parse(evalText));
        }
    }
}
