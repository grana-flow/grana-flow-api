namespace PlanWise.Domain.Exceptions;

public class EmailAlreadyRegisteredException : Exception
{
    public EmailAlreadyRegisteredException(string email) : base($"E-mail '{email}' is already registered.") { }
}
