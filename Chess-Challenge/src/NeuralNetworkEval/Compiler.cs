﻿// See https://aka.ms/new-console-template for more information

/*static void Main()
{
    Compile();
}

static void Compile()
{
    PositionEvaluation[][] allPositions = new PositionEvaluation[3][];
    allPositions[0] = JsonSerializer.Deserialize<PositionEvaluation[]>(File.ReadAllText("C:\\Users\\timon\\Documents\\Programmieren\\C#\\Chess-Challenge\\Tuner\\bin\\Debug\\net6.0\\positionTable1.json"));
    allPositions[1] = JsonSerializer.Deserialize<PositionEvaluation[]>(File.ReadAllText("C:\\Users\\timon\\Documents\\Programmieren\\C#\\Chess-Challenge\\Tuner\\bin\\Debug\\net6.0\\positionTable2.json"));
    allPositions[2] = JsonSerializer.Deserialize<PositionEvaluation[]>(File.ReadAllText("C:\\Users\\timon\\Documents\\Programmieren\\C#\\Chess-Challenge\\Tuner\\bin\\Debug\\net6.0\\positionTable3.json"));
    ChessChallenge.Chess.Board tempBoard = new ChessChallenge.Chess.Board();
    tempBoard.LoadStartPosition();
    Board board = new Board(tempBoard);
    int[] inputs = new int[64];
    string[] result = new string[allPositions[0].Length + allPositions[1].Length + allPositions[2].Length];
    int[] pieceVal = { 0, 100, 300, 350, 500, 900, 1000 };
    int multiplier;
    string resultString;
    string path = "datasetNeuralNetwork2.csv";
    bool color;
    int index = 0;

    for (int i = 0; i < allPositions.Length; i++)
    {
        PositionEvaluation[] positions = allPositions[i];
        for (int j = 0; j < positions.Length; j++)
        {
            inputs = new int[64];
            board.board.LoadPosition(positions[j].fen);
            resultString = positions[j].eval * (board.IsWhiteToMove ? 1 : -1) + ",";
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
            for (int k = 0; k < inputs.Length; k++)
            {
                resultString += inputs[k] + ",";
            }
            resultString = resultString.Remove(resultString.Length - 1);
            resultString += "\n";
            result[index] = resultString;
            index++;
        }
    }
    Console.WriteLine("Finished Compiling");
    File.WriteAllText(path, "");
    for (int i = 0; i < result.Length; i++)
    {
        File.AppendAllText(path, result[i]);
    }
    Console.WriteLine("Finished Saving");
}*/
public class PositionEvaluation
{
    public string fen { get; set; }
    public int eval { get; set; }
}