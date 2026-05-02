namespace CueCompanion.Services;

public abstract class AuthDependantService(AuthService auth) : StateSubscriberService
{
    private static readonly string InvalidSessionMessage = "Invalid session. Please log in again.";

    protected Task<Result> InvokeWithSessionAsync(Func<string, Task<Result>> action)
    {
        if (auth.SessionKey is not { } key)
            return Task.FromResult(Result.Failure(InvalidSessionMessage));

        return action(key);
    }

    protected Task<Result<T>> InvokeWithSessionAsync<T>(Func<string, Task<Result<T>>> action)
    {
        if (auth.SessionKey is not { } key)
            return Task.FromResult(Result<T>.Failure(InvalidSessionMessage));

        return action(key);
    }
}