namespace Payment.Orchestrator.UnitTests.Support;

internal static class Assert
{
    public static void Equal<T>(T expected, T actual, string name)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected {name} to be '{expected}', but was '{actual}'.");
        }
    }

    public static void True(bool actual, string name)
    {
        if (!actual)
        {
            throw new InvalidOperationException($"Expected {name} to be true.");
        }
    }

    public static void False(bool actual, string name)
    {
        if (actual)
        {
            throw new InvalidOperationException($"Expected {name} to be false.");
        }
    }

    public static void Single<T>(IReadOnlyCollection<T> items, string name)
    {
        if (items.Count != 1)
        {
            throw new InvalidOperationException($"Expected {name} to have 1 item, but it had {items.Count}.");
        }
    }

    public static void Contains(string expected, string actual, string name)
    {
        if (!actual.Contains(expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected {name} to contain '{expected}', but was '{actual}'.");
        }
    }

    public static async Task<TException> ThrowsAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException exception)
        {
            return exception;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Expected exception {typeof(TException).Name}, but got {exception.GetType().Name}.", exception);
        }

        throw new InvalidOperationException($"Expected exception {typeof(TException).Name}, but no exception was thrown.");
    }
}
