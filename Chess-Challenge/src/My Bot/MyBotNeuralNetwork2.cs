using NeuralNetworkEval;

public class MyBotNeuralNetwork2 : MyBot
{
    override
    public int Evaluate()
    {
        return NeuralNetworkEvaluator2.Evaluate(board);
    }
}
