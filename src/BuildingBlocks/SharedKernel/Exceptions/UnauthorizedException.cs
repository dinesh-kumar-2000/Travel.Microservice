namespace SharedKernel.Exceptions;

public class UnauthorizedException : BaseApplicationException
{
    public UnauthorizedException(string message = "Unauthorized access") 
        : base(message)
    {
    }
}

