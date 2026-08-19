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
              <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f4f4;">
              <tr>
                <td align="center" style="padding:20px 0;">
                  <table width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; background-color:#ffffff; border-radius:8px;">
                    <tr>
                      <td style="padding:30px 20px;">
                        <h2 style="font-size:24px; color:#222222; margin-bottom:16px; font-weight:700;  text-align: center;">
                          ¡Te damos la bienvenida, {userName}!
                        </h2>
                        <p style="font-size:16px; color:#474744; margin-bottom:16px; line-height:1.6;">
                          Confirma tu correo electrónico para activar tu cuenta de VerboCulto y ser parte de la comunidad:
                        </p>
                        <p style="margin:24px 0 24px 0; text-align:center;">                                
                            <a href="{confirmationLink}"
                                style="display:inline-block; background-color:#BF7449; color:#ffffff; text-decoration:none; font-weight:bold; padding:14px 28px; border-radius:8px; font-size:16px;">
                            Confirmar correo
                            </a>
                        </p>

                        <!-- Párrafo 2 -->
                        <p style="font-size:12px; color:#5555558a; margin-bottom:12px; padding-top: 2rem; line-height:1.6;">
                          Si no creaste esta cuenta, ignora este mensaje.
                        </p>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
            """
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}