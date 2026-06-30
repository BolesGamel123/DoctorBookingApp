namespace Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested entity is not found.
/// Caught by GlobalExceptionHandlerMiddleware to return a 404 response.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.") { }
}
