using ProjectManagement.Web.Models;

namespace ProjectManagement.Web.Services;

/// <summary>本週待完成 / 上週完成 總覽用的一筆資料。</summary>
public record DueSoonItem(string Type, string Title, string? Owner, DateTime? PlannedEndDate, bool IsOverdue, string Url);

/// <summary>上週完成的專案子工作項目。Owner 是該專案的負責人（用來篩選）。</summary>
public record CompletedSubTaskItem(string ProjectName, string? Owner, string SubTaskName, decimal EstimatedHours, DateTime CompletedAt, string Url);

/// <summary>
/// 上週一般需求的進度更新（不一定是完成，可能只是進度往前推進）。
/// Owner 是該需求的負責人（用來篩選），跟 ChangedByName（實際按下更新的人）不一定是同一人。
/// </summary>
public record RequirementUpdateItem(string Title, string? Owner, int OldProgressPercent, int NewProgressPercent, RequirementStatus? OldStatus, RequirementStatus? NewStatus, string? Note, string? ChangedByName, DateTime ChangedAt, string Url);

public record WeeklyOverviewResult(
    DateTime WeekStart,
    DateTime WeekEnd,
    DateTime LastWeekStart,
    DateTime LastWeekEnd,
    List<DueSoonItem> DueThisWeek,
    List<CompletedSubTaskItem> CompletedSubTasksLastWeek,
    List<RequirementUpdateItem> RequirementUpdatesLastWeek);

/// <summary>單一單位（部門）的工時耗用統計。Hours 為 null 代表完全沒有資料可算（跟 0 小時不同）。</summary>
public record DepartmentResourceUsage(
    string DepartmentName,
    decimal ProjectTotalHours,
    decimal ProjectCompletedHours,
    decimal RequirementTotalHours,
    decimal RequirementCompletedHours)
{
    public decimal GrandTotalHours => ProjectTotalHours + RequirementTotalHours;
    public decimal GrandCompletedHours => ProjectCompletedHours + RequirementCompletedHours;
}

/// <summary>甘特圖用的一筆資料（專案或一般需求）。</summary>
public record GanttItem(
    string Type,
    string Title,
    string? Owner,
    DateTime? PlannedStartDate,
    DateTime? PlannedEndDate,
    string StatusText,
    string ColorClass,
    decimal ProgressPercent,
    bool IsOverdue,
    string Url);

/// <summary>需求行事曆用的一筆標記（到期或完成）。</summary>
public record CalendarItem(
    string Type,
    string Title,
    DateTime Date,
    string Kind,
    string ColorClass,
    string Url);

public interface IReportService
{
    /// <summary>當週（週一～週日）預計要完成的工作項目，以及上週實際完成／更新的內容。</summary>
    Task<WeeklyOverviewResult> GetWeeklyOverviewAsync(DateTime? referenceDate = null);

    /// <summary>以單位（部門）為角度統計耗用工時（專案子工作項目時數 + 一般需求評估工時）。</summary>
    Task<List<DepartmentResourceUsage>> GetResourceUsageByDepartmentAsync();

    /// <summary>未結案（狀態非「已完成」）且已有預計開始或結束時間的專案與一般需求，供甘特圖使用。</summary>
    Task<List<GanttItem>> GetOpenItemsForGanttAsync();

    /// <summary>某年某月的行事曆標記：每個專案／需求的到期日，以及該月內實際完成的日期。</summary>
    Task<List<CalendarItem>> GetCalendarItemsAsync(int year, int month);
}
