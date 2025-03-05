using System.Net;
using System.Net.Mail;
using EmailServices.Contracts;
using EmailServices.Interface;

namespace EmailServices.Service;

public class EmailService : IEmailService
{
    private readonly string _server = "smtp.gmail.com";
    private readonly int _host = 587;
    private readonly int _timeout = 60000;
    private readonly Boolean _enableSsl = true;
    private readonly Boolean _defaultCredentials = false;

    public async Task SendEmail(
        string username,
        string password,
        EmailSendingDetails emailSendingDetails
    )
    {
        try
        {
            using (MailMessage emailMessage = new MailMessage())
            {
                emailMessage.From = new MailAddress(username, emailSendingDetails.DisplayName);
                emailMessage.Body = emailSendingDetails.Body;
                emailMessage.Subject = emailSendingDetails.Subject;
                emailMessage.IsBodyHtml = emailSendingDetails.IsBodyHtml;
                emailMessage.Priority = emailSendingDetails.MailPriority;
                emailSendingDetails.MailAddressesTo.ForEach(mailAddressTo =>
                {
                    emailMessage.To.Add(mailAddressTo);
                });

                using (var smtpClient = new SmtpClient(_server, _host))
                {
                    smtpClient.EnableSsl = _enableSsl;
                    smtpClient.Timeout = _timeout;
                    smtpClient.UseDefaultCredentials = _defaultCredentials;
                    smtpClient.Credentials = new NetworkCredential(username, password);

                    await smtpClient.SendMailAsync(emailMessage);
                }
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Error sending email. " + ex.Message);
        }
    }
}
