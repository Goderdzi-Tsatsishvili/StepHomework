namespace WebSaklaso.Models.Notification
{
    public record SendEmailRequestDto(string to, string subject, string body);
}
