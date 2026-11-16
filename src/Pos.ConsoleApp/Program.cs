using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.ConsoleApp;
using Pos.Infrastructure;

Console.OutputEncoding = Encoding.UTF8;

// Composition root: the only place that knows every layer and chooses concrete adapters.
var services = new ServiceCollection();
services.AddSingleton(Console.In);
services.AddSingleton(Console.Out);
services
    .AddPosApplication()
    .AddPosInfrastructure()
    .AddPosConsole();

// A missing registration fails here, at start-up, not in the middle of a scenario.
using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

PosConsoleApp app = provider.GetRequiredService<PosConsoleApp>();
if (args.Contains("--demo", StringComparer.OrdinalIgnoreCase))
{
    app.RunDemo();
}
else
{
    app.Run();
}
