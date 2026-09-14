using Microsoft.EntityFrameworkCore;
using ProjectManagement.Web.Data;
using ProjectManagement.Web.Models;

namespace ProjectManagement.Web.Services;

/// <summary>
/// 儀表板／報表用的彙整查詢。跟 ProjectService／RequirementService 不同，這裡只做「唯讀彙整」，
/// 不做任何新增/修改，所以沒有依賴 INotificationService。理由同其他服務：用 IDbContextFactory
/// 建立獨立的短生命週期 DbContext，避免跟畫面上其他元件共用同一個 Scoped DbContext 實例。
/// </summary>
public class ReportService(IDbContextFactory<ApplicationDbContext> dbFactory) : IReportService
{
    public async Task<WeeklyOverviewResult> GetWeeklyOverviewAsync(DateTime? referenceDate = null)
    {
        var today = (referenceDate ?? DateTime.Today).Date;
        var weekStart = StartOfWeek(today);
        var weekEnd = weekStart.AddDays(6);
        var lastWeekStart = weekStart.AddDays(-7);
        var lastWeekEnd = weekStart.AddDays(-1);

        await using var db = await dbFactory.CreateDbContextAsync();

        var projects = await db.Projects
            .Include(p => p.ResponsibleUser)
            .Include(p => p.SubTasks)
            .ToListAsync();

        var requirements = await db.GeneralRequirements
            .Include(r => r.ResponsibleUser)
            .ToListAsync();

        var dueThisWeek = new List<DueSoonItem>();

        foreach (var p in projects)
        {
            foreach (var t in p.SubTasks)
            {
                if (t.IsCompleted || t.PlannedEndDate is null) continue;
                if (t.PlannedEndDate.Value.Date < weekStart || t.PlannedEndDate.Value.Date > weekEnd) continue;

                dueThisWeek.Add(new DueSoonItem(
                    "專案子項目",
                    $"{p.Name} - {t.Name}",
                    p.ResponsibleDisplayName,
                    t.PlannedEndDate,
                    t.PlannedEndDate.Value.Date < today,
                    $"/projects/{p.Id}"));
            }
        }

        foreach (var r in requirements)
        {
            if (r.Status == RequirementStatus.Completed || r.PlannedEndDate is null) continue;
            if (r.PlannedEndDate.Value.Date < weekStart || r.PlannedEndDate.Value.Date > weekEnd) continue;

            dueThisWeek.Add(new DueSoonItem(
                "一般需求",
                r.Title,
                r.ResponsibleDisplayName,
                r.PlannedEndDate,
                r.PlannedEndDate.Value.Date < today,
                $"/requirements/{r.Id}"));
        }

        dueThisWeek = dueThisWeek.OrderBy(i => i.PlannedEndDate).ToList();

        var completedSubTasks = new List<CompletedSubTaskItem>();
        foreach (var p in projects)
        {
            foreach (var t in p.SubTasks)
            {
                if (!t.IsCompleted || t.CompletedAt is null) continue;
                if (t.CompletedAt.Value.Date < lastWeekStart || t.CompletedAt.Value.Date > lastWeekEnd) continue;

                completedSubTasks.Add(new CompletedSubTaskItem(p.Name, p.ResponsibleDisplayName, t.Name, t.EstimatedHours, t.CompletedAt.Value, $"/projects/{p.Id}"));
            }
        }
        completedSubTasks = completedSubTasks.OrderByDescending(i => i.CompletedAt).ToList();

        var requirementUpdates = await db.GeneralRequirementProgressLogs
            .Include(l => l.GeneralRequirement!.ResponsibleUser)
            .Include(l => l.ChangedByUser)
            .Where(l => l.ChangedAt >= lastWeekStart && l.ChangedAt < lastWeekEnd.AddDays(1))
            .OrderByDescending(l => l.ChangedAt)
            .ToListAsync();

        var requirementUpdateItems = requirementUpdates
            .Where(l => l.GeneralRequirement is not null)
            .Select(l => new RequirementUpdateItem(
                l.GeneralRequirement!.Title,
                l.GeneralRequirement!.ResponsibleDisplayName,
                l.OldProgressPercent,
                l.NewProgressPercent,
                l.OldStatus,
                l.NewStatus,
                l.Note,
                l.ChangedByUser?.DisplayName,
                l.ChangedAt,
                $"/requirements/{l.GeneralRequirementId}"))
            .ToList();

        return new WeeklyOverviewResult(weekStart, weekEnd, lastWeekStart, lastWeekEnd, dueThisWeek, completedSubTasks, requirementUpdateItems);
    }

    public async Task<List<DepartmentResourceUsage>> GetResourceUsageByDepartmentAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var projects = await db.Projects
            .Include(p => p.Department)
            .Include(p => p.SubTasks)
            .ToListAsync();

        var requirements = await db.GeneralRequirements
            .Include(r => r.Department)
            .ToListAsync();

        const string unassigned = "未分類單位";

        var projectGroups = projects
            .GroupBy(p => p.Department?.Name ?? unassigned)
            .ToDictionary(
                g => g.Key,
                g => (
                    Total: g.Sum(p => p.SubTasks.Sum(t => t.EstimatedHours)),
                    Completed: g.Sum(p => p.SubTasks.Where(t => t.IsCompleted).Sum(t => t.EstimatedHours))));

        var requirementGroups = requirements
            .GroupBy(r => r.Department?.Name ?? unassigned)
            .ToDictionary(
                g => g.Key,
                g => (
                    Total: g.Sum(r => r.EstimatedHours ?? 0m),
                    Completed: g.Sum(r => r.Status == RequirementStatus.Completed ? (r.EstimatedHours ?? 0m) : 0m)));

        var names = projectGroups.Keys.Union(requirementGroups.Keys);

        var result = new List<DepartmentResourceUsage>();
        foreach (var name in names)
        {
            var p = projectGroups.TryGetValue(name, out var pv) ? pv : (Total: 0m, Completed: 0m);
            var r = requirementGroups.TryGetValue(name, out var rv) ? rv : (Total: 0m, Completed: 0m);
            result.Add(new DepartmentResourceUsage(name, p.Total, p.Completed, r.Total, r.Completed));
        }

        return result.OrderByDescending(d => d.GrandTotalHours).ToList();
    }

    public async Task<List<GanttItem>> GetOpenItemsForGanttAsync()
    {
        var today = DateTime.Today;

        await using var db = await dbFactory.CreateDbContextAsync();

        var projects = await db.Projects
            .Include(p => p.ResponsibleUser)
            .Include(p => p.SubTasks)
            .Where(p => p.Status != ProjectStatus.Completed)
            .ToListAsync();

        var requirements = await db.GeneralRequirements
            .Include(r => r.ResponsibleUser)
            .Where(r => r.Status != RequirementStatus.Completed)
            .ToListAsync();

        var items = new List<GanttItem>();

        foreach (var p in projects)
        {
            if (p.PlannedStartDate is null && p.PlannedEndDate is null) continue;
            var overdue = p.PlannedEndDate is not null && p.PlannedEndDate.Value.Date < today;
            items.Add(new GanttItem(
                "專案", p.Name, p.ResponsibleDisplayName,
                p.PlannedStartDate, p.PlannedEndDate,
                StatusText(p.Status), GanttColorClass(p.Status.ToString(), overdue),
                p.CompletionPercent, overdue, $"/projects/{p.Id}"));
        }

        foreach (var r in requirements)
        {
            if (r.PlannedStartDate is null && r.PlannedEndDate is null) continue;
            var overdue = r.PlannedEndDate is not null && r.PlannedEndDate.Value.Date < today;
            items.Add(new GanttItem(
                "需求", r.Title, r.ResponsibleDisplayName,
                r.PlannedStartDate, r.PlannedEndDate,
                StatusText(r.Status), GanttColorClass(r.Status.ToString(), overdue),
                r.ProgressPercent, overdue, $"/requirements/{r.Id}"));
        }

        return items.OrderBy(i => i.PlannedStartDate ?? i.PlannedEndDate).ToList();
    }

    public async Task<List<CalendarItem>> GetCalendarItemsAsync(int year, int month)
    {
        var monthStart = new DateTime(year, month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var today = DateTime.Today;

        await using var db = await dbFactory.CreateDbContextAsync();

        var items = new List<CalendarItem>();

        var projects = await db.Projects.ToListAsync();
        foreach (var p in projects)
        {
            if (p.PlannedEndDate is not { } due || due.Date < monthStart || due.Date > monthEnd) continue;
            var overdue = p.Status != ProjectStatus.Completed && due.Date < today;
            items.Add(new CalendarItem("專案", p.Name, due.Date, "到期", overdue ? "cal-danger" : "cal-info", $"/projects/{p.Id}"));
        }

        var requirements = await db.GeneralRequirements.ToListAsync();
        foreach (var r in requirements)
        {
            if (r.PlannedEndDate is not { } due || due.Date < monthStart || due.Date > monthEnd) continue;
            var overdue = r.Status != RequirementStatus.Completed && due.Date < today;
            items.Add(new CalendarItem("需求", r.Title, due.Date, "到期", overdue ? "cal-danger" : "cal-info", $"/requirements/{r.Id}"));
        }

        // 實際完成日：一般需求進度更新到「已完成」那一筆記錄的日期。
        var completedLogs = await db.GeneralRequirementProgressLogs
            .Include(l => l.GeneralRequirement)
            .Where(l => l.NewStatus == RequirementStatus.Completed && l.ChangedAt >= monthStart && l.ChangedAt < monthEnd.AddDays(1))
            .ToListAsync();
        foreach (var log in completedLogs)
        {
            if (log.GeneralRequirement is null) continue;
            items.Add(new CalendarItem("需求", log.GeneralRequirement.Title, log.ChangedAt.Date, "完成", "cal-success", $"/requirements/{log.GeneralRequirementId}"));
        }

        // 專案子工作項目完成：標記在專案層級（避免行事曆被大量子項目洗版）。
        var completedSubTasks = await db.ProjectSubTasks
            .Include(t => t.Project)
            .Where(t => t.IsCompleted && t.CompletedAt != null && t.CompletedAt >= monthStart && t.CompletedAt < monthEnd.AddDays(1))
            .ToListAsync();
        foreach (var t in completedSubTasks)
        {
            if (t.Project is null || t.CompletedAt is null) continue;
            items.Add(new CalendarItem("專案", $"{t.Project.Name} - {t.Name}", t.CompletedAt.Value.Date, "完成", "cal-success", $"/projects/{t.ProjectId}"));
        }

        return items.OrderBy(i => i.Date).ToList();
    }

    public async Task<List<ScoreRow>> GetScoreRowsAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var projects = await db.Projects
            .Include(p => p.ResponsibleUser)
            .Include(p => p.SubTasks)
            .ToListAsync();

        var requirements = await db.GeneralRequirements
            .Include(r => r.ResponsibleUser)
            .ToListAsync();

        var projectRows = projects.Select(p => new ScoreRow(
            "專案",
            p.Name,
            p.ResponsibleDisplayName ?? "未指派",
            p.Status == ProjectStatus.Completed || p.CompletionPercent >= 100,
            p.CreatedAt,
            p.DifficultyBefore,
            p.DifficultyBeforeNote,
            p.DifficultyAfter,
            p.DifficultyAfterNote,
            p.HasPrimaryBenefit,
            p.HasPrimaryBenefitNote,
            p.HasLongTermBenefit,
            p.HasLongTermBenefitNote,
            p.HasCrossUnitBenefit,
            p.HasCrossUnitBenefitNote,
            p.DifficultyReductionScore,
            p.BenefitScore,
            p.TotalScore,
            $"/projects/{p.Id}"));

        var requirementRows = requirements.Select(r => new ScoreRow(
            "需求",
            r.Title,
            r.ResponsibleDisplayName ?? "未指派",
            r.Status == RequirementStatus.Completed,
            r.CreatedAt,
            r.DifficultyBefore,
            r.DifficultyBeforeNote,
            r.DifficultyAfter,
            r.DifficultyAfterNote,
            r.HasPrimaryBenefit,
            r.HasPrimaryBenefitNote,
            r.HasLongTermBenefit,
            r.HasLongTermBenefitNote,
            r.HasCrossUnitBenefit,
            r.HasCrossUnitBenefitNote,
            r.DifficultyReductionScore,
            r.BenefitScore,
            r.TotalScore,
            $"/requirements/{r.Id}"));

        return projectRows.Concat(requirementRows).OrderByDescending(r => r.TotalScore).ToList();
    }

    /// <summary>週一為每週的第一天。</summary>
    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }

    private static string StatusText(ProjectStatus status) => status switch
    {
        ProjectStatus.NotStarted => "未開始",
        ProjectStatus.InProgress => "進行中",
        ProjectStatus.Completed => "已完成",
        ProjectStatus.OnHold => "暫停",
        _ => status.ToString()
    };

    private static string StatusText(RequirementStatus status) => status switch
    {
        RequirementStatus.NotStarted => "未開始",
        RequirementStatus.InProgress => "進行中",
        RequirementStatus.Completed => "已完成",
        RequirementStatus.OnHold => "擱置",
        _ => status.ToString()
    };

    /// <summary>
    /// 專案／需求的狀態列舉字面值剛好都叫 NotStarted/InProgress/Completed/OnHold，
    /// 用字串比對就能共用同一套顏色規則，不用為兩個列舉各寫一份。
    /// </summary>
    private static string GanttColorClass(string statusName, bool overdue)
    {
        if (overdue) return "gantt-bar-danger";
        return statusName switch
        {
            nameof(ProjectStatus.InProgress) => "gantt-bar-primary",
            nameof(ProjectStatus.OnHold) => "gantt-bar-warning",
            nameof(ProjectStatus.Completed) => "gantt-bar-success",
            _ => "gantt-bar-secondary"
        };
    }
}
