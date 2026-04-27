using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PaymentOrchestrator.Application.Payments;
using PaymentOrchestrator.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:8080",
                "https://localhost:8080",
                "http://127.0.0.1:8080",
                "https://127.0.0.1:8080")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<ProcessPaymentUseCase>();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseCors("Frontend");

app.MapGet("/", () => Results.Ok(new
{
    service = "PaymentOrchestrator",
    status = "running"
}));

app.MapPost("/payments", async Task<Results<Created<PaymentResponse>, BadRequest<ValidationProblemDetails>, ProblemHttpResult>> (
    CreatePaymentRequest request,
    ProcessPaymentUseCase useCase,
    CancellationToken cancellationToken) =>
{
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(request);

    if (!Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
    {
        var errors = validationResults
            .GroupBy(result => result.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(
                group => group.Key,
                group => group.Select(result => result.ErrorMessage ?? "Invalid value.").ToArray());

        return TypedResults.BadRequest(new ValidationProblemDetails(errors));
    }

    try
    {
        var response = await useCase.ExecuteAsync(request, cancellationToken);
        return TypedResults.Created($"/payments/{response.Id}", response);
    }
    catch (InvalidOperationException exception)
    {
        return TypedResults.Problem(
            title: "Payment provider unavailable",
            detail: exception.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.Run();
