using DictionaryAPI.Interfaces;
using DictionaryAPI.Models.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress(userName, toEmail));
        message.Subject = "Confirma tu correo — VerboCulto";

        message.Body = new TextPart("html")
        {
            Text = $"""
                <div style="font-family: sans-serif; max-width: 480px; margin: 0 auto;">
                  <h2>Bienvenido, {userName}</h2>
                  <p>Confirma tu correo electrónico para activar tu cuenta:</p>
                  <a href="{confirmationLink}"
                     style="display: inline-block; padding: 0.75rem 1.5rem;
                            background: #6366f1; color: white; border-radius: 8px;
                            text-decoration: none; font-weight: 500;">
                    Confirmar correo
                  </a>
                  <p style="margin-top: 1rem; color: #6b7280; font-size: 0.875rem;">
                    Si no creaste esta cuenta, ignora este mensaje.
                  </p>
                </div>
            """
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}