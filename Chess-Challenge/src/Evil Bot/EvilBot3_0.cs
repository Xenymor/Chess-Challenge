using ChessChallenge.API;
using System;
using System.Collections.Generic;

namespace ChessChallenge.EvilBot3_0
{
    internal class EvilBot : IChessBot
    {
        Board board;
        int depth = 5;

        public Move Think(Board board, Timer timer)
        {
            this.board = board;
            MoveDouble bestMove = alphaBeta(double.MinValue, double.MaxValue, depth);
            Console.WriteLine("EvilBot: " + bestMove.GetEval());
            return bestMove.GetMove();
        }

        private MoveDouble alphaBeta(double alpha, double beta, int depth)
        {
            Move[] moves = board.GetLegalMoves();
            if (depth <= 0 || moves.Length == 0 || board.IsDraw() || board.IsInCheckmate())
            {
                return new MoveDouble(new Move(), EvaluatePosition());
            }
            Array.Sort(moves, new MyMoveComparer(board));
            MoveDouble bestMove = new MoveDouble(new Move(), !board.IsWhiteToMove ? double.MaxValue : double.MinValue);
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                MoveDouble score = alphaBeta(alpha, beta, depth - 1);
                board.UndoMove(move);
                if (board.IsWhiteToMove)
                {
                    if (score.GetEval() >= beta)
                    {
                        return new MoveDouble(move, score.GetEval());
                    }
                    if (score.GetEval() > alpha)
                    {
                        alpha = score.GetEval();
                        bestMove = new MoveDouble(move, alpha);
                        if (alpha == 1000)
                        {
                            return bestMove;
                        }
                    }

                }
                else
                {
                    if (score.GetEval() <= alpha)
                    {
                        return new MoveDouble(move, score.GetEval());
                    }
                    if (score.GetEval() < beta)
                    {
                        beta = score.GetEval();
                        bestMove = new MoveDouble(move, beta);
                        if (beta == -1000)
                        {
                            return bestMove;
                        }
                    }
                }
            }
            /*if (bestMove.GetMove().Equals(new Move())) {
                Console.WriteLine("alpha: " + alpha + "; beta: " + beta + "; PlayerToMove: " + board.IsWhiteToMove + "; Depth remaining: " + depth);
                Environment.Exit(-1);
            }*/
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
            result = sort(result, movesForward(y, board.IsWhiteToMove), movesForward(x, board.IsWhiteToMove));
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
}
