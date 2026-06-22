using WebSaklaso.Models.Notification;

namespace WebSaklaso.Service.Contracts
{
    public interface IEmailService
    {
        Task<SendEmailResponseDto> Send(string to, string subject, string body);
    }
}
