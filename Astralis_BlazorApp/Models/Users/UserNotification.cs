namespace Astralis_BlazorApp.Models
{
    public class UserNotification
    {
        public int UserId { get; set; }

        public int NotificationId { get; set; }

        public bool ByMail { get; set; }
    }
}
