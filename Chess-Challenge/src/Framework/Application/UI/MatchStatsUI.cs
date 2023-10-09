using Raylib_cs;
using System.Numerics;

namespace ChessChallenge.Application
{
    public static class MatchStatsUI
    {
        public static long depthSum1 = 0;
        public static int movesPlayed1 = 0;
        public static long depthSum2 = 0;
        public static int movesPlayed2 = 0;

        public static void DrawMatchStats(ChallengeController controller)
        {
            if (controller.PlayerWhite.IsBot && controller.PlayerBlack.IsBot)
            {
                int nameFontSize = UIHelper.ScaleInt(40);
                int regularFontSize = UIHelper.ScaleInt(35);
                int headerFontSize = UIHelper.ScaleInt(45);
                Color col = new(180, 180, 180, 255);
                Vector2 startPos = UIHelper.Scale(new Vector2(1500, 250));
                float spacingY = UIHelper.Scale(35);

                DrawNextText($"Game {controller.CurrGameNumber} of {controller.TotalGameCount}", headerFontSize, Color.WHITE);
                startPos.Y += spacingY * 2;
                DrawNextText("Elo Difference: " + MenuUI.CalculateEloDifference(), regularFontSize, Color.WHITE);
                DrawNextText("Error Margin: " + MenuUI.CalculateEloErrorMargin(), regularFontSize, Color.WHITE);
                startPos.Y += spacingY * 2;

                DrawStats(ChallengeController.BotStatsA);
                DrawNextText("Average depth: " + (movesPlayed1 == 0 ? 0 : ((float)depthSum1 / movesPlayed1)), regularFontSize, col);
                startPos.Y += spacingY * 2;
                DrawStats(ChallengeController.BotStatsB);
                DrawNextText("Average depth: " + (movesPlayed2 == 0 ? 0 : ((float)depthSum2 / movesPlayed2)), regularFontSize, col);


                void DrawStats(ChallengeController.BotMatchStats stats)
                {
                    DrawNextText(stats.BotName + ":", nameFontSize, Color.WHITE);
                    DrawNextText($"Score: +{stats.NumWins} ={stats.NumDraws} -{stats.NumLosses}", regularFontSize, col);
                    DrawNextText($"Num Timeouts: {stats.NumTimeouts}", regularFontSize, col);
                    DrawNextText($"Num Illegal Moves: {stats.NumIllegalMoves}", regularFontSize, col);
                }

                void DrawNextText(string text, int fontSize, Color col)
                {
                    UIHelper.DrawText(text, startPos, fontSize, 1, col);
                    startPos.Y += spacingY;
                }
            }
        }
    }
}