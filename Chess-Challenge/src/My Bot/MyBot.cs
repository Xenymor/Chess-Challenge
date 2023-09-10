using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    private const int CHECKMATE_SCORE = 100_000;
    public static Board board;
    int depth = 40;
    Dictionary<ulong, byte> order = new Dictionary<ulong, byte>();
    int moveEstimate = 200;
    Random rand = new Random();
    int[] pieceVal = { 0, 100, 310, 330, 500, 1000, 10000 };
    int[] piecePhase = { 0, 0, 1, 1, 2, 4, 0 };
    ulong[] psts = { 657614902731556116, 420894446315227099, 384592972471695068, 312245244820264086, 364876803783607569, 366006824779723922, 366006826859316500, 786039115310605588, 421220596516513823, 366011295806342421, 366006826859316436, 366006896669578452, 162218943720801556, 440575073001255824, 657087419459913430, 402634039558223453, 347425219986941203, 365698755348489557, 311382605788951956, 147850316371514514, 329107007234708689, 402598430990222677, 402611905376114006, 329415149680141460, 257053881053295759, 291134268204721362, 492947507967247313, 367159395376767958, 384021229732455700, 384307098409076181, 402035762391246293, 328847661003244824, 365712019230110867, 366002427738801364, 384307168185238804, 347996828560606484, 329692156834174227, 365439338182165780, 386018218798040211, 456959123538409047, 347157285952386452, 365711880701965780, 365997890021704981, 221896035722130452, 384289231362147538, 384307167128540502, 366006826859320596, 366006826876093716, 366002360093332756, 366006824694793492, 347992428333053139, 457508666683233428, 329723156783776785, 329401687190893908, 366002356855326100, 366288301819245844, 329978030930875600, 420621693221156179, 422042614449657239, 384602117564867863, 419505151144195476, 366274972473194070, 329406075454444949, 275354286769374224, 366855645423297932, 329991151972070674, 311105941360174354, 256772197720318995, 365993560693875923, 258219435335676691, 383730812414424149, 384601907111998612, 401758895947998613, 420612834953622999, 402607438610388375, 329978099633296596, 67159620133902 };
    Move bestRootMove = Move.NullMove;

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
        MyBot.board = board;
        order = new Dictionary<ulong, byte>();
        int depthCalculated = 0; //#DEBUG
        bool broke = false; //#DEBUG
        int eval = 0;
        for (int i = 0; i < depth; i++)
        {
            eval = alphaBeta(int.MinValue, int.MaxValue, i, true);
            if (timer.MillisecondsElapsedThisTurn >= timeForMove)
            {
                depthCalculated = i+1; //#DEBUG
                broke = true; //#DEBUG
                break;
            }
        }
        if (!broke) //#DEBUG
            depthCalculated = depth; //#DEBUG
        Console.WriteLine("MyBot: " + eval / 100d + "; depth: " + depthCalculated); //#DEBUG
        return bestRootMove;
    }

    private int alphaBeta(int alpha, int beta, int depth, bool isFirstCall)
    {
        if (!isFirstCall)
            if (depth <= 0 || board.IsDraw() || board.IsInCheckmate())
                return Evaluate();
        Move[] moves = board.GetLegalMoves();
        if (moves.Length == 0)
            return Evaluate();
        if (order.TryGetValue(board.ZobristKey, out byte index))
            if (index < moves.Length)
                (moves[index], moves[0]) = (moves[0], moves[index]);
        byte bestMoveIndex = 0;
        Move bestMove = new Move();
        int bestEval = board.IsWhiteToMove ? int.MaxValue-1 : int.MinValue+1;
        for (byte i = 0; i < moves.Length; i++)
        {
            Move move = moves[i];
            board.MakeMove(move);
            int score = alphaBeta(alpha, beta, depth - 1, false);
            board.UndoMove(move);
            if (board.IsWhiteToMove)
            {
                if (score >= beta)
                {
                    order[board.ZobristKey] = (byte)(i == 0 ? index : i == index ? 0 : i);
                    return score;
                }
                if (score > alpha)
                {
                    alpha = score;
                    bestEval = score;
                    bestMoveIndex = i;
                    bestMove = move;
                    if (alpha == CHECKMATE_SCORE)
                    {
                        if (isFirstCall)
                        {
                            bestRootMove = move;
                        }
                        order[board.ZobristKey] = (byte)(i == 0 ? index : i == index ? 0 : i);
                        return score;
                    }
                }
            }
            else
            {
                if (score <= alpha)
                {
                    order[board.ZobristKey] = (byte)(i == 0 ? index : i == index ? 0 : i);
                    return score;
                }
                if (score < beta)
                {
                    beta = score;
                    bestEval = score;
                    bestMoveIndex = i;
                    bestMove = move;
                    if (beta == -CHECKMATE_SCORE)
                    {
                        if (isFirstCall)
                        {
                            bestRootMove = move;
                        }
                        order[board.ZobristKey] = (byte)(i == 0 ? index : i == index ? 0 : i);
                        return score;
                    }
                }
            }
        }
        bestMoveIndex = (byte)((bestMoveIndex == index) ? 0 : (bestMoveIndex == 0) ? index : bestMoveIndex);
        order[board.ZobristKey] = bestMoveIndex;
        if (isFirstCall)
        {
            bestRootMove = bestMove;
        }
        return bestEval;
    }

    public int getPstVal(int psq)
    {
        return (int)(((psts[psq / 10] >> (6 * (psq % 10))) & 63) - 20) * 8;
    }

    public int Evaluate()
    {
        if (board.IsInCheckmate())
        {
            return CHECKMATE_SCORE * (board.IsWhiteToMove ? -1 : 1);
        }
        if (board.IsDraw())
        {
            return 0;
        }
        int mg = 0, eg = 0, phase = 0;

        foreach (bool stm in new[] { true, false })
        {
            for (var p = PieceType.Pawn; p <= PieceType.King; p++)
            {
                int piece = (int)p, ind;
                ulong mask = board.GetPieceBitboard(p, stm);
                while (mask != 0)
                {
                    phase += piecePhase[piece];
                    ind = 128 * (piece - 1) + BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^ (stm ? 56 : 0);
                    mg += getPstVal(ind) + pieceVal[piece];
                    eg += getPstVal(ind + 64) + pieceVal[piece];
                }
            }

            mg = -mg;
            eg = -eg;
        }

        return (mg * phase + eg * (24 - phase)) / 24 + rand.Next(-2, 2);
    }
}