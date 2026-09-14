using System.ComponentModel.DataAnnotations;
using ProjectManagement.Web.Data;

namespace ProjectManagement.Web.Models;

/// <summary>
/// 說明記錄：掛在一般需求或專案子工作項目底下、可隨時新增的自由文字說明（跟建立時填的靜態「說明」欄位不同，
/// 這裡是可以不斷累加的紀錄，類似留言串）。每則說明都可以各自上傳附件——
/// 附件用 <see cref="OwnerEntityType.Note"/> + 這筆記錄的 Id 掛在 <see cref="Attachment"/> 底下。
/// </summary>
public class ItemNote
{
    public int Id { get; set; }

    /// <summary>目前只會是 GeneralRequirement 或 ProjectSubTask。</summary>
    public OwnerEntityType OwnerType { get; set; }
    public int OwnerId { get; set; }

    [Required, StringLength(2000)]
    public string Content { get; set; } = string.Empty;

    public string CreatedByUserId { get; set; } = string.Empty;
    public ApplicationUser? CreatedByUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
