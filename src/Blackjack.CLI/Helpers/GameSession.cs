using Blackjack.Core;
using Blackjack.Core.Models;

namespace Blackjack.CLI.Helpers;

public static class GameSession
{
    public static void Run(string playerName)
    {
        int points = BaseValues.InitialPlayerBalance;

        while (true)
        {
            var player = new PlayerModel(Guid.NewGuid(), playerName, points);
            var game = new GameModel(new DealerModel(), new List<PlayerModel> { player });
            game.StartGame();

            PlayRound(game, player);
            points = player.Points;

            var choice = Prompt.PostRound(points);
            if (choice == 'Q') break;
            if (choice == 'N') points = BaseValues.InitialPlayerBalance; // 'C' keeps points
        }

        Console.WriteLine("\nThanks for playing.");
    }

    private static void PlayRound(GameModel game, PlayerModel player)
    {
        while (!player.EndedTurn)
        {
            ConsoleRenderer.ShowTable(game, player, revealDealer: false);
            switch (Prompt.Move(player))
            {
                case 'H': game.HitPlayer(player); break;
                case 'S': game.StandPlayer(player); break;
                case 'D': game.DoubleDownPlayer(player); break;   // no-op if !CanDoubleDown
                case 'P': game.SplitPlayer(player); break;        // no-op if !SplitPossible
            }
        }

        // Capture stake before Result zeroes Bet/SplitBet, so net is computable.
        int staked = player.Bet + (player.HasSplit ? player.SplitBet : 0);
        int before = player.Points;

        game.HitDealer();
        game.Result(game.Dealer, game.Players);

        int net = (player.Points - before) - staked;
        ConsoleRenderer.ShowTable(game, player, revealDealer: true);
        Console.WriteLine(net > 0 ? $"You win!  +{net} pts"
                        : net < 0 ? $"You lose.  {net} pts"
                        : "Even.");
    }
}
