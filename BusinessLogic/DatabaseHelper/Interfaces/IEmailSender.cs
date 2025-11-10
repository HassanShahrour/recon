namespace Reconova.BusinessLogic.DatabaseHelper.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
