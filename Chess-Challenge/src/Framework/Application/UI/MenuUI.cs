using Raylib_cs;
using System;
using System.IO;
using System.Numerics;

namespace ChessChallenge.Application
{
    public static class MenuUI
    {
        private const double LN10 = 2.302585092994046;

        public static void DrawButtons(ChallengeController controller)
        {
            Vector2 buttonPos = UIHelper.Scale(new Vector2(260, 100));
            Vector2 buttonSize = UIHelper.Scale(new Vector2(260, 55));
            float spacing = buttonSize.Y * 1.2f;
            float breakSpacing = spacing * 0.6f;

            // Game Buttons
            if (NextButtonInRow("Human vs MyBot", ref buttonPos, spacing, buttonSize))
            {
                var whiteType = controller.HumanWasWhiteLastGame ? ChallengeController.PlayerType.MyBot : ChallengeController.PlayerType.Human;
                var blackType = !controller.HumanWasWhiteLastGame ? ChallengeController.PlayerType.MyBot : ChallengeController.PlayerType.Human;
                controller.StartNewGame(whiteType, blackType);
            }
            if (NextButtonInRow("MyBot vs MyBot", ref buttonPos, spacing, buttonSize))
            {
                controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.MyBot);
            }
            if (NextButtonInRow("MyBot vs EvilBot", ref buttonPos, spacing, buttonSize))
            {
                controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.EvilBot);
            }
            if (NextButtonInRow("MyBot vs Tier2", ref buttonPos, spacing, buttonSize))
            {
                controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.Tier2);
            }
            if (NextButtonInRow("MyBot vs EvilBot2", ref buttonPos, spacing, buttonSize))
            {
                controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.EvilBot2);
            }
            if (NextButtonInRow("Human vs EvilBot", ref buttonPos, spacing, buttonSize))
            {
                var whiteType = controller.HumanWasWhiteLastGame ? ChallengeController.PlayerType.EvilBot : ChallengeController.PlayerType.Human;
                var blackType = !controller.HumanWasWhiteLastGame ? ChallengeController.PlayerType.EvilBot : ChallengeController.PlayerType.Human;
                controller.StartNewGame(whiteType, blackType);
            }

            // Page buttons
            buttonPos.Y += breakSpacing;
            if (NextButtonInRow("Calculate Elo difference", ref buttonPos, spacing, buttonSize))
            {
                Console.WriteLine("EloDifference: " + CalculateEloDifference() + "; ErrorMargin: " + CalculateEloErrorMargin());
            }
            if (NextButtonInRow("Save Games", ref buttonPos, spacing, buttonSize))
            {
                string pgns = controller.AllPGNs;
                string directoryPath = Path.Combine(FileHelper.AppDataPath, "Games");
                Directory.CreateDirectory(directoryPath);
                string fileName = FileHelper.GetUniqueFileName(directoryPath, "games", ".txt");
                string fullPath = Path.Combine(directoryPath, fileName);
                File.WriteAllText(fullPath, pgns);
                ConsoleHelper.Log("Saved games to " + fullPath, false, ConsoleColor.Blue);
            }
            if (NextButtonInRow("Rules & Help", ref buttonPos, spacing, buttonSize))
            {
                FileHelper.OpenUrl("https://github.com/SebLague/Chess-Challenge");
            }
            if (NextButtonInRow("Documentation", ref buttonPos, spacing, buttonSize))
            {
                FileHelper.OpenUrl("https://seblague.github.io/chess-coding-challenge/documentation/");
            }
            if (NextButtonInRow("Submission Page", ref buttonPos, spacing, buttonSize))
            {
                FileHelper.OpenUrl("https://forms.gle/6jjj8jxNQ5Ln53ie6");
            }

            // Window and quit buttons
            buttonPos.Y += breakSpacing;

            bool isBigWindow = Raylib.GetScreenWidth() > Settings.ScreenSizeSmall.X;
            string windowButtonName = isBigWindow ? "Smaller Window" : "Bigger Window";
            if (NextButtonInRow(windowButtonName, ref buttonPos, spacing, buttonSize))
            {
                Program.SetWindowSize(isBigWindow ? Settings.ScreenSizeSmall : Settings.ScreenSizeBig);
            }
            if (NextButtonInRow("Exit (ESC)", ref buttonPos, spacing, buttonSize))
            {
                Environment.Exit(0);
            }

            bool NextButtonInRow(string name, ref Vector2 pos, float spacingY, Vector2 size)
            {
                bool pressed = UIHelper.Button(name, pos, size);
                pos.Y += spacingY;
                return pressed;
            }
        }

        public static int CalculateEloDifference()
        {
            double wins = ChallengeController.BotStatsA.NumWins;
            double draws = ChallengeController.BotStatsA.NumDraws;
            double losses = ChallengeController.BotStatsA.NumLosses;
            double score = wins + draws / 2d;
            double total = wins + draws + losses;
            double percentage = (score / total);
            return (int)Math.Round(-400 * Math.Log(1d / percentage - 1) / LN10);
        }

        private static double CalculateEloErrorMargin()
        {
            double wins = ChallengeController.BotStatsA.NumWins;
            double draws = ChallengeController.BotStatsA.NumDraws;
            double losses = ChallengeController.BotStatsA.NumLosses;

            double total = wins + draws + losses;
            double percentage = (wins + draws * 0.5) / total;

            double winP = wins / total;
            double drawP = draws / total;
            double lossP = losses / total;

            double winsDev = winP * Math.Pow(1d - percentage, 2);
            double drawsDev = drawP * Math.Pow(0.5d - percentage, 2);
            double lossesDev = lossP * Math.Pow(0d - percentage, 2);

            double stdDeviation = Math.Sqrt(winsDev + drawsDev + lossesDev) / Math.Sqrt(total);

            double confidenceP = 0.95d;
            double minConfidenceP = (1d - confidenceP) / 2d;
            double maxConfidenceP = 1d - minConfidenceP;

            double devMin = percentage + PhiInv(minConfidenceP) * stdDeviation;
            double devMax = percentage + PhiInv(maxConfidenceP) * stdDeviation;

            double difference = CalculateEloDifference(devMax) - CalculateEloDifference(devMin);

            return Math.Round(difference / 2d);
        }

        private static double CalculateEloDifference(double percentage)
        {
            return -400d * Math.Log(1d / percentage - 1d) / LN10;
        }

        private static double PhiInv(double p)
        {
            double res = Math.Sqrt(2d) * CalculateInverseErrorFunction(2d * p - 1d);
            return res;
        }

        private static double CalculateInverseErrorFunction(double x)
        {
            double pi = Math.PI;
            double a = 8d * (pi - 3d) / (3d * pi * (4d - pi));
            double y = Math.Log(1d - Math.Pow(x, 2d));
            double z = 2d / (pi * a) + y / 2d;

            double res = Math.Sqrt(z * z - y / a) - z;
            double ret = Math.Sqrt(res);

            if (x < 0)
                return -ret;

            return ret;
        }
    }
}