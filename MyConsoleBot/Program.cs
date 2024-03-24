using ChessChallenge.API;

static class Program
{
    static int[] parameters;
    public static void Main()
    {
        IChessBot bot = new MyBot(true);
        Type botType = bot.GetType();
        ChessChallenge.Chess.Board tempBoard = new ChessChallenge.Chess.Board();
        tempBoard.LoadStartPosition();
        Board board = new Board(tempBoard);
        List<string> parameterNames = new List<string>();
        bool isTuning = true;
        parameterNames.AddRange(new string[] { "NO", "PV", "NV", "BV", "RV", "QV", "KV", "DPM", "DPE", "BPM", "BPE", "OFM", "OFE", "FP", "RFP", "SEC", "SEP", "HTL", "AWW", "AWS", "STL" });
        parameters = new int[parameterNames.Count];
        for (int i = 0; i < parameters.Length; i++)
        {
            parameters[i] = 1;
            //Console.WriteLine("IntegerParameter " + parameterNames[i] + " " + -1000 + " " + 1000);
        }
        parameters[parameterNames.IndexOf("NO")] = 0;
        parameters[parameterNames.IndexOf("PV")] = 100;
        parameters[parameterNames.IndexOf("KV")] = 10_000;
        parameters[parameterNames.IndexOf("NV")] = 310;
        parameters[parameterNames.IndexOf("BV")] = 330;
        parameters[parameterNames.IndexOf("RV")] = 500;
        parameters[parameterNames.IndexOf("QV")] = 1000;
        parameters[parameterNames.IndexOf("DPM")] = 15;
        parameters[parameterNames.IndexOf("DPE")] = 30;
        parameters[parameterNames.IndexOf("BPM")] = 23;
        parameters[parameterNames.IndexOf("BPE")] = 60;
        parameters[parameterNames.IndexOf("OFM")] = 13;
        parameters[parameterNames.IndexOf("OFE")] = 10;
        parameters[parameterNames.IndexOf("FP")] = 141;
        parameters[parameterNames.IndexOf("RFP")] = 74;
        parameters[parameterNames.IndexOf("SEC")] = 1;
        parameters[parameterNames.IndexOf("SEP")] = 1;
        parameters[parameterNames.IndexOf("HTL")] = 13;
        parameters[parameterNames.IndexOf("AWW")] = 60;
        parameters[parameterNames.IndexOf("AWS")] = 25;
        parameters[parameterNames.IndexOf("STL")] = 3;
        /*for (int i = 0; i < parameters.Length; i++)
        {
            Console.WriteLine("IntegerParameter " + parameterNames[i] + " " + parameters[i]);
        }
        for (int i = 0; i < parameters.Length; i++)
        {
            Console.WriteLine("IntegerParameter " + parameterNames[i] + " " + (int)(parameters[i]*0.9) + " " + (int)(parameters[i]*1.1));
        }
        for (int i = 0; i < parameters.Length; i++)
        {
            Console.Write(", " + parameters[i]);
        }
        Console.WriteLine();
        for (int i = 0; i < parameterNames.Count; i++)
        {
            Console.Write("\", \"" + parameterNames[i]);
        }*/
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
                    bot = (IChessBot)botType.GetConstructor(new Type[] { true.GetType() }).Invoke(new object[] { isTuning });
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
                    int wtime = 120_000;
                    int btime = 120_000;
                    int depth = 0;
                    for (int i = 1; i < tokens.Length - 1; i++)
                    {
                        if (tokens[i] == "wtime")
                            wtime = int.Parse(tokens[i + 1]);
                        else if (tokens[i] == "btime")
                            btime = int.Parse(tokens[i + 1]);
                        else if (tokens[i] == "depth")
                        {
                            depth = int.Parse(tokens[i + 1]);
                            wtime = int.MaxValue;
                            btime = int.MaxValue;
                        }
                    }

                    // Call engine to calculate and return best move
                    string bestMoveString;
                    if (isTuning)
                        if (depth == 0)
                            bestMoveString = ((MyBot)bot).Think(board, new ChessChallenge.API.Timer(board.IsWhiteToMove ? wtime : btime), parameters).ToString();
                        else
                            bestMoveString = ((MyBot)bot).Think(board, new ChessChallenge.API.Timer(board.IsWhiteToMove ? wtime : btime), parameters, depth).ToString();
                    else
                        bestMoveString = bot.Think(board, new ChessChallenge.API.Timer(board.IsWhiteToMove ? wtime : btime)).ToString();
                    string bestMoveFormattedString = bestMoveString.Substring(7, bestMoveString.Length - 8);

                    Console.WriteLine("bestmove " + bestMoveFormattedString);
                    break;

                case "setvalue":
                    if (!isTuning)
                    {
                        bot = (IChessBot)botType.GetConstructor(new Type[] { false.GetType() }).Invoke(new object[] { true });
                        isTuning = true;
                    }
                    parameters[parameterNames.IndexOf(tokens[1])] = int.Parse(tokens[2]);
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