using ChessChallenge.Application;

namespace Chess_Challenge.src.NeuralNetworkEval2
{
    public class NeuralNetworkTrainer
    {
        private const string SAVE_PATH = "C:\\Users\\timon\\Documents\\Programmieren\\C#\\Chess-Challenge\\Chess-Challenge\\src\\NeuralNetworkEval2\\NeuralNetwork.json";


        public static void Main()
        {
            Program.Main();

            /*
            HyperParameters hyperParameters = new HyperParameters();
            // Set learning params
            hyperParameters.regularization = 0.1;
            hyperParameters.momentum = 0.9;
            hyperParameters.initialLearningRate = 0.5;
            hyperParameters.learnRateDecay = 0.075;
            hyperParameters.minibatchSize = 32;

            // Set network params
            hyperParameters.layerSizes = new int[] { 64, 1048, 500, 50, 1 };
            hyperParameters.activationType = Activation.ActivationType.ReLU;
            hyperParameters.costType = Cost.CostType.MeanSquareError;
            hyperParameters.outputActivationType = Activation.ActivationType.ReLU;

            //Load NeuralNetwork
            NetworkTrainer networkTrainer = new NetworkTrainer();
            if (File.Exists(SAVE_PATH))
            {
                networkTrainer.neuralNetwork = JsonConvert.DeserializeObject<NetworkSaveData>(File.ReadAllText(SAVE_PATH)).LoadNetwork();
                Console.WriteLine("Loaded");
            }

            //Load Data
            DataPoint[] dataPoints = LoadData("C:\\Users\\timon\\Documents\\Programmieren\\C#\\Chess-Challenge\\Chess-Challenge\\bin\\Debug\\net6.0\\datasetNeuralNetwork2.csv");
            TrainingSessionInfo trainingSessionInfo = new TrainingSessionInfo(900);

            networkTrainer.Start(dataPoints, hyperParameters, SAVE_PATH, 0.8f, trainingSessionInfo);

            int counter = 0;
            int lastEpochsCompleted = -1;
            while (counter < 1_000_000)
            {
                if ((int)trainingSessionInfo.epochsCompleted > lastEpochsCompleted)
                {
                    lastEpochsCompleted = (int)trainingSessionInfo.epochsCompleted;
                    string saveDataString = JsonConvert.SerializeObject(NetworkSaveData.getSaveData(networkTrainer.neuralNetwork), Formatting.Indented);
                    File.WriteAllText(SAVE_PATH, saveDataString);
                    Console.WriteLine("Epochs completed: " + lastEpochsCompleted);
                    Console.WriteLine("Batches completed: " + trainingSessionInfo.batchesCompleted);
                    Console.WriteLine("Cost: " + getCost(networkTrainer.neuralNetwork, dataPoints));
                }
                networkTrainer.Learn();
                counter++;
            }
            */
        }
        /*
        private static double getCost(NeuralNetwork neuralNetwork, DataPoint[] dataPoints)
        {
            double sum = 0;
            int count = 0;
            for (int i = 0; i < dataPoints.Length * 0.01; i++)
            {
                sum += getCost(neuralNetwork, dataPoints[i]);
                count++;
            }
            return sum / count;
        }

        private static double getCost(NeuralNetwork neuralNetwork, DataPoint dataPoint)
        {
            return neuralNetwork.cost.CostFunction(neuralNetwork.CalculateOutputs(dataPoint.inputs), dataPoint.expectedOutputs);
        }

        private static DataPoint[] LoadData(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }
            string[] dataStrings = File.ReadAllLines(path);
            DataPoint[] dataPoints = new DataPoint[dataStrings.Length];

            for (int i = 0; i < dataStrings.Length; i++)
            {
                dataPoints[i] = CreateDataPoint(dataStrings[i]);
            }
            return dataPoints;
        }

        private static DataPoint CreateDataPoint(string line)
        {
            string[] inputStrings = line.Split(',');
            double[] outputs = new double[] { int.Parse(inputStrings[0]) };
            double[] inputs = new double[inputStrings.Length - 1];

            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = int.Parse(inputStrings[i + 1]);
            }

            return new DataPoint(inputs, outputs);
        }
        */
    }
}
