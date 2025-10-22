namespace SharedKernel.Exceptions;

public class ValidationException : BaseApplicationException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : base("Validation failure.")
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }
}

