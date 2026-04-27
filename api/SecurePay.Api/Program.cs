using SecurePay.Api.Records;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    service = "SecurePay",
    status = "available"
}));

app.MapPost("/payments", (SecurePayPaymentRequest request) =>
{
    if (request.AmountCents <= 0 || string.IsNullOrWhiteSpace(request.CurrencyCode))
    {
        return Results.BadRequest(new
        {
            result = "failed"
        });
    }

    var response = new SecurePayPaymentResponse(
        $"SP-{Random.Shared.Next(10000, 99999)}",
        "success");

    return Results.Ok(response);
});

app.Run();