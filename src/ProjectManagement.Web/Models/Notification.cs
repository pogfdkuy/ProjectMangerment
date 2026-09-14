using System.ComponentModel.DataAnnotations;
using ProjectManagement.Web.Data;

namespace ProjectManagement.Web.Models;

/// <summary>
/// 站內通知。於「被指派為負責人」「進度被更新」「狀態變更為已完成」等事件觸發時建立。
/// 目前僅實作站內通知（頁面上的通知鈴鐺），Email 寄送可日後在 INotificationService 中擴充。
/// </summary>
public class Notification
{
    public int Id { get; set; }

    public string RecipientUserId { get; set; } = string.Empty;
    public ApplicationUser? RecipientUser { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Message { get; set; }

    /// <summary>點擊通知後導向的相對路徑，例如 /projects/12。</summary>
    [StringLength(300)]
    public string? Link { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
