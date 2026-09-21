using Blackjack.Core.Models;

namespace Blackjack.CLI.Helpers;

public static class ConsoleRenderer
{
    public static void ShowTable(GameModel game, PlayerModel player, bool revealDealer)
    {
        Console.Clear();
        Console.WriteLine("=== Blackjack ===\n");

        var d = game.Dealer;
        if (revealDealer)
            Console.WriteLine($"Dealer: {Cards(FullDealerHand(d))}  (value {d.HandValue})"
                              + Flag(d.BustedHand));
        else
            Console.WriteLine($"Dealer: {d.Hand.FirstOrDefault()?.Name ?? "—"}, [hidden]  "
                              + $"(showing {d.HandValue})");

        Console.WriteLine();
        Console.WriteLine($"{player.Name} — {player.Points} pts");
        Console.WriteLine($"  Hand:  {Cards(player.Hand)}  (value {player.HandValue})"
                          + Flag(player.BustedHand));
        if (player.HasSplit)
            Console.WriteLine($"  Split: {Cards(player.SplitHand)}  (value {player.SplitHandValue})"
                              + Flag(player.BustedSplit));
        Console.WriteLine($"  Bet:   {LiveBet(player)}");
        Console.WriteLine();
    }

    // Hole is counted via Append, not stored in Hand — stitch it in for display.
    private static IEnumerable<CardModel> FullDealerHand(DealerModel d) =>
        d.HoleCard is null ? d.Hand : d.Hand.Append(d.HoleCard);

    private static string Cards(IEnumerable<CardModel> cards) =>
        string.Join(", ", cards.Select(c => c.Name));

    private static string Flag(bool busted) => busted ? "  ← BUST" : "";

    private static int LiveBet(PlayerModel p) =>
        (p.BustedHand ? 0 : p.Bet)
        + (p.HasSplit && !p.BustedSplit ? p.SplitBet : 0);
}
