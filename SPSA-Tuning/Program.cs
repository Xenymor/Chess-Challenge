using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SPSA_Tuning
{

    class SPSAAlgorithm
    {   // Set your initial parameter values
        static double[] parameters = { 310, 330, 500, 1000, 15, 30, 23, 60, 13, 10, 141, 74, 1, 1, 13, 60, 25, 3 };
        static List<string> paramNames = new List<string>(new string[] { "NV", "BV", "RV", "QV", "DPM", "DPE", "BPM", "BPE", "OFM", "OFE", "FP", "RFP", "SEC", "SEP", "HTL", "AWW", "AWS", "STL" });

        // Set your cutechess-cli command
        static int gameCount = 2;
        static string options = $"-games {gameCount} -repeat 2 -each tc=inf/60 proto=uci -rounds 2 -concurrency 4 -openings file=C:/Users/timon/Documents/Programmieren/C#/Chess-Challenge/SPSA-Tuning/PGN/lichess_db_standard_rated_2013-01.pgn format=pgn order=random plies=5 policy=encounter";
        static string cutechessFile = "C:/Users/timon/Documents/Programmieren/C#/Chess-Challenge/chessScript.02/cutechess-cli.exe";
        static string engine = "-engine cmd=C:/Users/timon/Documents/Programmieren/C#/Chess-Challenge/MyConsoleBot/bin/Release/net6.0/MyConsoleBot.exe name=\"MyBot\"";

        // Set SPSA hyperparameters
        static int iterations = 1000;
        static double stepSize = 0.1;
        static double perturbation = 50;

        static void Main()
        {


            // Run SPSA
            for (int i = 0; i < iterations; i++)
            {
                // Generate random perturbation vector
                double[] delta = GenerateRandomVector(parameters.Length, perturbation);

                // Calculate cost function at perturbed and anti-perturbed points
                double yPlus = GetCost(AddVectors(parameters, delta), parameters);
                double yMinus = GetCost(SubtractVectors(parameters, delta), parameters);

                // Estimate gradient
                double[] gradient = DivideVectorScalar(SubtractVectors(delta, NegateVector(delta)), 2.0 * perturbation);

                // Update parameters
                parameters = SubtractVectors(parameters, MultiplyVectorScalar(gradient, stepSize * (yPlus - yMinus)));

                // Get improvement - Replace this with your own method
                double improvement = GetImprovement(parameters);

                // Print current iteration and improvement
                Console.WriteLine($"Iteration {i + 1}, Improvement: {improvement}");
            }
        }

        static double GetCost(double[] parameters, double[] originalParams)
        {
            // Setup process
            Process process = new Process();
            process.StartInfo.FileName = cutechessFile;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            string initString1 = "";
            string initString2 = "";

            // Set parameters
            for (int i = 0; i < parameters.Length; i++)
            {
                initString1 += $" initstr=\"setvalue {paramNames[i]} {(int)parameters[i]}\"";
                initString2 += $" initstr=\"setvalue {paramNames[i]} {(int)originalParams[i]}\"";
            }

            // Run the cutechess-cli command
            string command = $"{options} {engine}{initString1} {engine}{initString2}";
            process.StartInfo.Arguments = command;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Console.Error.WriteLine($"failed to execute command: {command}");
                Console.Error.WriteLine(process.StandardError.ReadToEnd());
                return double.PositiveInfinity;
            }

            // Convert Cutechess-cli's result into Cost
            int result = 0;
            int counter = 0;
            foreach (string line in output.Split('\n'))
            {
                Console.WriteLine(line);
                if (line.StartsWith("Finished game"))
                {
                    counter++;
                    if (line.Contains(": 1-0"))
                    {
                        result += 1;
                    }
                    else if (line.Contains(": 0-1"))
                    {
                        result += -1;
                    }
                    else if (line.Contains(": 1/2-1/2"))
                    {
                        result += 0;
                    }
                    else
                    {
                        Console.Error.WriteLine("the game did not terminate properly");
                        return double.PositiveInfinity;
                    }
                    if (counter >= gameCount)
                    {
                        break;
                    }
                }
            }

            // Use the 'result' variable as needed
            int v = 1 / (result + gameCount);
            Console.WriteLine(result + "; " + v);
            return v * v;
        }

        // Placeholder for your specific improvement calculation
        static double GetImprovement(double[] parameters)
        {
            // Implement your logic for improvement calculation here
            // This method needs to be replaced with your actual logic
            return 0.0;
        }

        // Helper methods for vector operations
        static double[] AddVectors(double[] a, double[] b)
        {
            return a.Zip(b, (x, y) => x + y).ToArray();
        }

        static double[] SubtractVectors(double[] a, double[] b)
        {
            return a.Zip(b, (x, y) => x - y).ToArray();
        }

        static double[] MultiplyVectorScalar(double[] a, double scalar)
        {
            return a.Select(x => x * scalar).ToArray();
        }

        static double[] DivideVectorScalar(double[] a, double scalar)
        {
            return a.Select(x => x / scalar).ToArray();
        }

        static double[] NegateVector(double[] a)
        {
            return a.Select(x => -x).ToArray();
        }

        static Random rand = new Random();

        static double[] GenerateRandomVector(int length, double scale)
        {
            return Enumerable.Range(0, length).Select(_ => (rand.NextDouble() - 0.5) * 2.0 * scale).ToArray();
        }
    }

}