using Microsoft.Extensions.DependencyInjection;
using Pos.Application.Abstractions;
using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Pos.Infrastructure.Export;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPosInfrastructure(this IServiceCollection services)
    {
        // Singletons: the in-memory repositories hold the data of the whole session.
        services.AddSingleton<IStudentRepository, InMemoryStudentRepository>();
        services.AddSingleton<IRubricRepository, InMemoryRubricRepository>();
        services.AddSingleton<IEvaluationRepository, InMemoryEvaluationRepository>();

        services.AddSingleton<IResultExporter, CsvResultExporter>();
        services.AddSingleton<IResultExporter, JsonResultExporter>();

        return services;
    }
}
