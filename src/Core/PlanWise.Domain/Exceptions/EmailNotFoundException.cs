namespace PlanWise.Domain.Exceptions;

public class EmailNotFoundException : Exception
{
    public EmailNotFoundException() : base("E-mail not found.") { }
}
