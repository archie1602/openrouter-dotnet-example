using AspNetCore;
using AspNetCore.Clients;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.RegisterInfrastructure(
    builder.Configuration,
    builder.Environment);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/solve", async (
        [FromServices] ILlmClient llmClient,
        [FromBody] SolveRequestDto request,
        CancellationToken ct) =>
    {
        if (string.IsNullOrEmpty(request.Problem))
        {
            return Results.BadRequest("Problem field is required.");
        }

        var solution = await llmClient.Solve(request.Problem, ct);

        return Results.Ok(
            string.IsNullOrWhiteSpace(solution)
                ? new SolveResponseDto(false)
                : new SolveResponseDto(true, solution));
    })
    .WithName("SolveMathProblem");

await app.RunAsync();

// DTO models for the POST body
public sealed record SolveRequestDto(string Problem);

public sealed record SolveResponseDto(bool HasSolved, string? Solution = null);