using ChessChallenge.API;
using System.IO;
using System.Text.Json;

namespace NeuralNetworkEval
{

    public class NeuralNetworkEvaluator2
    {
        static double[] inputs = new double[64];
        static int[] pieceVal = { 0, 100, 300, 350, 500, 900, 1000 };
        static int multiplier;
        static bool color;

        static NeuralNetwork neuralNetwork;

        public static void Load(string filePath)
        {
            neuralNetwork = JsonSerializer.Deserialize<NeuralNetwork>(File.ReadAllText(filePath));
        }

        public static int Evaluate(Board board)
        {
            multiplier = 1;
            color = board.IsWhiteToMove;
            do
            {
                for (int piece = 1; piece < pieceVal.Length; piece++)
                {
                    ulong bitboard = board.GetPieceBitboard((PieceType)piece, color);
                    while (bitboard != 0)
                    {
                        int square = BitboardHelper.ClearAndGetIndexOfLSB(ref bitboard);
                        inputs[square] = pieceVal[piece] * multiplier;
                    }
                }
                multiplier = -multiplier;
                color = !color;
            } while (multiplier != 1);
            return (int)(neuralNetwork.CalculateOutputs(inputs)[0] - 30_000d);
        }
    }
}
