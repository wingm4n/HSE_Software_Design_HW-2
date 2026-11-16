using Microsoft.Extensions.DependencyInjection;
using Pos.Application.Facade;
using Pos.Application.Services;
using Pos.Domain.Grading;

namespace Pos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPosApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        // The domain has no DI package, so its stateless service and grading rules are registered here.
        services.AddSingleton<FinalGradeCalculator>();
        services.AddSingleton<IFinalGradeRule, AllCriteriaRule>();
        services.AddSingleton<IFinalGradeRule, GradedOnlyRule>();

        services.AddTransient<StudentService>();
        services.AddTransient<RubricService>();
        services.AddTransient<EvaluationService>();
        services.AddTransient<ResultService>();
        services.AddTransient<IPosFacade, PosFacade>();

        return services;
    }
}
