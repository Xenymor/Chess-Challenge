using Chess_Challenge;
using ChessChallenge.API;
using System.Reflection;

namespace NeuralNetworkEval
{
    public class NeuralNetworkEvaluator
    {
        static EvalModel.ModelInput inputs = new EvalModel.ModelInput();
        static int[] pieceVal = { 0, 100, 300, 350, 500, 900, 1000 };
        static int multiplier;
        static bool color;
        static int[] localInputs = new int[64];

        public static int Evaluate(Board board)
        {
            localInputs = new int[64];
            ulong bitboard;
            int square;
            multiplier = 1;
            do
            {
                for (int piece = 1; piece < pieceVal.Length; piece++)
                {
                    bitboard = board.GetPieceBitboard((PieceType)piece, color);
                    while (bitboard != 0)
                    {
                        square = BitboardHelper.ClearAndGetIndexOfLSB(ref bitboard);
                        localInputs[square] = pieceVal[piece] * multiplier;
                    }
                }
                multiplier = -multiplier;
                color = !color;
            } while (multiplier != 1);
            setColumns(localInputs, inputs);
            EvalModel.ModelOutput result = EvalModel.Predict(inputs);
            return (int)result.Score;
        }

        private static void setColumns(int[] localInputs, EvalModel.ModelInput inputs)
        {
            string colName;
            System.Collections.Generic.IEnumerable<FieldInfo> fieldInfos = typeof(EvalModel.ModelInput).GetRuntimeFields();
            int index = 0;
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                if (index != 0)
                    fieldInfo.SetValue(inputs, localInputs[index - 1]);
                index++;
            }
        }
    }
}
