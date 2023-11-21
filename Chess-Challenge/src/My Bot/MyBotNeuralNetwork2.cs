using ChessChallenge.API;
using NeuralNetworkEval;

public class MyBotNeuralNetwork2 : MyBot
{
    override
    public int Evaluate(Board board)
    {
        return NeuralNetworkEvaluator2.Evaluate(board);
    }
}
