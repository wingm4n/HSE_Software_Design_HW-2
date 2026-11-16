using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.Application.Facade;
using Pos.ConsoleApp;
using Pos.Infrastructure;

namespace Pos.Tests.TestSupport;

/// <summary>The same registrations as in Program.cs, but with in-memory console streams.</summary>
public static class TestHost
{
    public static ServiceProvider Create(string input = "", TextWriter? output = null) =>
        new ServiceCollection()
            .AddSingleton<TextReader>(new StringReader(input))
            .AddSingleton(output ?? new StringWriter())
            .AddPosApplication()
            .AddPosInfrastructure()
            .AddPosConsole()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

    public static IPosFacade CreateFacade() => Create().GetRequiredService<IPosFacade>();
}
