using ProjectManagement.Web.Models;

namespace ProjectManagement.Web.Services;

public interface INotificationService
{
    Task NotifyAsync(string recipientUserId, string title, string? message, string? link);

    Task<List<Notification>> GetRecentAsync(string userId, int take = 20);

    Task<int> GetUnreadCountAsync(string userId);

    Task MarkAsReadAsync(int notificationId, string userId);

    Task MarkAllAsReadAsync(string userId);
}
