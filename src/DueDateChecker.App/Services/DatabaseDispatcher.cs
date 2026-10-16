using DueDateChecker.Core.Data;

namespace DueDateChecker.App.Services;

/// <summary>
/// Serializes access to the one app database and keeps synchronous SQLite work
/// off the UI thread. A failed initialization can be retried without deleting data.
/// </summary>
public sealed class DatabaseDispatcher(DatabaseInitializer initializer)
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _initialized;

    public Task InitializeAsync() => RunAsync(() => Task.CompletedTask);

    public Task RunAsync(Func<Task> operation) => RunAsync(async () =>
    {
        await operation().ConfigureAwait(false);
        return true;
    });

    public async Task<T> RunAsync<T>(Func<Task<T>> operation)
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            return await Task.Run(async () =>
            {
                if (!_initialized)
                {
                    initializer.Initialize();
                    _initialized = true;
                }

                return await operation().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }
}
