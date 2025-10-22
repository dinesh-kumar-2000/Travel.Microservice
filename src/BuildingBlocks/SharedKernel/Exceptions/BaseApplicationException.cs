namespace SharedKernel.Exceptions;

/// <summary>
/// Base class for all application exceptions
/// Reduces boilerplate code in custom exceptions
/// </summary>
public abstract class BaseApplicationException : Exception
{
    protected BaseApplicationException(string message) : base(message)
    {
    }

    protected BaseApplicationException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

