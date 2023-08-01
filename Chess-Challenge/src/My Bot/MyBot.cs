using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Reflection;

public class MyBot : IChessBot
{
    private const ulong FIRST_RANK = 0xFF;
    private const ulong LAST_RANK = 18374686479671623680;
    private const ulong CENTER = 0x1818000000;
    Board board;
    const int depth = 20;
    Dictionary<ulong, object[]> order;
    int moveEstimate = 200;

    public Move Think(Board board, Timer timer)
    {
        int gameLength = board.GameMoveHistory.Length;
        int movesRemaining = moveEstimate - gameLength;
        if (movesRemaining <= 0)
        {
            moveEstimate += 50;
            movesRemaining = moveEstimate - gameLength;
        }
        double timeForMove = timer.MillisecondsRemaining / movesRemaining;
        order = new Dictionary<ulong, object[]>();
        this.board = board;
        MoveDouble bestMove = new MoveDouble(new Move(), double.NaN);
        int depthCalculated = 0;
        for (byte i = 1; i < depth; i++)
        {
            bestMove = alphaBeta(double.MinValue, double.MaxValue, i);
            Console.WriteLine("-- MyBot: " + bestMove.GetEval() + "; Move: " + bestMove.GetMove() + "; depth: " + i);
            if (timer.MillisecondsElapsedThisTurn >= timeForMove)
            {
                depthCalculated = i;
                break;
            }
        }
        Console.WriteLine("MyBot: " + bestMove.GetEval() + "; depth: " + depthCalculated);
        return bestMove.GetMove();
    }

    private MoveDouble alphaBeta(double alpha, double beta, byte depth)
    {
        if (depth <= 0 || board.IsDraw() || board.IsInCheckmate())
        {
            return new MoveDouble(new Move(), EvaluatePosition());
        }
        if (order.TryGetValue(board.ZobristKey, out object[] saved))
        {
            if (depth <= (byte)saved[0])
            {
                return (MoveDouble)saved[1];
            }
        }

        Move[] moves = board.GetLegalMoves();
        if (moves.Length == 0)
        {
            return new MoveDouble(new Move(), EvaluatePosition());
        }
        MoveDouble bestMove = new MoveDouble(new Move(), !board.IsWhiteToMove ? double.MaxValue : double.MinValue);
        for (byte i = 0; i < moves.Length; i++)
        {
            Move move = moves[i];
            board.MakeMove(move);
            MoveDouble score = alphaBeta(alpha, beta, (byte) (depth - 1));
            board.UndoMove(move);
            if (board.IsWhiteToMove)
            {
                if (score.GetEval() >= beta)
                {
                    MoveDouble result = new MoveDouble(move, score.GetEval());
                    order[board.ZobristKey] = new object[] {depth, result};
                    return result;
                }
                if (score.GetEval() > alpha)
                {
                    alpha = score.GetEval();
                    bestMove = new MoveDouble(move, alpha);
                    if (alpha == 1000)
                    {
                        order[board.ZobristKey] = new object[] { depth, bestMove };
                        return bestMove;
                    }
                }
            }
            else
            {
                if (score.GetEval() <= alpha)
                {
                    MoveDouble result = new MoveDouble(move, score.GetEval());
                    order[board.ZobristKey] = new object[] { depth, result };
                    return result;
                }
                if (score.GetEval() < beta)
                {
                    beta = score.GetEval();
                    bestMove = new MoveDouble(move, beta);
                    byte bestMoveIndex = i;
                    if (beta == -1000)
                    {
                        order[board.ZobristKey] = new object[] { depth, bestMove };
                        return bestMove;
                    }
                }
            }
        }
        order[board.ZobristKey] = new object[] { depth, bestMove };
        return bestMove;
    }

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
        bool isEndgame = (white < 13) && (black < 13);
        int undevelopedWhitePieces = NumberOfSetBits(getPiecesOnFirstRank(true));
        int undevelopedBlackPieces = NumberOfSetBits(getPiecesOnFirstRank(false));
        int whiteCenterPawns = isEndgame ? 0 : NumberOfSetBits(board.GetPieceBitboard(PieceType.Pawn, true) & CENTER);
        int blackCenterPawns = isEndgame ? 0 : NumberOfSetBits(board.GetPieceBitboard(PieceType.Pawn, false) & CENTER);
        Square whiteKingSquare = board.GetKingSquare(true);
        Square blackKingSquare = board.GetKingSquare(false);
        int kingDist = Math.Max(Math.Abs(whiteKingSquare.Rank-blackKingSquare.Rank), Math.Abs(blackKingSquare.File-whiteKingSquare.File));
        double whiteKingScore = isEndgame ? -kingDist/16d : ((8 - whiteKingSquare.Rank) / 16d + ((whiteKingSquare.File == 6 || whiteKingSquare.File == 2) ? 0.5 : 0));
        double blackKingScore = isEndgame ? -kingDist/16d : (-(8 - blackKingSquare.Rank) / 16d + ((blackKingSquare.File == 6 || blackKingSquare.File == 2) ? 0.5 : 0));
        white += -undevelopedWhitePieces / 5d + whiteCenterPawns / 4d + whiteKingScore;
        black += -undevelopedBlackPieces / 5d + blackCenterPawns / 4d + blackKingScore;
        double eval = (white - black);
        return eval;
    }

    private ulong getPiecesOnFirstRank(bool isWhite)
    {
        return ((isWhite ? board.WhitePiecesBitboard : board.BlackPiecesBitboard) ^ board.GetPieceBitboard(PieceType.King, isWhite) ^ board.GetPieceBitboard(PieceType.Pawn, isWhite) ^ board.GetPieceBitboard(PieceType.Rook, isWhite) ^ board.GetPieceBitboard(PieceType.Queen, isWhite)) & (isWhite ? FIRST_RANK : LAST_RANK);
    }

    static int NumberOfSetBits(ulong i)
    {
        i = i - ((i >> 1) & 0x5555555555555555UL);
        i = (i & 0x3333333333333333UL) + ((i >> 2) & 0x3333333333333333UL);
        return (int)(unchecked(((i + (i >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
    }
}

class MoveDouble
{
    Move move;
    double eval;

    public MoveDouble(Move lastMove, double v)
    {
        move = lastMove;
        eval = v;
    }

    public Move GetMove() { return move; }

    public double GetEval() { return eval; }
}