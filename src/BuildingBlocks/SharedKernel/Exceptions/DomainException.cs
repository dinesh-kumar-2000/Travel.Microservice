namespace SharedKernel.Exceptions;

public class DomainException : BaseApplicationException
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

