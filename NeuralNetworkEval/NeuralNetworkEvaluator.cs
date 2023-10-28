using ChessChallenge.API;

namespace NeuralNetworkEval
{
    public class NeuralNetworkEval
    {
        static EvalModel.ModelInput inputs = new EvalModel.ModelInput();
        static int[] pieceVal = { 0, 100, 300, 350, 500, 900, 1000 };
        static int multiplier;
        static bool color;

        public static int Evaluate(Board board)
        {
            int[] localInputs = new int[64];
            do
            {
                for (int piece = 1; piece < pieceVal.Length; piece++)
                {
                    ulong bitboard = board.GetPieceBitboard((PieceType)piece, color);
                    while (bitboard != 0)
                    {
                        int square = BitboardHelper.ClearAndGetIndexOfLSB(ref bitboard);
                        localInputs[square] = pieceVal[piece] * multiplier;
                    }
                }
                multiplier = -multiplier;
                color = !color;
            } while (multiplier != 1);
            setColumns(localInputs, inputs);
            return (int) EvalModel.Predict(inputs).Col0;
        }

        private static void setColumns(int[] localInputs, EvalModel.ModelInput inputs)
        {
            for (int i = 0; i < 64; i++)
            {
                string colName = "Col" + i;
                typeof(EvalModel.ModelInput).GetField(colName).SetValue(inputs, localInputs[i]);
            }
        }
    }
}
