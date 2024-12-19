using EmailServices.Contracts;

namespace EmailServices.Interface;

public interface IEmailService
{
    Task SendEmail(EmailSendingDetails emailSendingDetails);
}
