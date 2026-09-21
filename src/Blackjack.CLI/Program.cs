using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

var rootCommand = new RootCommand();

//rootCommand.Subcommands.Add();

return rootCommand.Parse(args).Invoke();
