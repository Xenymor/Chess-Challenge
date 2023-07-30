using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    Board board;
    int depth = 5;
    Dictionary<ulong, byte> order = new Dictionary<ulong, byte>();

    public Move Think(Board board, Timer timer)
    {
        this.board = board;
        MoveDouble bestMove = new MoveDouble(new Move(), double.NaN);
        bestMove = alphaBeta(double.MinValue, double.MaxValue, depth);
        Console.WriteLine("MyBot: " + bestMove.GetEval());
        return bestMove.GetMove();
    }

    private MoveDouble alphaBeta(double alpha, double beta, int depth)
    {
        if (depth <= 0 || board.IsDraw() || board.IsInCheckmate())
        {
            return new MoveDouble(new Move(), EvaluatePosition());
        }
        Move[] moves = board.GetLegalMoves();
        if (moves.Length == 0)
        {
            return new MoveDouble(new Move(), EvaluatePosition());
        }
        Array.Sort(moves, new MyMoveComparer(board));
        if (order.TryGetValue(board.ZobristKey, out byte index))
        {
            (moves[index], moves[0]) = (moves[0], moves[index]);
        }
        MoveDouble bestMove = new MoveDouble(new Move(), !board.IsWhiteToMove ? double.MaxValue : double.MinValue);
        byte bestMoveIndex = 0;
        for (byte i = 0; i < moves.Length; i++)
        {
            Move move = moves[i];
            board.MakeMove(move);
            MoveDouble score = alphaBeta(alpha, beta, depth - 1);
            board.UndoMove(move);
            if (board.IsWhiteToMove)
            {
                if (score.GetEval() >= beta)
                {
                    order[board.ZobristKey] = (byte)i;
                    return new MoveDouble(move, score.GetEval());
                }
                if (score.GetEval() > alpha)
                {
                    alpha = score.GetEval();
                    bestMove = new MoveDouble(move, alpha);
                    bestMoveIndex = (byte)i;
                    if (alpha == 1000)
                    {
                        order[board.ZobristKey] = (byte)i;
                        return bestMove;
                    }
                }

            }
            else
            {
                if (score.GetEval() <= alpha)
                {
                    order[board.ZobristKey] = (byte)i;
                    return new MoveDouble(move, score.GetEval());
                }
                if (score.GetEval() < beta)
                {
                    beta = score.GetEval();
                    bestMove = new MoveDouble(move, beta);
                    bestMoveIndex = (byte)i;
                    if (beta == -1000)
                    {
                        order[board.ZobristKey] = (byte)i;
                        return bestMove;
                    }
                }
            }
        }
        /*if (bestMove.GetMove().Equals(new Move())) {
            Console.WriteLine("alpha: " + alpha + "; beta: " + beta + "; PlayerToMove: " + board.IsWhiteToMove + "; Depth remaining: " + depth);
            Environment.Exit(-1);
        }*/
        order[board.ZobristKey] = bestMoveIndex;
        return bestMove;
    }

    /*private double EvaluatePosition()
    {
        if (board.IsInCheckmate())
        {
            return !board.IsWhiteToMove ? 1000 : -1000;
        }
        else if (board.IsDraw())
        {
            return 0;
        }

        double white = 0;
        double black = 0;

        PieceList[] pieceLists = board.GetAllPieceLists();
        for (int i = 0; i < pieceLists.Length; i++)
        {
            PieceList pieceList = pieceLists[i];
            if (pieceList.Count == 0)
            {
                continue;
            }
            double pieceValue = getPieceValue(pieceList.GetPiece(0).PieceType);
            foreach (var piece in pieceList)
            {
                ulong attacks = 0;
                if (piece.IsBishop || piece.IsRook || piece.IsQueen)
                {
                    attacks = BitboardHelper.GetSliderAttacks(piece.PieceType, piece.Square, board);
                } else if (piece.IsKnight)
                {
                    attacks = BitboardHelper.GetKnightAttacks(piece.Square);
                } else if (piece.IsPawn)
                {
                    attacks = BitboardHelper.GetPawnAttacks(piece.Square, piece.IsWhite);
                } else if (piece.IsKing)
                {
                    attacks = BitboardHelper.GetKingAttacks(piece.Square);
                }
                if (i < pieceLists.Length / 2)
                {
                    attacks = attacks & board.WhitePiecesBitboard;

                    white += pieceValue + (BitOperations.PopCount(attacks)/5);
                }
                else
                {
                    attacks = attacks & board.BlackPiecesBitboard;
                    black += pieceValue + (BitOperations.PopCount(attacks)/5);
                }
            }
        }

        double eval = (white - black);
        return eval;
    }

    private double getPieceValue(PieceType pieceType)
    {
        switch (pieceType)
        {
            case PieceType.Pawn:
                return 1;
            case PieceType.Knight:
                return 3;
            case PieceType.Bishop:
                return 3.5;
            case PieceType.Rook:
                return 5;
            case PieceType.Queen:
                return 9;
            case PieceType.King:
                return 0;
            default:
                return 0;
        }
    }*/

    private double EvaluatePosition()
    {
        if (board.IsInCheckmate())
        {
            return !board.IsWhiteToMove ? 1000 : -1000;
        }
        else if (board.IsDraw())
        {
            return 0;
        }

        PieceList[] pls = board.GetAllPieceLists();
        double white = pls[0].Count
            + pls[1].Count * 3
            + pls[2].Count * 3.5
            + pls[3].Count * 5
            + pls[4].Count * 9;
        double black = pls[6].Count
            + pls[7].Count * 3
            + pls[8].Count * 3.5
            + pls[9].Count * 5
            + pls[10].Count * 9;

        double eval = (white - black);
        return eval;
    }
}

class MoveDouble
{
    Move move;
    double eval;

    public MoveDouble(Move lastMove, double v)
    {
        this.move = lastMove;
        this.eval = v;
    }

    public Move GetMove() { return move; }

    public double GetEval() { return eval; }
}

class MyMoveComparer : IComparer<Move>
{
    private Board board;

    public MyMoveComparer(Board board)
    {
        this.board = board;
    }

    public int Compare(Move x, Move y)
    {
        int result = 0;
        result = sort(result, isCheckMate(y), isCheckMate(x));
        result = sort(result, isCheck(y), isCheck(x));
        result = sort(result, y.CapturePieceType, x.CapturePieceType);
        result = sort(result, movesTowardsCenter(y), movesTowardsCenter(x));
        result = sort(result, movesForward(y, board.IsWhiteToMove), movesForward(x, board.IsWhiteToMove));
        return result;
    }

    private bool movesTowardsCenter(Move y)
    {
        double startDist = getDist(y.StartSquare.Rank, y.StartSquare.File, 4.5, 4.5);
        double endDist = getDist(y.TargetSquare.Rank, y.TargetSquare.File, 4.5, 4.5);
        return startDist > endDist;
    }

    private double getDist(double x1, double y1, double x2, double y2)
    {
        return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }

    private bool isCheckMate(Move y)
    {
        board.MakeMove(y);
        bool result = board.IsInCheckmate();
        board.UndoMove(y);
        return result;
    }

    private bool movesForward(Move x, bool isWhiteToMove)
    {
        if (isWhiteToMove)
        {
            if ((x.StartSquare.Rank - x.TargetSquare.Rank) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if ((x.StartSquare.Rank - x.TargetSquare.Rank) < 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private bool isCheck(Move x)
    {
        board.MakeMove(x);
        bool isCheck = board.IsInCheck();
        board.UndoMove(x);
        return isCheck;
    }

    private int sort(int old, bool a, bool b)
    {
        if (old == 0)
        {
            return a == b ? 0 : a ? 1 : -1;
        }
        return old;
    }

    private int sort(int old, Enum a, Enum b)
    {
        if (old == 0)
        {
            return a.CompareTo(b);
        }
        return old;
    }

    private int sort(int old, int a, int b)
    {
        if (old == 0)
        {
            return a.CompareTo(b);
        }
        return old;
    }
}