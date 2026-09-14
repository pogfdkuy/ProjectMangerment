namespace ProjectManagement.Web.Models;

/// <summary>
/// 系統角色。對應 ASP.NET Core Identity 的 Role 名稱（見 DbSeeder.RoleNames）。
/// </summary>
public static class RoleNames
{
    public const string Admin = "Admin";     // 系統管理員：使用者/角色/分類維護，所有資料存取
    public const string Manager = "Manager"; // 主管：建立專案/需求、指派負責人、檢視報表
    public const string Member = "Member";   // 承辦人：僅能維護自己負責的專案/需求
    public const string Viewer = "Viewer";   // 唯讀：僅能檢視

    public static readonly string[] All = [Admin, Manager, Member, Viewer];
}

public enum ProjectStatus
{
    NotStarted = 0,   // 未開始
    InProgress = 1,   // 進行中
    Completed = 2,    // 已完成
    OnHold = 3         // 暫停
}

public enum RequirementStatus
{
    NotStarted = 0,   // 未開始
    InProgress = 1,   // 進行中
    Completed = 2,    // 已完成
    OnHold = 3         // 擱置
}

/// <summary>
/// 附件、通知、說明記錄等共用的「掛載對象」型別，用來標示這筆資料是掛在專案、一般需求、
/// 專案子工作項目，還是一筆說明記錄（<see cref="ItemNote"/>）底下。
/// </summary>
public enum OwnerEntityType
{
    Project = 0,
    GeneralRequirement = 1,
    ProjectSubTask = 2,

    /// <summary>掛在某一筆 <see cref="ItemNote"/> 底下的附件（讓每則說明可以各自附檔案）。</summary>
    Note = 3
}
