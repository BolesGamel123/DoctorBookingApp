namespace Application.Common.Exceptions;

/// <summary>
/// Thrown by the ValidationBehavior pipeline when FluentValidation fails.
/// Caught by GlobalExceptionHandlerMiddleware to return a 400 response.
/// </summary>
public sealed class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.ToList().AsReadOnly();
    }
}
