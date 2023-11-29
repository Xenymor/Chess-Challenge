using Chess_Challenge.src.EvilBot6_3;
using ChessChallenge.API;
using System.Reflection;

static class Program
{
    public static void Main()
    {
        IChessBot bot = new MyBot();
        Type botType = bot.GetType();
        ChessChallenge.Chess.Board tempBoard = new ChessChallenge.Chess.Board();
        tempBoard.LoadStartPosition();
        Board board = new Board(tempBoard);
        float[] parameters = new float[]        { 0, 100, 310, 330, 500, 1000, 10000, 1, 2,    23,   62,  15, 30};
        List<String> parameterNames = new List<String>();
        bool isTuning = false;
        parameterNames.AddRange(new string[] { "", "PV", "NV", "BV", "RV", "QV", "KV", "MM", "ME", "BPM", "BPE", "DM", "DE" });
        while (true)
        {
            string command = Console.ReadLine();

            if (string.IsNullOrEmpty(command))
                continue;

            string[] tokens = command.Split(' ');

            switch (tokens[0])
            {
                case "uci":
                    Console.WriteLine("uciok");
                    break;

                case "ucinewgame":
                    bot = (IChessBot)botType.GetConstructor(new Type[] { true.GetType() }).Invoke(new object[] { false });
                    tempBoard = new ChessChallenge.Chess.Board();
                    tempBoard.LoadStartPosition();
                    board = new Board(tempBoard);
                    break;

                case "isready":
                    Console.WriteLine("readyok");
                    break;

                case "quit":
                    Environment.Exit(0);
                    break;

                case "position":
                    if (tokens.Length >= 2 && tokens[1].Equals("startpos"))
                    {
                        // Handle starting position setup
                        board.board.LoadStartPosition();
                        if (tokens.Length > 2 && tokens[2].Equals("moves"))
                        {
                            for (int i = 3; i < tokens.Length; i++)
                            {
                                board.MakeMove(new Move(tokens[i], board));
                            }
                        }
                    }
                    // Handle other position setup options if needed
                    break;

                case "go":
                    // Parse time controls and other parameters
                    int wtime = 0;
                    int btime = 0;
                    for (int i = 1; i < tokens.Length - 1; i++)
                    {
                        if (tokens[i] == "wtime")
                            wtime = int.Parse(tokens[i + 1]);
                        else if (tokens[i] == "btime")
                            btime = int.Parse(tokens[i + 1]);
                    }

                    // Call engine to calculate and return best move
                    string bestMoveString;
                    if (!isTuning)
                        bestMoveString = bot.Think(board, new ChessChallenge.API.Timer(board.IsWhiteToMove ? wtime : btime)).ToString();
                    else
                        bestMoveString = ((MyBot)bot).Think(board, new ChessChallenge.API.Timer(board.IsWhiteToMove ? wtime : btime), parameters).ToString();
                    string bestMoveFormattedString = bestMoveString.Substring(7, bestMoveString.Length - 8);

                    Console.WriteLine("bestmove " + bestMoveFormattedString);
                    break;

                case "setvalue":
                    if (!isTuning)
                    {
                        bot = (IChessBot)botType.GetConstructor(new Type[] { false.GetType() }).Invoke(new object[] { true });
                        isTuning = true;
                    }
                    parameters[parameterNames.IndexOf(tokens[1])] = float.Parse(tokens[2]);
                    break;

                default:
                    Console.WriteLine("?");
                    break;
            }
        }
    }

    private static ChessChallenge.Chess.Move GetMove(string v)
    {
        return new ChessChallenge.Chess.Move(GetSquareIndex(v[0] + "" + v[1]), GetSquareIndex(v[2] + "" + v[3]));
    }

    private static int GetSquareIndex(string fieldString)
    {
        return new Square(fieldString).Index;
    }
}