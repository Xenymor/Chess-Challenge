using NeuralNetworkEval;

public class MyBotNeuralNetwork : MyBot
{
    override
    public int Evaluate()
    {
        return NeuralNetworkEvaluator.Evaluate(board);
    }
}
