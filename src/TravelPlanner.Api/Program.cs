using TravelPlanner.Application.Abstractions;
using TravelPlanner.Infrastructure;
using TravelPlanner.Domain.Entities;
using TravelPlanner.Domain.Enums;
using TravelPlanner.Application.Trips.PlanTrip;
using TravelPlanner.Application.Trips.RefineTrip;
using TravelPlanner.Application.Trips;
using Scalar.AspNetCore;
using FluentValidation;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Api.Middleware;
using static TravelPlanner.Application.Trips.RefineTrip.RefineTripHandler;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<PlanTripHandler>();
builder.Services.AddScoped<RefineTripHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<TripValidator>();
builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())    // ← new block
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.MapPost("/api/trips/plan", async (
    PlanTripRequest request,
    [FromServices]PlanTripHandler handler,
    [FromServices]IValidator<TripCriteria> validator,
    CancellationToken cancellationToken) =>
{
    var criteria = new TripCriteria(
        request.Destination,
        request.StartDate,
        request.EndDate,
        request.Budget,
        request.BudgetLevel,
        request.Interests);

    var validation = await validator.ValidateAsync(criteria, cancellationToken);
    if (!validation.IsValid)
    {
        return Results.ValidationProblem(validation.ToDictionary());
    }
    var trip = await handler.HandleTripAsync(criteria, cancellationToken);

    return Results.Ok(trip.ToTripResponse());

})
.WithName("PlanTrip")
.WithSummary("Plan a new trip")
.Produces<TripResponse>(StatusCodes.Status200OK)
.ProducesValidationProblem();

app.MapGet("api/trips", async([FromServices] ITripRepository tripRepository, CancellationToken cancellationToken) =>
{
    var trips = await tripRepository.GetAllAsync(cancellationToken);
    return Results.Ok(trips.Select(t => t.ToSummaryResponse()));
})
.WithName("GetAllTrips")
.WithSummary("Get all trips")
.Produces<List<TripResponseSummary>>(StatusCodes.Status200OK);

app.MapGet("api/trips/{tripId:int}", async(int tripId, [FromServices] ITripRepository tripRepository, CancellationToken cancellationToken) =>
{
    var trip = await tripRepository.GetByIdAsync(tripId, cancellationToken);
    return trip is null ? Results.NotFound(new { message = $"Trip {tripId} not found" }) : Results.Ok(trip.ToTripResponse());
})
.WithName("GetTripById")
.WithSummary("Get a trip by its ID")
.Produces<TripResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapPost("api/trips/{tripid:int}/refine", async(int tripId, RefineRequest request, [FromServices] RefineTripHandler handler, CancellationToken cancellationToken) =>
{
    if(string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(new { message = "Message is required" });
    }
    var response = await handler.HandleAsync(tripId, request.Message, cancellationToken);
    return Results.Ok(response);
})
.WithName("RefineTrip")
.WithSummary("Refine an existing Trip")
.Produces<RefineTripResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.ProducesValidationProblem();

app.Run();

public record PlanTripRequest(
    string Destination,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Budget,
    BudgetLevel BudgetLevel,
    List<string> Interests);

public record RefineRequest(string Message);