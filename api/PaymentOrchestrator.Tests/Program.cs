using PaymentOrchestrator.Tests.Integration;

try
{
    await PaymentContractsIntegrationTests.RunAsync();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Integration tests passed.");
    Console.ResetColor();
    return 0;
}
catch (Exception exception)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Integration tests failed.");
    Console.ResetColor();
    Console.WriteLine(exception.Message);
    return 1;
}
