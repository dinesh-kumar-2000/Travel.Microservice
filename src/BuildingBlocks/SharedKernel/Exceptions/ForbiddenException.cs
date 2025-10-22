namespace SharedKernel.Exceptions;

public class ForbiddenException : BaseApplicationException
{
    public ForbiddenException(string message = "Access forbidden") 
        : base(message)
    {
    }
}

