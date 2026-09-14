using Microsoft.EntityFrameworkCore;
using ProjectManagement.Web.Data;
using ProjectManagement.Web.Models;

namespace ProjectManagement.Web.Services;

/// <summary>
/// 站內通知服務。目前只寫入資料庫供畫面上的通知鈴鐺讀取；
/// 若未來要加 Email 通知，建議直接在 NotifyAsync 裡呼叫 IEmailSender 一併寄送，不需要更動呼叫端。
///
/// 這裡改用 IDbContextFactory 而不是直接注入 ApplicationDbContext：
/// 通知鈴鐺(NotificationBell)是放在版面配置(MainLayout)裡的元件，會跟目前顯示的頁面同時初始化、
/// 同時查資料庫。如果兩邊共用同一個 Scoped DbContext 實例，就會撞出
/// "A second operation was started on this context instance..." 的例外。
/// 每次操作都用工廠建立一個獨立的 DbContext，就不會跟頁面本身的查詢互相干擾。
/// </summary>
public class NotificationService(IDbContextFactory<ApplicationDbContext> dbFactory) : INotificationService
{
    public async Task NotifyAsync(string recipientUserId, string title, string? message, string? link)
    {
        if (string.IsNullOrWhiteSpace(recipientUserId)) return;

        await using var db = await dbFactory.CreateDbContextAsync();

        db.Notifications.Add(new Notification
        {
            RecipientUserId = recipientUserId,
            Title = title,
            Message = message,
            Link = link
        });
        await db.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetRecentAsync(string userId, int take = 20)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.Notifications
            .Where(n => n.RecipientUserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.Notifications.CountAsync(n => n.RecipientUserId == userId && !n.IsRead);
    }

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == userId);
        if (notification is null) return;

        notification.IsRead = true;
        await db.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        await db.Notifications
            .Where(n => n.RecipientUserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }
}
