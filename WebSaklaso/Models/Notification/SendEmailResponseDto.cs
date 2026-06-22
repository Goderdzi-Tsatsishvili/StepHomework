namespace WebSaklaso.Models.Notification
{
    public record SendEmailResponseDto(bool success, string message, Exception error = null);
}
