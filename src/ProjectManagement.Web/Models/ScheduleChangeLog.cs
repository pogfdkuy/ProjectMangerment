using ProjectManagement.Web.Data;

namespace ProjectManagement.Web.Models;

/// <summary>
/// 預計開始／結束時間的異動歷程（稽核用）：專案或一般需求的「預計開始時間」「預計結束時間」
/// 只要被改動，就會各自寫一筆記錄，記下改之前跟改之後的值、誰改的、什麼時候改的。
/// 用 <see cref="OwnerEntityType"/> + OwnerId 掛在 Project 或 GeneralRequirement 底下，
/// 作法跟 <see cref="Attachment"/>／<see cref="ItemNote"/> 一樣（目前只會是 Project 或 GeneralRequirement）。
/// </summary>
public class ScheduleChangeLog
{
    public int Id { get; set; }

    public OwnerEntityType OwnerType { get; set; }
    public int OwnerId { get; set; }

    public DateTime? OldPlannedStartDate { get; set; }
    public DateTime? NewPlannedStartDate { get; set; }

    public DateTime? OldPlannedEndDate { get; set; }
    public DateTime? NewPlannedEndDate { get; set; }

    public string ChangedByUserId { get; set; } = string.Empty;
    public ApplicationUser? ChangedByUser { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
