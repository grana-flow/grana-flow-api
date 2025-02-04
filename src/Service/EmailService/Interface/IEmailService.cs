using EmailServices.Contracts;

namespace EmailServices.Interface;

public interface IEmailService
{
    Task SendEmail(string username, string password, EmailSendingDetails emailSendingDetails);
}
