using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    Move bestRootMove;

    public int getPstVal(int psq)
    {
        return (int)(((psts[psq / 10] >> (6 * (psq % 10))) & 63) - 20) * 8;
    }

    int[] pieceVal = { 0, 100, 310, 330, 500, 1000, 10000 };
    int[] piecePhase = { 0, 0, 1, 1, 2, 4, 0 };
    ulong[] psts = { 657614902731556116, 420894446315227099, 384592972471695068, 312245244820264086, 364876803783607569, 366006824779723922, 366006826859316500, 786039115310605588, 421220596516513823, 366011295806342421, 366006826859316436, 366006896669578452, 162218943720801556, 440575073001255824, 657087419459913430, 402634039558223453, 347425219986941203, 365698755348489557, 311382605788951956, 147850316371514514, 329107007234708689, 402598430990222677, 402611905376114006, 329415149680141460, 257053881053295759, 291134268204721362, 492947507967247313, 367159395376767958, 384021229732455700, 384307098409076181, 402035762391246293, 328847661003244824, 365712019230110867, 366002427738801364, 384307168185238804, 347996828560606484, 329692156834174227, 365439338182165780, 386018218798040211, 456959123538409047, 347157285952386452, 365711880701965780, 365997890021704981, 221896035722130452, 384289231362147538, 384307167128540502, 366006826859320596, 366006826876093716, 366002360093332756, 366006824694793492, 347992428333053139, 457508666683233428, 329723156783776785, 329401687190893908, 366002356855326100, 366288301819245844, 329978030930875600, 420621693221156179, 422042614449657239, 384602117564867863, 419505151144195476, 366274972473194070, 329406075454444949, 275354286769374224, 366855645423297932, 329991151972070674, 311105941360174354, 256772197720318995, 365993560693875923, 258219435335676691, 383730812414424149, 384601907111998612, 401758895947998613, 420612834953622999, 402607438610388375, 329978099633296596, 67159620133902 };

    public Move Think(Board board, Timer timer)
    {

        int Evaluate()
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
                        ind = ((piece - 1) << 7) + square ^ (stm ? 56 : 0);

                        //Piece Square Values
                        middleGame += getPstVal(ind) + pieceVal[piece];
                        endGame += getPstVal(ind + 64) + pieceVal[piece];
                    }
                }
                middleGame = -middleGame;
                endGame = -endGame;
                stm = !stm;
            } while (!stm);
            return (middleGame * phase + endGame * (24 - phase)) / (board.IsWhiteToMove ? 24 : -24);
        }

        int searchDepth = 0;

        int Search(int depth, int alpha, int beta)
        {

            // Quiescence & eval
            if (depth <= 0)
                alpha = Math.Max(alpha, Evaluate());  //eval = material + mobility
                                                                                         // no beta cutoff check here, it will be done latter

            foreach (Move move in board.GetLegalMoves(depth <= 0)
                .OrderByDescending(move => (move == bestRootMove ? 1 : 0, move.CapturePieceType, 0 - move.MovePieceType)))
            {
                if (alpha >= beta)
                    break;

                board.MakeMove(move);

                int score =
                    board.IsDraw() ? 0 :
                    board.IsInCheckmate() ? 30000 :
                    -Search(depth - 1, -beta, -alpha);

                if (score > alpha)
                {
                    alpha = score;
                    if (depth == searchDepth)
                        bestRootMove = move;
                }

                // Check timer now: after updating best root move (so no illegal move), but before UndoMove (which takes some time)
                if (timer.MillisecondsElapsedThisTurn * 30 >= timer.MillisecondsRemaining)
                    depth /= 0;

                board.UndoMove(move);
            }

            return alpha;
        }

        try
        {
            for (; ; )
                Search(++searchDepth, -40000, 40000);
        }
        catch { }

        return bestRootMove;
    }
}