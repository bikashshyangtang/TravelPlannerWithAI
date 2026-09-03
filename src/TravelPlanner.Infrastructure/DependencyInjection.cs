using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TravelPlanner.Application.Abstractions;
using TravelPlanner.Infrastructure.Persistence;
using TravelPlanner.Infrastructure.ExternalApis;
using TravelPlanner.Infrastructure.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using TravelPlanner.Infrastructure.AI.Plugins;

namespace TravelPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TravelPlannerDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("Default")));
        services.AddScoped<ITripRepository, TripRepository>();
        //services.AddScoped<IItineraryGenerator, FakeItineraryGenerator>();
        
        var groqModel = configuration["Groq:Model"] ?? throw new InvalidOperationException("Groq model is not configured.");
        var groqApiKey = configuration["Groq:ApiKey"] ?? throw new InvalidOperationException("Groq API key is not configured.");
        services.AddScoped(sp =>
        {
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.AddOpenAIChatCompletion(
                modelId: groqModel,
                apiKey: groqApiKey,
                endpoint: new Uri("https://api.groq.com/openai/v1")
            );
            var kernel = kernelBuilder.Build();
            //kernel.Plugins.AddFromObject(sp.GetRequiredService<PlacesPlugin>());
            return kernel;
        });
        services.AddScoped<IItineraryGenerator, SemanticKernelItineraryGenerator>();
        services.AddScoped<IItineraryRefiner, SemanticKernelItineraryRefiner>();
        services.AddHttpClient<IPlaceService, FourSquarePlaceService>();
        services.AddScoped<PlacesPlugin>();
        return services;
    }
}