using Chess_Challenge.src.EvilBot6_7;
using ChessChallenge.API;
using ChessChallenge.Example;


static class Program
{
    public static void Main()
    {
        IChessBot bot = new EvilBot6_7();
        ChessChallenge.Chess.Board tempBoard = new ChessChallenge.Chess.Board();
        tempBoard.LoadStartPosition();
        Board board = new Board(tempBoard);
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
                    bot = new EvilBot6_7();
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
                    string bestMoveString = bot.Think(board, new ChessChallenge.API.Timer(board.IsWhiteToMove ? wtime : btime)).ToString();
                    string bestMoveFormattedString = bestMoveString.Substring(7, bestMoveString.Length - 8);

                    Console.WriteLine("bestmove " + bestMoveFormattedString);
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