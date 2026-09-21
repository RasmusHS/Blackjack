using System.CommandLine;

//var services = new ServiceCollection();

var nameOption = new Option<string?>("--name", "-n") { Description = "Player name" };

var root = new RootCommand("Blackjack — 1 player vs dealer");
root.Options.Add(nameOption);

root.SetAction(parseResult =>
{
    var name = parseResult.GetValue(nameOption);
    if (string.IsNullOrWhiteSpace(name))
        name = Prompt.ForName();

    GameSession.Run(name);
    return 0;
});

//rootCommand.Subcommands.Add();

return root.Parse(args).Invoke();
