using System.Net.Mail;

namespace EmailServices.Contracts;

public class EmailSendingDetails
{
    public string DisplayName { get; protected set; }
    public string Body { get; protected set; }
    public string Subject { get; protected set; }
    public Boolean IsBodyHtml { get; protected set; }
    public MailPriority MailPriority { get; protected set; }
    public List<string> MailAddressesTo { get; protected set; }

    public EmailSendingDetails(
        string displayName,
        string body,
        string subject,
        bool isBodyHtml,
        MailPriority mailPriority,
        List<string> mailAddressesTo
    )
    {
        DisplayName = displayName;
        Body = body;
        Subject = subject;
        IsBodyHtml = isBodyHtml;
        MailPriority = mailPriority;
        MailAddressesTo = mailAddressesTo;
    }
}
