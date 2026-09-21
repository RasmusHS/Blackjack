using Blackjack.Core;
using Blackjack.Core.Models;

namespace Blackjack.CLI.Helpers;

public static class Prompt
{
    public static string ForName()
    {
        string? name;
        do { Console.Write("Enter your name: "); name = Console.ReadLine(); }
        while (string.IsNullOrWhiteSpace(name));
        return name.Trim();
    }

    public static char Move(PlayerModel p)
    {
        var options = new List<(char key, string label)> { ('H', "[H]it"), ('S', "[S]tand") };
        if (p.CanDoubleDown) options.Add(('D', "[D]ouble"));
        if (p.SplitPossible) options.Add(('P', "[P]Split"));

        Console.WriteLine("Moves:  " + string.Join("    ", options.Select(o => o.label)));
        Console.Write("> ");
        return ReadFrom(options.Select(o => o.key).ToArray());
    }

    public static char PostRound(int points)
    {
        if (points >= BaseValues.Bet)
        {
            Console.WriteLine("\n[C]ontinue    [N]ew game    [Q]uit");
            return ReadFrom('C', 'N', 'Q');
        }
        Console.WriteLine("\nOut of points to bet.   [N]ew game    [Q]uit");
        return ReadFrom('N', 'Q');
    }

    private static char ReadFrom(params char[] allowed)
    {
        while (true)
        {
            var key = char.ToUpperInvariant(Console.ReadKey(intercept: true).KeyChar);
            if (allowed.Contains(key)) { Console.WriteLine(key); return key; }
        }
    }
}
