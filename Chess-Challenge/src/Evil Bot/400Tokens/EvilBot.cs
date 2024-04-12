using ChessChallenge.API;
using System;
using System.Linq;

namespace Chess_Challenge.src.EvilBot
{
    public class _400EvilBot : IChessBot
    {
        Move bestRootMove;

        // Due to the rules of the challenge and how token counting works, evaluation constants are packed into C# decimals,
        // as they allow the most efficient (12 usable bits per token).
        // The ordering is as follows: Midgame term 1, endgame term 1, midgame, term 2, endgame term 2...
        static sbyte[] extracted = new[] { 4835740172228143389605888m, 1862983114964290202813595648m, 6529489037797228073584297991m, 6818450810788061916507740187m, 7154536855449028663353021722m, 14899014974757699833696556826m, 25468819436707891759039590695m, 29180306561342183501734565961m, 944189991765834239743752701m, 4194697739m, 4340114601700738076711583744m, 3410436627687897068963695623m, 11182743911298765866015857947m, 10873240011723255639678263585m, 17684436730682332602697851426m, 17374951722591802467805509926m, 31068658689795177567161113954m, 1534136309681498319279645285m, 18014679997410182140m, 1208741569195510172352512m, 13789093343132567021105512448m, 6502873946609222871099113472m, 1250m }.SelectMany(x => decimal.GetBits(x).Take(3).SelectMany(y => (sbyte[])(Array)BitConverter.GetBytes(y))).ToArray();

        // After extracting the raw mindgame/endgame terms, we repack it into integers of midgame/endgame pairs.
        // The scheme in bytes (assuming little endian) is: 00 EG 00 MG
        // The idea of this is that we can do operations on both midgame and endgame values simultaneously, preventing the need
        // for evaluation for separate mid-game / end-game terms.
        int[] evalValues = Enumerable.Range(0, 138).Select(i => extracted[i * 2] | extracted[i * 2 + 1] << 16).ToArray();

        public Move Think(Board board, Timer timer)
        {
            int Evaluate()
            {
                int score = 0, phase = 0;
                foreach (bool isWhite in new[] { !board.IsWhiteToMove, board.IsWhiteToMove })
                {
                    score = -score;

                    //       None (skipped)               King
                    for (var pieceIndex = 0; ++pieceIndex <= 6;)
                    {
                        var bitboard = board.GetPieceBitboard((PieceType)pieceIndex, isWhite);

                        // This and the following line is an efficient way to loop over each piece of a certain type.
                        // Instead of looping each square, we can skip empty squares by looking at a bitboard of each piece,
                        // and incrementally removing squares from it. More information: https://www.chessprogramming.org/Bitboards
                        while (bitboard != 0)
                        {
                            var sq = BitboardHelper.ClearAndGetIndexOfLSB(ref bitboard);

                            // Flip square if black.
                            // This is needed for piece square tables (PSTs), because they are always written from the side that is playing.
                            if (!isWhite) sq ^= 56;

                            // We count the phase of the current position.
                            // The phase represents how much we are into the end-game in a gradual way. 24 = all pieces on the board, 0 = only pawns/kings left.
                            // This is a core principle of tapered evaluation. We increment phase for each piece for both sides based on it's importance:
                            // None: 0 (obviously)
                            // Pawn: 0
                            // Knight: 1
                            // Bishop: 1
                            // Rook: 2
                            // Queen: 4
                            // King: 0 (because checkmate and stalemate has it's own special rules late on)
                            // These values are encoded in the decimals mentioned before and aren't explicit in the engine's code.
                            phase += evalValues[pieceIndex];

                            // Material and PSTs
                            // PST mean "piece-square tables", it is naturally better for a piece to be on a specific square.
                            // More: https://www.chessprogramming.org/Piece-Square_Tables
                            // In this engine, in order to save tokens, the concept of "material" has been removed.
                            // Instead, each square for each piece has a higher value adjusted to the type of piece that occupies it.
                            // In order to fit in 1 byte per row/column, the value of each row/column has been divided by 8,
                            // and here multiplied by 8 (<< 3 is equivalent but ends up 1 token smaller).
                            // Additionally, each column/row, or file/rank is evaluated, as opposed to every square individually,
                            // which is only ~20 ELO weaker compared to full PSTs and saves a lot of tokens.
                            score += evalValues[pieceIndex * 8 + sq / 8]
                                   + evalValues[56 + pieceIndex * 8 + sq % 8]
                                   << 3;
                        }
                    }
                }
                return score;
            }

            int searchDepth = 1;

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
                        board.IsInCheckmate() ? 1_999_999_999 :
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
                {
                    Search(++searchDepth, -2_000_000_000, 2_000_000_000);
                }
            }
            catch { }

            return bestRootMove;
        }
    }
}
