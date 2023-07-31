using ChessChallenge.API;
using System.Collections.Generic;

namespace ChessChallenge.EvilBot3_2
{
    internal class EvilBot : IChessBot
    {
        private const ulong FIRST_RANK = 255;
        private const ulong LAST_RANK = 18374686479671623680;
        private const ulong CENTER = 103481868288;
        Board board;
        int depth = 20;
        Dictionary<ulong, byte> order;
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
            order = new Dictionary<ulong, byte>();
            this.board = board;
            MoveDouble bestMove = new MoveDouble(new Move(), double.NaN);
            int depthCalculated = 0;
            for (int i = 0; i < depth; i++)
            {
                bestMove = alphaBeta(double.MinValue, double.MaxValue, i);
                if (timer.MillisecondsElapsedThisTurn >= timeForMove)
                {
                    depthCalculated = i;
                    break;
                }
            }
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
                        order[board.ZobristKey] = (byte)(i == 0 ? index : i == index ? 0 : i);
                        return new MoveDouble(move, score.GetEval());
                    }
                    if (score.GetEval() > alpha)
                    {
                        alpha = score.GetEval();
                        bestMove = new MoveDouble(move, alpha);
                        bestMoveIndex = i;
                        if (alpha == 1000)
                        {
                            order[board.ZobristKey] = (byte)(i == 0 ? index : i == index ? 0 : i);
                            return bestMove;
                        }
                    }
                }
                else
                {
                    if (score.GetEval() <= alpha)
                    {
                        order[board.ZobristKey] = (byte)(i == 0 ? index : i == index ? 0 : i);
                        return new MoveDouble(move, score.GetEval());
                    }
                    if (score.GetEval() < beta)
                    {
                        beta = score.GetEval();
                        bestMove = new MoveDouble(move, beta);
                        bestMoveIndex = i;
                        if (beta == -1000)
                        {
                            order[board.ZobristKey] = (byte)(i == 0 ? index : i == index ? 0 : i);
                            return bestMove;
                        }
                    }
                }
            }
            bestMoveIndex = (byte)((bestMoveIndex == index) ? 0 : (bestMoveIndex == 0) ? index : bestMoveIndex);
            order[board.ZobristKey] = bestMoveIndex;
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
            int undevelopedWhitePieces = NumberOfSetBits((board.WhitePiecesBitboard ^ board.GetPieceBitboard(PieceType.King, true) ^ board.GetPieceBitboard(PieceType.Pawn, true) ^ board.GetPieceBitboard(PieceType.Rook, true)) & FIRST_RANK);
            int undevelopedBlackPieces = NumberOfSetBits((board.BlackPiecesBitboard ^ board.GetPieceBitboard(PieceType.King, false) ^ board.GetPieceBitboard(PieceType.Pawn, false) ^ board.GetPieceBitboard(PieceType.Rook, false)) & LAST_RANK);
            int whiteCenterPawns = NumberOfSetBits(board.GetPieceBitboard(PieceType.Pawn, true) & CENTER);
            int blackCenterPawns = NumberOfSetBits(board.GetPieceBitboard(PieceType.Pawn, false) & CENTER);
            white += -undevelopedWhitePieces / 5d + whiteCenterPawns / 4d;
            black += -undevelopedBlackPieces / 5d + blackCenterPawns / 4d;
            double eval = (white - black);
            return eval;
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
            this.move = lastMove;
            this.eval = v;
        }

        public Move GetMove() { return move; }

        public double GetEval() { return eval; }
    }
}
