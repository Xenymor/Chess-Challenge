using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    //This bot is a combination of ErwanF's Nano and the evaluation of Smol. I simplified the evaluation quite a bit to only include the PSTS.
    //After that I optimized the tokens further and added PVS.
    Move bestRootMove;

    static sbyte[] extracted = new[] { 4835740172228143389605888m, 1862983114964290202813595648m, 6529489037797228073584297991m, 6818450810788061916507740187m, 7154536855449028663353021722m, 14899014974757699833696556826m, 25468819436707891759039590695m, 29180306561342183501734565961m, 944189991765834239743752701m, 4194697739m, 4340114601700738076711583744m, 3410436627687897068963695623m, 11182743911298765866015857947m, 10873240011723255639678263585m, 17684436730682332602697851426m, 17374951722591802467805509926m, 31068658689795177567161113954m, 1534136309681498319279645285m, 18014679997410182140m, 1208741569195510172352512m, 13789093343132567021105512448m, 6502873946609222871099113472m, 1250m }.SelectMany(x => decimal.GetBits(x).Take(3).SelectMany(y => (sbyte[])(Array)BitConverter.GetBytes(y))).ToArray();

    int[] evalValues = Enumerable.Range(0, 138).Select(i => extracted[i * 2] | extracted[i * 2 + 1] << 16).ToArray();

    public Move Think(Board board, Timer timer)
    {
        int searchDepth = 1;

        int Search(int depth, int alpha, int beta)
        {
            int score = 0, phase = 0;
            if (depth <= 0)
            {
                foreach (bool isWhite in new[] { !board.IsWhiteToMove, board.IsWhiteToMove })
                {
                    score = -score;

                    for (var pieceIndex = 0; ++pieceIndex <= 6;)
                    {
                        var bitboard = board.GetPieceBitboard((PieceType)pieceIndex, isWhite);

                        while (bitboard != 0)
                        {
                            var sq = BitboardHelper.ClearAndGetIndexOfLSB(ref bitboard);

                            if (!isWhite) sq ^= 56;

                            phase += evalValues[pieceIndex];

                            score += evalValues[pieceIndex * 8 + sq / 8]
                                   + evalValues[56 + pieceIndex * 8 + sq % 8]
                                   << 3;
                        }
                    }
                }
                alpha = Math.Max(alpha, score);
            }

            foreach (Move move in board.GetLegalMoves(depth <= 0)
                .OrderByDescending(move => (move == bestRootMove ? 1 : 0, move.CapturePieceType, 0 - move.MovePieceType)))
            {
                if (alpha >= beta)
                    break;

                board.MakeMove(move);
                int S(int newAlpha) => score = -Search(depth - 1, -newAlpha, -alpha);
                score =
                    board.IsDraw() ? 0 :
                    board.IsInCheckmate() ? 1_999_999_999 :
                    alpha < S(alpha + 1) ? S(beta) : score;

                if (score > alpha)
                {
                    alpha = score;
                    if (depth == searchDepth)
                        bestRootMove = move;
                }

                if (timer.MillisecondsElapsedThisTurn * 25 >= timer.MillisecondsRemaining)
                    depth /= 0;
                //Convert.ToUInt32(timer.MillisecondsRemaining - timer.MillisecondsElapsedThisTurn * 25); 1 token shorter (untested)

                board.UndoMove(move);
            }

            return alpha;
        }

        try
        {
            for (; ; )
            {
                Search(++searchDepth, -2_000_000_000, 2_000_000_000);
            }
        }
        catch { }

        return bestRootMove;
    }
}