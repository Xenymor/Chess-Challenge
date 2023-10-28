using ChessChallenge.API;
using ChessChallenge.Application;
using System;

namespace Chess_Challenge.src.EvilBot6_3
{
    public class EvilBot6_3 : IChessBot
    {
        Move bestRootMove = Move.NullMove;

        struct TTEntry
        {
            public ulong key;
            public Move move;
            public int depth, score, bound;
            public TTEntry(ulong newKey, Move newMove, int newDepth, int newScore, int newBound)
            {
                key = newKey; move = newMove; depth = newDepth; score = newScore; bound = newBound;
            }
        }

        const int ENTRIES = (1 << 20);
        TTEntry[] tt = new TTEntry[ENTRIES];

        int[] pieceVal = { 0, 100, 310, 330, 500, 1000, 10000 };
        int[] piecePhase = { 0, 0, 1, 1, 2, 4, 0 };
        ulong[] psts = { 657614902731556116, 420894446315227099, 384592972471695068, 312245244820264086, 364876803783607569, 366006824779723922, 366006826859316500, 786039115310605588, 421220596516513823, 366011295806342421, 366006826859316436, 366006896669578452, 162218943720801556, 440575073001255824, 657087419459913430, 402634039558223453, 347425219986941203, 365698755348489557, 311382605788951956, 147850316371514514, 329107007234708689, 402598430990222677, 402611905376114006, 329415149680141460, 257053881053295759, 291134268204721362, 492947507967247313, 367159395376767958, 384021229732455700, 384307098409076181, 402035762391246293, 328847661003244824, 365712019230110867, 366002427738801364, 384307168185238804, 347996828560606484, 329692156834174227, 365439338182165780, 386018218798040211, 456959123538409047, 347157285952386452, 365711880701965780, 365997890021704981, 221896035722130452, 384289231362147538, 384307167128540502, 366006826859320596, 366006826876093716, 366002360093332756, 366006824694793492, 347992428333053139, 457508666683233428, 329723156783776785, 329401687190893908, 366002356855326100, 366288301819245844, 329978030930875600, 420621693221156179, 422042614449657239, 384602117564867863, 419505151144195476, 366274972473194070, 329406075454444949, 275354286769374224, 366855645423297932, 329991151972070674, 311105941360174354, 256772197720318995, 365993560693875923, 258219435335676691, 383730812414424149, 384601907111998612, 401758895947998613, 420612834953622999, 402607438610388375, 329978099633296596, 67159620133902 };
        ushort[] killers = new ushort[2000];

        Board board;
        Timer timer;

        public int getPstVal(int psq)
        {
            return (int)(((psts[psq / 10] >> (6 * (psq % 10))) & 63) - 20) * 8;
        }

        public int Evaluate(Board board)
        {
            int middleGame = 0, endGame = 0, phase = 0;
            bool stm = true;
            do
            {
                for (var p = PieceType.Pawn; p <= PieceType.King; p++)
                {
                    int piece = (int)p, ind;
                    ulong mask = board.GetPieceBitboard(p, stm);
                    while (mask != 0)
                    {
                        phase += piecePhase[piece];
                        int square = BitboardHelper.ClearAndGetIndexOfLSB(ref mask);
                        ind = 128 * (piece - 1) + square ^ (stm ? 56 : 0);

                        //Piece Square Values
                        middleGame += getPstVal(ind) + pieceVal[piece];
                        endGame += getPstVal(ind + 64) + pieceVal[piece];

                        //Mobility Bonus
                        if ((int)p >= 2 && (int)p <= 4)
                        {
                            int bonus = BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPieceAttacks((PieceType)piece + 1, new Square(square), board, stm));
                            middleGame += bonus;
                            endGame += bonus * 2;
                        }

                        // Bishop pair bonus
                        if (piece == 2 && mask != 0)
                        {
                            middleGame += 23;
                            endGame += 62;
                        }

                        // Doubled pawns penalty
                        if (piece == 0 && (0x101010101010101UL << (square & 7) & mask) > 0)
                        {
                            middleGame -= 15;
                            endGame -= 30;
                        }
                    }
                }
                middleGame = -middleGame;
                endGame = -endGame;
                stm = !stm;
            } while (!stm);
            int score = ((middleGame * phase + endGame * (24 - phase)) / 24) + 16;
            return score * (board.IsWhiteToMove ? 1 : -1);
        }

        public int TunerEvaluate(Board board, float[] parameters) //#DEBUG
        {
            float middleGame = 0, endGame = 0, phase = 0; //#DEBUG
            bool stm = true; //#DEBUG
            do //#DEBUG
            { //#DEBUG
                for (var p = PieceType.Pawn; p <= PieceType.King; p++) //#DEBUG
                { //#DEBUG
                    int piece = (int)p, ind; //#DEBUG
                    ulong mask = board.GetPieceBitboard(p, stm); //#DEBUG
                    while (mask != 0) //#DEBUG
                    { //#DEBUG
                        phase += piecePhase[piece]; //#DEBUG
                        int square = BitboardHelper.ClearAndGetIndexOfLSB(ref mask); //#DEBUG
                        ind = 128 * (piece - 1) + square ^ (stm ? 56 : 0); //#DEBUG

                        //Piece Square Values
                        middleGame += getPstVal(ind) + parameters[piece + 6]; //#DEBUG
                        endGame += getPstVal(ind + 64) + parameters[piece + 6]; //#DEBUG

                        //Mobility Bonus
                        if ((int)p >= 2 && (int)p <= 4) //#DEBUG
                        { //#DEBUG
                            int bonus = BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPieceAttacks((PieceType)piece + 1, new Square(square), board, stm)); //#DEBUG
                            middleGame += bonus * parameters[2]; //#DEBUG
                            endGame += bonus * parameters[3]; //#DEBUG
                        } //#DEBUG

                        // Bishop pair bonus
                        if (piece == 2 && mask != 0) //#DEBUG
                        { //#DEBUG
                            middleGame += parameters[0]; //#DEBUG
                            endGame += parameters[1]; //#DEBUG
                        } //#DEBUG

                        // Doubled pawns penalty
                        if (piece == 0 && (0x101010101010101UL << (square & 7) & mask) > 0) //#DEBUG
                        { //#DEBUG
                            middleGame -= parameters[4]; //#DEBUG
                            endGame -= parameters[5]; //#DEBUG
                        } //#DEBUG
                    } //#DEBUG
                } //#DEBUG
                middleGame = -middleGame; //#DEBUG
                endGame = -endGame; //#DEBUG
                stm = !stm; //#DEBUG
            } while (!stm); //#DEBUG
            float score = ((middleGame * phase + endGame * (24 - phase)) / 24) + 16; //#DEBUG
            return (int)(score * (board.IsWhiteToMove ? 1 : -1)); //#DEBUG
        } //#DEBUG

        public int AlphaBeta(int alpha, int beta, int depth, int ply, int extension)
        {
            ulong key = board.ZobristKey;
            bool qsearch = depth <= 0;
            bool notFirstCall = ply > 0;
            int bestScore = -30000;

            if (notFirstCall && board.IsRepeatedPosition())
                return 0;

            TTEntry entry = tt[key % ENTRIES];

            if (notFirstCall && entry.key == key && entry.depth >= depth && (
                entry.bound == 3
                    || entry.bound == 2 && entry.score >= beta
                    || entry.bound == 1 && entry.score <= alpha
            )) return entry.score;

            int eval = Evaluate(board);

            if (qsearch)
            {
                bestScore = eval;
                if (bestScore >= beta) return bestScore;
                alpha = Math.Max(alpha, bestScore);
            }

            Move[] moves = board.GetLegalMoves(qsearch);
            int[] scores = new int[moves.Length];

            for (int i = 0; i < moves.Length; i++)
            {
                Move move = moves[i];
                scores[i] = (move == entry.move ? 1_000_000 :
                killers[ply] == move.RawValue ? 900_000 :
                move.IsCapture ? 100 * (int)move.CapturePieceType - (int)move.MovePieceType
                : 0);
            }

            Move bestMove = Move.NullMove;
            int origAlpha = alpha;

            for (int i = 0; i < moves.Length; i++)
            {
                if (
                    (Settings.TimeForMove != 0 && timer.MillisecondsElapsedThisTurn >= Settings.TimeForMove)//#DEBUG
                    || (Settings.TimeForMove == 0 && //#DEBUG
                    timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30)) return 30000;

                for (int j = i + 1; j < moves.Length; j++)
                {
                    if (scores[j] > scores[i])
                        (scores[i], scores[j], moves[i], moves[j]) = (scores[j], scores[i], moves[j], moves[i]);
                }

                Move move = moves[i];
                board.MakeMove(move);
                int newExtension = qsearch ? 0 : (board.IsInCheck() ? 1 : ((move.MovePieceType == PieceType.Pawn && (move.TargetSquare.Rank == 6 || move.TargetSquare.Rank == 1)) ? 1 : 0));
                int score = -AlphaBeta(-(alpha + 1), -alpha, depth - 1 + newExtension, ply + 1, newExtension + extension);
                if (score > alpha && score < beta)
                {
                    score = Math.Max(score, -AlphaBeta(-beta, -alpha, depth - 1 + newExtension, ply + 1, newExtension + extension));
                }
                board.UndoMove(move);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                    if (ply == 0) bestRootMove = move;

                    alpha = Math.Max(alpha, score);

                    if (alpha >= beta) break;

                }
            }

            if (!qsearch && moves.Length == 0) return board.IsInCheck() ? -30_000 + ply * 1000 : 0;

            int bound = bestScore >= beta ? 2 : bestScore > origAlpha ? 3 : 1;

            if (bound == 2)
            {
                killers[ply] = bestMove.RawValue;
            }

            tt[key % ENTRIES] = new TTEntry(key, bestMove, depth, bestScore, bound);

            return bestScore;
        }

        int lastEval = 0;

        public Move Think(Board board, Timer timer)
        {
            this.board = board;
            this.timer = timer;
            bestRootMove = Move.NullMove;
            int calculatedDepth = 0;
            int eval = lastEval;
            int alpha = eval - 25;
            int beta = eval + 25;
            while (calculatedDepth < 50)
            {
                eval = AlphaBeta(alpha, beta, calculatedDepth, 0, 0);
                if (eval <= alpha)
                    alpha -= 60;
                else if (eval >= beta)
                    beta += 60;
                else
                {
                    calculatedDepth++;
                    alpha = eval - 25;
                    beta = eval + 25;
                    lastEval = eval; //#DEBUG
                }
                if ((Settings.TimeForMove != 0 && timer.MillisecondsElapsedThisTurn >= Settings.TimeForMove) //#DEBUG
                        || (Settings.TimeForMove == 0 && //#DEBUG
                        timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30))
                {
                    eval = lastEval; //#DEBUG
                    break;
                }
            }
            Console.WriteLine("MyBot: " + EvalToString(eval / -100f) + ";\tDepth: " + calculatedDepth); //#DEBUG
            MatchStatsUI.depthSum1 += calculatedDepth; //#DEBUG
            MatchStatsUI.movesPlayed1++; //#DEBUG
            return bestRootMove.IsNull ? board.GetLegalMoves()[0] : bestRootMove;
        }

        private string EvalToString(float v)
        {
            if (v >= 100)
            {
                return "M" + (300 - v) / 10;
            }
            else if (v <= -100)
            {
                return "M" + (-300 - v) / 10;
            }
            return v.ToString();
        }
    }

}