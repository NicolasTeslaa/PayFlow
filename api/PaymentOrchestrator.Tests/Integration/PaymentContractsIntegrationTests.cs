using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PaymentOrchestrator.Tests.Integration;

public static class PaymentContractsIntegrationTests
{
    private const string FastPayUrl = "http://127.0.0.1:5281";
    private const string SecurePayUrl = "http://127.0.0.1:5282";
    private const string OrchestratorUrl = "http://127.0.0.1:5182";

    public static async Task RunAsync()
    {
        var solutionDirectory = FindSolutionDirectory();
        var processes = new List<ManagedProcess>();

        try
        {
            processes.Add(StartService(
                "FastPay",
                solutionDirectory,
                Path.Combine(solutionDirectory, "FastPay.Api", "FastPay.Api.csproj"),
                FastPayUrl));

            processes.Add(StartService(
                "SecurePay",
                solutionDirectory,
                Path.Combine(solutionDirectory, "SecurePay.Api", "SecurePay.Api.csproj"),
                SecurePayUrl));

            processes.Add(StartService(
                "PaymentOrchestrator",
                solutionDirectory,
                Path.Combine(solutionDirectory, "PaymentOrchestrator.Api", "PaymentOrchestrator.Api.csproj"),
                OrchestratorUrl,
                new Dictionary<string, string>
                {
                    ["PaymentProviders__Endpoints__FastPay"] = FastPayUrl,
                    ["PaymentProviders__Endpoints__SecurePay"] = SecurePayUrl,
                    ["ConnectionStrings__Payments"] = $"payflow-tests-{Guid.NewGuid():N}"
                }));

            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };

            await WaitForHealthAsync(httpClient, $"{FastPayUrl}/health", "FastPay");
            await WaitForHealthAsync(httpClient, $"{SecurePayUrl}/health", "SecurePay");
            await WaitForHealthAsync(httpClient, $"{OrchestratorUrl}/", "PaymentOrchestrator");

            await FastPayReceivesExpectedPayloadAndReturnsExpectedShapeAsync(httpClient);
            await SecurePayReceivesExpectedPayloadAndReturnsExpectedShapeAsync(httpClient);
            await OrchestratorReturnsExpectedUserResponseAsync(httpClient);
        }
        finally
        {
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }
    }

    private static async Task FastPayReceivesExpectedPayloadAndReturnsExpectedShapeAsync(HttpClient httpClient)
    {
        const string payload = """
        {
          "transaction_amount": 120.50,
          "currency": "BRL",
          "payer": {
            "email": "cliente@teste.com"
          },
          "installments": 1,
          "description": "Compra via FastPay"
        }
        """;

        using var response = await httpClient.PostAsync(
            $"{FastPayUrl}/payments",
            new StringContent(payload, Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = document.RootElement;

        AssertHasString(root, "id", value => value.StartsWith("FP-", StringComparison.Ordinal));
        AssertString(root, "status", "approved");
        AssertString(root, "status_detail", "Pagamento aprovado");

        Console.WriteLine("FastPay contract: OK");
    }

    private static async Task SecurePayReceivesExpectedPayloadAndReturnsExpectedShapeAsync(HttpClient httpClient)
    {
        const string payload = """
        {
          "amount_cents": 12050,
          "currency_code": "BRL",
          "client_reference": "ORD-20251022"
        }
        """;

        using var response = await httpClient.PostAsync(
            $"{SecurePayUrl}/payments",
            new StringContent(payload, Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = document.RootElement;

        AssertHasString(root, "transaction_id", value => value.StartsWith("SP-", StringComparison.Ordinal));
        AssertString(root, "result", "success");

        Console.WriteLine("SecurePay contract: OK");
    }

    private static async Task OrchestratorReturnsExpectedUserResponseAsync(HttpClient httpClient)
    {
        using var response = await httpClient.PostAsJsonAsync($"{OrchestratorUrl}/payments", new
        {
            amount = 120.50m,
            currency = "BRL"
        });

        response.EnsureSuccessStatusCode();
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = document.RootElement;

        AssertNumber(root, "id");
        AssertHasString(root, "externalId", value => value.StartsWith("SP-", StringComparison.Ordinal));
        AssertString(root, "status", "approved");
        AssertString(root, "provider", "SecurePay");
        AssertDecimal(root, "grossAmount", 120.50m);
        AssertDecimal(root, "fee", 4.01m);
        AssertDecimal(root, "netAmount", 116.49m);

        Console.WriteLine("PaymentOrchestrator response: OK");
    }

    private static ManagedProcess StartService(
        string name,
        string workingDirectory,
        string projectPath,
        string url,
        IReadOnlyDictionary<string, string>? environment = null)
    {
        var dotnet = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet";
        var startInfo = new ProcessStartInfo
        {
            FileName = dotnet,
            Arguments = $"run --no-build --project \"{projectPath}\" --urls {url}",
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.Environment["DOTNET_CLI_HOME"] = Environment.GetEnvironmentVariable("DOTNET_CLI_HOME") ?? Path.Combine(workingDirectory, "..", ".dotnet-home");

        var nugetPackages = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
        if (!string.IsNullOrWhiteSpace(nugetPackages))
        {
            startInfo.Environment["NUGET_PACKAGES"] = nugetPackages;
        }

        if (environment is not null)
        {
            foreach (var item in environment)
            {
                startInfo.Environment[item.Key] = item.Value;
            }
        }

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Could not start {name}.");

        process.OutputDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                Console.WriteLine($"[{name}] {args.Data}");
            }
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                Console.WriteLine($"[{name}:err] {args.Data}");
            }
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return new ManagedProcess(name, process);
    }

    private static async Task WaitForHealthAsync(HttpClient httpClient, string url, string serviceName)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(30);
        Exception? lastException = null;

        while (DateTimeOffset.UtcNow < deadline)
        {
            try
            {
                using var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
            {
                lastException = exception;
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"{serviceName} did not become available at {url}. {lastException?.Message}");
    }

    private static string FindSolutionDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "PaymentOrchestrator.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate PaymentOrchestrator.sln.");
    }

    private static void AssertString(JsonElement element, string propertyName, string expected)
    {
        AssertHasString(element, propertyName, value => value == expected);
    }

    private static void AssertHasString(JsonElement element, string propertyName, Func<string, bool> predicate)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String ||
            !predicate(property.GetString() ?? string.Empty))
        {
            throw new InvalidOperationException($"Expected string property '{propertyName}' was not returned with the expected value.");
        }
    }

    private static void AssertNumber(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Number)
        {
            throw new InvalidOperationException($"Expected numeric property '{propertyName}' was not returned.");
        }
    }

    private static void AssertDecimal(JsonElement element, string propertyName, decimal expected)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Number ||
            property.GetDecimal() != expected)
        {
            throw new InvalidOperationException($"Expected decimal property '{propertyName}' to be {expected}.");
        }
    }

    private sealed class ManagedProcess : IDisposable
    {
        private readonly string _name;
        private readonly Process _process;

        public ManagedProcess(string name, Process process)
        {
            _name = name;
            _process = process;
        }

        public void Dispose()
        {
            if (_process.HasExited)
            {
                _process.Dispose();
                return;
            }

            try
            {
                _process.Kill(entireProcessTree: true);
                _process.WaitForExit(TimeSpan.FromSeconds(5));
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Could not stop {_name}: {exception.Message}");
            }
            finally
            {
                _process.Dispose();
            }
        }
    }
}
