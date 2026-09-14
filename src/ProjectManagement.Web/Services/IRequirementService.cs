using ProjectManagement.Web.Models;

namespace ProjectManagement.Web.Services;

public interface IRequirementService
{
    Task<List<GeneralRequirement>> GetListAsync(int? departmentId = null, int? systemCategoryId = null, RequirementStatus? status = null, string? keyword = null, string? responsibleUserId = null);

    Task<GeneralRequirement?> GetByIdAsync(int id);

    Task<GeneralRequirement> CreateAsync(GeneralRequirement requirement);

    /// <summary>
    /// 更新需求基本資料。<paramref name="changedByUserId"/> 只有在「預計開始／結束時間」實際被改動時才會用到
    /// （寫入 <see cref="Models.ScheduleChangeLog"/>），沒有值就不記錄時程異動歷程。
    /// </summary>
    Task UpdateBasicInfoAsync(GeneralRequirement requirement, string? changedByUserId = null);

    /// <summary>取得某需求的時程（預計開始／結束時間）異動歷程，新的在前。</summary>
    Task<List<Models.ScheduleChangeLog>> GetScheduleChangeLogsAsync(int requirementId);

    /// <summary>更新進度／狀態，並寫入一筆歷程紀錄。</summary>
    Task UpdateProgressAsync(int requirementId, int newProgressPercent, RequirementStatus newStatus, string? note, string changedByUserId);
}
