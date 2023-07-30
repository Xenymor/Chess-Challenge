using ChessChallenge.API;
using System;
using System.Collections.Generic;

namespace ChessChallenge.EvilBot2_1
{
    internal class EvilBot : IChessBot
    {
        private Board board;
        private int width = 5;
        private int depth = 6;

        public Move Think(Board board, Timer timer)
        {
            this.board = board;
            Move[] moves = board.GetLegalMoves();
            Array.Sort(moves, new MyMoveComparer(board));
            MoveDouble bestMove = new MoveDouble(new Move(), double.MinValue);
            bool iAmWhite = board.IsWhiteToMove;
            for (int i = 0; i < ((moves.Length < width) ? moves.Length : width); i++)
            {
                board.MakeMove(moves[i]);
                MoveDouble result = FindBestMove(width, depth, iAmWhite, moves[i]);
                if (result.GetEval() >= bestMove.GetEval())
                {
                    bestMove = result;
                }
                board.UndoMove(moves[i]);
            }
            return bestMove.GetMove();
        }

        private MoveDouble FindBestMove(int width, int depth, bool iAmWhite, Move lastMove)
        {
            if (depth <= 0 || board.IsInCheckmate() || board.IsDraw())
            {
                return new MoveDouble(lastMove, EvaluatePosition(iAmWhite));
            }

            Move[] moves = board.GetLegalMoves();
            if (moves.Length == 0)
            {
                return new MoveDouble(lastMove, EvaluatePosition(iAmWhite));
            }
            Array.Sort(moves, new MyMoveComparer(board));
            MoveDouble bestMove = new MoveDouble(new Move(), (board.IsWhiteToMove == iAmWhite) ? double.MinValue : double.MaxValue);
            for (int i = 0; i < ((moves.Length < width) ? moves.Length : width); i++)
            {
                board.MakeMove(moves[i]);
                MoveDouble result = new MoveDouble(lastMove, FindBestMove(width, depth - 1, iAmWhite, moves[i]).GetEval());
                board.UndoMove(moves[i]);
                if (iAmWhite == board.IsWhiteToMove)
                {
                    if (result.GetEval() >= bestMove.GetEval())
                    {
                        bestMove = result;
                    }
                }
                else if (result.GetEval() < bestMove.GetEval())
                {
                    bestMove = result;
                }
            }
            if (bestMove.GetMove().Equals(new Move()))
            {
                Console.WriteLine(bestMove.GetMove() + " " + bestMove.GetEval());
            }
            return bestMove;
        }

        private double EvaluatePosition(bool amIWhite)
        {
            if (board.IsInCheckmate())
            {
                return board.IsWhiteToMove ^ amIWhite ? 1000 : -1000;
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

            double eval = (white - black) * (amIWhite ? 1 : -1);
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

        internal void SetEval(double v)
        {
            eval = v;
        }
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
            result = sort(result, movesForward(x, board.IsWhiteToMove), movesForward(y, board.IsWhiteToMove));
            return result;
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

        private bool canCapture(Square targetSquare)
        {
            foreach (Move m in board.GetLegalMoves(true))
            {
                if (m.TargetSquare.Equals(targetSquare))
                {
                    return true;
                }
            }
            return false;
        }

        private bool isCheck(Move x)
        {
            board.MakeMove(x);
            Boolean isCheck = board.IsInCheck();
            board.UndoMove(x);
            return isCheck;
        }

        private int sort(int old, Boolean a, Boolean b)
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

    }
}
