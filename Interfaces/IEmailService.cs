namespace DictionaryAPI.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink);
    }
}
