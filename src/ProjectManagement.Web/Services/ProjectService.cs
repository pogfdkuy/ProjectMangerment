using Microsoft.EntityFrameworkCore;
using ProjectManagement.Web.Data;
using ProjectManagement.Web.Models;

namespace ProjectManagement.Web.Services;

/// <summary>
/// 改用 IDbContextFactory 而不是直接注入 ApplicationDbContext：每次操作都建立一個獨立的短生命週期
/// DbContext，避免跟畫面上其他同時執行的元件（例如版面配置裡的通知鈴鐺）共用同一個 Scoped DbContext
/// 實例而互相干擾，撞出 "A second operation was started on this context instance..." 的例外。
/// </summary>
public class ProjectService(IDbContextFactory<ApplicationDbContext> dbFactory, INotificationService notifications) : IProjectService
{
    private static IQueryable<Project> WithIncludes(ApplicationDbContext db) =>
        db.Projects
            .Include(p => p.Department)
            .Include(p => p.SystemCategory)
            .Include(p => p.ResponsibleUser)
            .Include(p => p.RequesterUser)
            .Include(p => p.DeveloperUser)
            .Include(p => p.SubTasks);

    public async Task<List<Project>> GetListAsync(int? departmentId = null, int? systemCategoryId = null, ProjectStatus? status = null, string? keyword = null, string? responsibleUserId = null)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var query = WithIncludes(db);

        if (departmentId is not null) query = query.Where(p => p.DepartmentId == departmentId);
        if (systemCategoryId is not null) query = query.Where(p => p.SystemCategoryId == systemCategoryId);
        if (status is not null) query = query.Where(p => p.Status == status);
        if (!string.IsNullOrWhiteSpace(responsibleUserId)) query = query.Where(p => p.ResponsibleUserId == responsibleUserId);
        if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(p => p.Name.Contains(keyword));

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await WithIncludes(db).FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project> CreateAsync(Project project)
    {
        NormalizeResponsiblePersonFields(project);
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;

        await using var db = await dbFactory.CreateDbContextAsync();
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(project.ResponsibleUserId))
        {
            await notifications.NotifyAsync(
                project.ResponsibleUserId,
                "你被指派為專案負責人",
                $"專案「{project.Name}」已指派給你負責。",
                $"/projects/{project.Id}");
        }

        return project;
    }

    public async Task UpdateAsync(Project project, string? changedByUserId = null)
    {
        NormalizeResponsiblePersonFields(project);

        await using var db = await dbFactory.CreateDbContextAsync();
        var existing = await db.Projects.FirstOrDefaultAsync(p => p.Id == project.Id)
            ?? throw new InvalidOperationException($"Project {project.Id} not found.");

        var responsibleChanged = existing.ResponsibleUserId != project.ResponsibleUserId
            && !string.IsNullOrWhiteSpace(project.ResponsibleUserId);

        // 預計開始／結束時間只要有任何一個被改動，就寫一筆時程異動歷程（稽核用）。
        // 沒有 changedByUserId（理論上不會發生，呼叫端都會帶入目前登入者）就不記錄，避免寫入無效的操作者。
        if (!string.IsNullOrWhiteSpace(changedByUserId) &&
            (existing.PlannedStartDate != project.PlannedStartDate || existing.PlannedEndDate != project.PlannedEndDate))
        {
            db.ScheduleChangeLogs.Add(new ScheduleChangeLog
            {
                OwnerType = OwnerEntityType.Project,
                OwnerId = existing.Id,
                OldPlannedStartDate = existing.PlannedStartDate,
                NewPlannedStartDate = project.PlannedStartDate,
                OldPlannedEndDate = existing.PlannedEndDate,
                NewPlannedEndDate = project.PlannedEndDate,
                ChangedByUserId = changedByUserId,
                ChangedAt = DateTime.UtcNow
            });
        }

        existing.Name = project.Name;
        existing.Description = project.Description;
        existing.Remarks = project.Remarks;
        existing.SystemName = project.SystemName;
        existing.ApplicationTicketNumber = project.ApplicationTicketNumber;
        existing.DepartmentId = project.DepartmentId;
        existing.SystemCategoryId = project.SystemCategoryId;
        existing.ResponsibleUserId = project.ResponsibleUserId;
        existing.ResponsibleUserName = project.ResponsibleUserName;
        existing.RequesterUserId = project.RequesterUserId;
        existing.RequesterUserName = project.RequesterUserName;
        existing.DeveloperUserId = project.DeveloperUserId;
        existing.DeveloperUserName = project.DeveloperUserName;
        existing.Status = project.Status;
        existing.PlannedStartDate = project.PlannedStartDate;
        existing.PlannedEndDate = project.PlannedEndDate;
        existing.DifficultyBefore = project.DifficultyBefore;
        existing.DifficultyBeforeNote = project.DifficultyBeforeNote;
        existing.DifficultyAfter = project.DifficultyAfter;
        existing.DifficultyAfterNote = project.DifficultyAfterNote;
        existing.HasPrimaryBenefit = project.HasPrimaryBenefit;
        existing.HasPrimaryBenefitNote = project.HasPrimaryBenefitNote;
        existing.HasLongTermBenefit = project.HasLongTermBenefit;
        existing.HasLongTermBenefitNote = project.HasLongTermBenefitNote;
        existing.HasCrossUnitBenefit = project.HasCrossUnitBenefit;
        existing.HasCrossUnitBenefitNote = project.HasCrossUnitBenefitNote;
        existing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        if (responsibleChanged)
        {
            await notifications.NotifyAsync(
                project.ResponsibleUserId!,
                "你被指派為專案負責人",
                $"專案「{existing.Name}」已指派給你負責。",
                $"/projects/{existing.Id}");
        }
    }

    public async Task<List<ScheduleChangeLog>> GetScheduleChangeLogsAsync(int projectId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.ScheduleChangeLogs
            .Include(l => l.ChangedByUser)
            .Where(l => l.OwnerType == OwnerEntityType.Project && l.OwnerId == projectId)
            .OrderByDescending(l => l.ChangedAt)
            .ToListAsync();
    }

    public async Task<ProjectSubTask> AddSubTaskAsync(int projectId, ProjectSubTask subTask)
    {
        subTask.ProjectId = projectId;
        subTask.CreatedAt = DateTime.UtcNow;

        await using var db = await dbFactory.CreateDbContextAsync();
        db.ProjectSubTasks.Add(subTask);

        var project = await db.Projects.FindAsync(projectId);
        if (project is not null) project.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return subTask;
    }

    public async Task UpdateSubTaskAsync(ProjectSubTask subTask)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var existing = await db.ProjectSubTasks.FirstOrDefaultAsync(t => t.Id == subTask.Id)
            ?? throw new InvalidOperationException($"SubTask {subTask.Id} not found.");

        existing.Name = subTask.Name;
        existing.Description = subTask.Description;
        existing.EstimatedHours = subTask.EstimatedHours;
        existing.PlannedStartDate = subTask.PlannedStartDate;
        existing.PlannedEndDate = subTask.PlannedEndDate;

        await TouchProjectAsync(db, existing.ProjectId);
        await db.SaveChangesAsync();
    }

    public async Task SetSubTaskCompletionAsync(int subTaskId, bool isCompleted)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var subTask = await db.ProjectSubTasks.FirstOrDefaultAsync(t => t.Id == subTaskId)
            ?? throw new InvalidOperationException($"SubTask {subTaskId} not found.");

        subTask.IsCompleted = isCompleted;
        subTask.CompletedAt = isCompleted ? DateTime.UtcNow : null;

        await TouchProjectAsync(db, subTask.ProjectId);
        await db.SaveChangesAsync();
    }

    public async Task DeleteSubTaskAsync(int subTaskId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var subTask = await db.ProjectSubTasks.FindAsync(subTaskId);
        if (subTask is null) return;

        db.ProjectSubTasks.Remove(subTask);
        await TouchProjectAsync(db, subTask.ProjectId);
        await db.SaveChangesAsync();
    }

    private static async Task TouchProjectAsync(ApplicationDbContext db, int projectId)
    {
        var project = await db.Projects.FindAsync(projectId);
        if (project is not null) project.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 負責人／需求者／開發者都可以「從清單選」或「手動輸入姓名」二選一：
    /// 下拉選單「未指定」選項對 string 型別的欄位（如 ResponsibleUserId）會綁定成空字串而非 null，
    /// 這裡統一轉成 null，避免存進資料庫違反外鍵約束；同時只要有選到系統使用者（Id 有值），
    /// 手動輸入的姓名欄位就清空，避免兩邊資料不一致、顯示時不知道該用哪一個。
    /// </summary>
    private static void NormalizeResponsiblePersonFields(Project project)
    {
        (project.ResponsibleUserId, project.ResponsibleUserName) = NormalizePersonField(project.ResponsibleUserId, project.ResponsibleUserName);
        (project.RequesterUserId, project.RequesterUserName) = NormalizePersonField(project.RequesterUserId, project.RequesterUserName);
        (project.DeveloperUserId, project.DeveloperUserName) = NormalizePersonField(project.DeveloperUserId, project.DeveloperUserName);
    }

    private static (string? UserId, string? UserName) NormalizePersonField(string? userId, string? userName)
    {
        userId = string.IsNullOrWhiteSpace(userId) ? null : userId;
        userName = userId is not null ? null : (string.IsNullOrWhiteSpace(userName) ? null : userName.Trim());
        return (userId, userName);
    }
}
