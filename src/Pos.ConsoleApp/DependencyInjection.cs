using Microsoft.Extensions.DependencyInjection;
using Pos.ConsoleApp.Io;
using Pos.ConsoleApp.Menu;
using Pos.ConsoleApp.Screens;

namespace Pos.ConsoleApp;

public static class DependencyInjection
{
    // Expects TextReader and TextWriter to be registered by the caller: the real console or test streams.
    public static IServiceCollection AddPosConsole(this IServiceCollection services)
    {
        services.AddSingleton<ConsoleIO>();

        // The main menu shows screens in registration order.
        services.AddTransient<IMenuScreen, StudentsScreen>();
        services.AddTransient<IMenuScreen, CriteriaScreen>();
        services.AddTransient<IMenuScreen, EvaluationsScreen>();
        services.AddTransient<IMenuScreen, ResultsScreen>();

        services.AddTransient<DemoScenario>();
        services.AddTransient<PosConsoleApp>();

        return services;
    }
}
