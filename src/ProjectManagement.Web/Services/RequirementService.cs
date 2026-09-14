using Microsoft.EntityFrameworkCore;
using ProjectManagement.Web.Data;
using ProjectManagement.Web.Models;

namespace ProjectManagement.Web.Services;

/// <summary>
/// 改用 IDbContextFactory 而不是直接注入 ApplicationDbContext：每次操作都建立一個獨立的短生命週期
/// DbContext，避免跟畫面上其他同時執行的元件（例如版面配置裡的通知鈴鐺）共用同一個 Scoped DbContext
/// 實例而互相干擾，撞出 "A second operation was started on this context instance..." 的例外。
/// </summary>
public class RequirementService(IDbContextFactory<ApplicationDbContext> dbFactory, INotificationService notifications) : IRequirementService
{
    private static IQueryable<GeneralRequirement> WithIncludes(ApplicationDbContext db) =>
        db.GeneralRequirements
            .Include(r => r.Department)
            .Include(r => r.SystemCategory)
            .Include(r => r.ResponsibleUser)
            .Include(r => r.RequesterUser)
            .Include(r => r.DeveloperUser);

    public async Task<List<GeneralRequirement>> GetListAsync(int? departmentId = null, int? systemCategoryId = null, RequirementStatus? status = null, string? keyword = null, string? responsibleUserId = null)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var query = WithIncludes(db);

        if (departmentId is not null) query = query.Where(r => r.DepartmentId == departmentId);
        if (systemCategoryId is not null) query = query.Where(r => r.SystemCategoryId == systemCategoryId);
        if (status is not null) query = query.Where(r => r.Status == status);
        if (!string.IsNullOrWhiteSpace(responsibleUserId)) query = query.Where(r => r.ResponsibleUserId == responsibleUserId);
        if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(r => r.Title.Contains(keyword));

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<GeneralRequirement?> GetByIdAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.GeneralRequirements
            .Include(r => r.Department)
            .Include(r => r.SystemCategory)
            .Include(r => r.ResponsibleUser)
            .Include(r => r.RequesterUser)
            .Include(r => r.DeveloperUser)
            .Include(r => r.ProgressLogs.OrderByDescending(l => l.ChangedAt))
            .ThenInclude(l => l.ChangedByUser)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<GeneralRequirement> CreateAsync(GeneralRequirement requirement)
    {
        NormalizeResponsiblePersonFields(requirement);
        requirement.CreatedAt = DateTime.UtcNow;
        requirement.UpdatedAt = DateTime.UtcNow;

        await using var db = await dbFactory.CreateDbContextAsync();
        db.GeneralRequirements.Add(requirement);
        await db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(requirement.ResponsibleUserId))
        {
            await notifications.NotifyAsync(
                requirement.ResponsibleUserId,
                "你被指派為需求負責人",
                $"一般需求「{requirement.Title}」已指派給你負責。",
                $"/requirements/{requirement.Id}");
        }

        return requirement;
    }

    public async Task UpdateBasicInfoAsync(GeneralRequirement requirement, string? changedByUserId = null)
    {
        NormalizeResponsiblePersonFields(requirement);

        await using var db = await dbFactory.CreateDbContextAsync();
        var existing = await db.GeneralRequirements.FirstOrDefaultAsync(r => r.Id == requirement.Id)
            ?? throw new InvalidOperationException($"GeneralRequirement {requirement.Id} not found.");

        var responsibleChanged = existing.ResponsibleUserId != requirement.ResponsibleUserId
            && !string.IsNullOrWhiteSpace(requirement.ResponsibleUserId);

        // 預計開始／結束時間只要有任何一個被改動，就寫一筆時程異動歷程（稽核用）。
        // 沒有 changedByUserId（理論上不會發生，呼叫端都會帶入目前登入者）就不記錄，避免寫入無效的操作者。
        if (!string.IsNullOrWhiteSpace(changedByUserId) &&
            (existing.PlannedStartDate != requirement.PlannedStartDate || existing.PlannedEndDate != requirement.PlannedEndDate))
        {
            db.ScheduleChangeLogs.Add(new ScheduleChangeLog
            {
                OwnerType = OwnerEntityType.GeneralRequirement,
                OwnerId = existing.Id,
                OldPlannedStartDate = existing.PlannedStartDate,
                NewPlannedStartDate = requirement.PlannedStartDate,
                OldPlannedEndDate = existing.PlannedEndDate,
                NewPlannedEndDate = requirement.PlannedEndDate,
                ChangedByUserId = changedByUserId,
                ChangedAt = DateTime.UtcNow
            });
        }

        existing.Title = requirement.Title;
        existing.Description = requirement.Description;
        existing.Remarks = requirement.Remarks;
        existing.DepartmentId = requirement.DepartmentId;
        existing.SystemCategoryId = requirement.SystemCategoryId;
        existing.ResponsibleUserId = requirement.ResponsibleUserId;
        existing.ResponsibleUserName = requirement.ResponsibleUserName;
        existing.RequesterUserId = requirement.RequesterUserId;
        existing.RequesterUserName = requirement.RequesterUserName;
        existing.DeveloperUserId = requirement.DeveloperUserId;
        existing.DeveloperUserName = requirement.DeveloperUserName;
        existing.EstimatedHours = requirement.EstimatedHours;
        existing.PlannedStartDate = requirement.PlannedStartDate;
        existing.PlannedEndDate = requirement.PlannedEndDate;
        existing.DifficultyBefore = requirement.DifficultyBefore;
        existing.DifficultyAfter = requirement.DifficultyAfter;
        existing.HasPrimaryBenefit = requirement.HasPrimaryBenefit;
        existing.HasLongTermBenefit = requirement.HasLongTermBenefit;
        existing.HasCrossUnitBenefit = requirement.HasCrossUnitBenefit;
        existing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        if (responsibleChanged)
        {
            await notifications.NotifyAsync(
                requirement.ResponsibleUserId!,
                "你被指派為需求負責人",
                $"一般需求「{existing.Title}」已指派給你負責。",
                $"/requirements/{existing.Id}");
        }
    }

    public async Task<List<ScheduleChangeLog>> GetScheduleChangeLogsAsync(int requirementId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.ScheduleChangeLogs
            .Include(l => l.ChangedByUser)
            .Where(l => l.OwnerType == OwnerEntityType.GeneralRequirement && l.OwnerId == requirementId)
            .OrderByDescending(l => l.ChangedAt)
            .ToListAsync();
    }

    public async Task UpdateProgressAsync(int requirementId, int newProgressPercent, RequirementStatus newStatus, string? note, string changedByUserId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var requirement = await db.GeneralRequirements.FirstOrDefaultAsync(r => r.Id == requirementId)
            ?? throw new InvalidOperationException($"GeneralRequirement {requirementId} not found.");

        var log = new GeneralRequirementProgressLog
        {
            GeneralRequirementId = requirementId,
            OldProgressPercent = requirement.ProgressPercent,
            NewProgressPercent = newProgressPercent,
            OldStatus = requirement.Status,
            NewStatus = newStatus,
            Note = note,
            ChangedByUserId = changedByUserId,
            ChangedAt = DateTime.UtcNow
        };
        db.GeneralRequirementProgressLogs.Add(log);

        requirement.ProgressPercent = newProgressPercent;
        requirement.Status = newStatus;
        requirement.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(requirement.ResponsibleUserId) && requirement.ResponsibleUserId != changedByUserId)
        {
            await notifications.NotifyAsync(
                requirement.ResponsibleUserId,
                "需求進度已更新",
                $"一般需求「{requirement.Title}」進度更新為 {newProgressPercent}%。",
                $"/requirements/{requirement.Id}");
        }
    }

    /// <summary>
    /// 負責人／需求者／開發者都可以「從清單選」或「手動輸入姓名」二選一：
    /// 下拉選單「未指定」選項對 string 型別的欄位（如 ResponsibleUserId）會綁定成空字串而非 null，
    /// 這裡統一轉成 null，避免存進資料庫違反外鍵約束；同時只要有選到系統使用者（Id 有值），
    /// 手動輸入的姓名欄位就清空，避免兩邊資料不一致、顯示時不知道該用哪一個。
    /// </summary>
    private static void NormalizeResponsiblePersonFields(GeneralRequirement requirement)
    {
        (requirement.ResponsibleUserId, requirement.ResponsibleUserName) = NormalizePersonField(requirement.ResponsibleUserId, requirement.ResponsibleUserName);
        (requirement.RequesterUserId, requirement.RequesterUserName) = NormalizePersonField(requirement.RequesterUserId, requirement.RequesterUserName);
        (requirement.DeveloperUserId, requirement.DeveloperUserName) = NormalizePersonField(requirement.DeveloperUserId, requirement.DeveloperUserName);
    }

    private static (string? UserId, string? UserName) NormalizePersonField(string? userId, string? userName)
    {
        userId = string.IsNullOrWhiteSpace(userId) ? null : userId;
        userName = userId is not null ? null : (string.IsNullOrWhiteSpace(userName) ? null : userName.Trim());
        return (userId, userName);
    }
}
