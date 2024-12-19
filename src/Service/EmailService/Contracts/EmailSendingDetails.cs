using System.Net.Mail;

namespace EmailServices.Contracts;

public class EmailSendingDetails
{
    public string Username { get; protected set; }
    public string PasswordApp { get; protected set; }
    public string DisplayName { get; protected set; }
    public string Body { get; protected set; }
    public string Subject { get; protected set; }
    public Boolean IsBodyHtml { get; protected set; }
    public MailPriority MailPriority { get; protected set; }
    public List<string> MailAddressesTo { get; protected set; }

    public EmailSendingDetails(
        string username,
        string passwordApp,
        string displayName,
        string body,
        string subject,
        bool isBodyHtml,
        MailPriority mailPriority,
        List<string> mailAddressesTo
    )
    {
        Username = username;
        PasswordApp = passwordApp;
        DisplayName = displayName;
        Body = body;
        Subject = subject;
        IsBodyHtml = isBodyHtml;
        MailPriority = mailPriority;
        MailAddressesTo = mailAddressesTo;
    }
}
