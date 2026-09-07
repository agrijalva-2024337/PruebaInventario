using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Options;

namespace SLCDM.Api.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        IReadOnlyList<EmailAttachment>? attachments = null,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.From))
        {
            _logger.LogWarning("SMTP deshabilitado o incompleto. No se envio '{Subject}' a {To}.", subject, to);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.From));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var body = new BodyBuilder { HtmlBody = htmlBody };
        if (attachments is not null)
        {
            foreach (var file in attachments)
            {
                body.Attachments.Add(file.FileName, file.Content, MimeKit.ContentType.Parse(file.ContentType));
            }
        }

        message.Body = body.ToMessageBody();

        using var client = new SmtpClient();
        var secure = _options.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
        await client.ConnectAsync(_options.Host, _options.Port, secure, cancellationToken);
        if (!string.IsNullOrWhiteSpace(_options.User))
        {
            var password = (_options.Password ?? string.Empty).Replace(" ", "", StringComparison.Ordinal);
            await client.AuthenticateAsync(_options.User.Trim(), password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
        _logger.LogInformation("Correo enviado a {To}: {Subject}", to, subject);
    }
}
