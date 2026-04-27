using FastPay.Api.Records;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    service = "FastPay",
    status = "available"
}));

app.MapPost("/payments", (FastPayPaymentRequest request) =>
{
    if (request.TransactionAmount <= 0 || string.IsNullOrWhiteSpace(request.Currency))
    {
        return Results.BadRequest(new
        {
            status = "rejected",
            status_detail = "Payload invalido"
        });
    }

    var response = new FastPayPaymentResponse(
        $"FP-{Random.Shared.Next(100000, 999999)}",
        "approved",
        "Pagamento aprovado");

    return Results.Ok(response);
});

app.Run();
