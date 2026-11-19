namespace GestorDeTareas.Business.Services;

public sealed record TaskServiceResult<T>(T? Value, IReadOnlyDictionary<string, string[]> Errors, bool NotFound)
{
    public static TaskServiceResult<T> Success(T value) => new(value, new Dictionary<string, string[]>(), false);

    public static TaskServiceResult<T> Invalid(IReadOnlyDictionary<string, string[]> errors) => new(default, errors, false);

    public static TaskServiceResult<T> Missing() => new(default, new Dictionary<string, string[]>(), true);
}