namespace SLCDM.Application.Common.Interfaces;

public sealed record EmailAttachment(string FileName, string ContentType, byte[] Content);

public interface IEmailSender
{
    Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        IReadOnlyList<EmailAttachment>? attachments = null,
        CancellationToken cancellationToken = default);
}
