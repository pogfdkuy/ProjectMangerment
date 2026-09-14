using ProjectManagement.Web.Models;

namespace ProjectManagement.Web.Services;

public interface IProjectService
{
    Task<List<Project>> GetListAsync(int? departmentId = null, int? systemCategoryId = null, ProjectStatus? status = null, string? keyword = null, string? responsibleUserId = null);

    Task<Project?> GetByIdAsync(int id);

    Task<Project> CreateAsync(Project project);

    /// <summary>
    /// 更新專案基本資料。<paramref name="changedByUserId"/> 只有在「預計開始／結束時間」實際被改動時才會用到
    /// （寫入 <see cref="Models.ScheduleChangeLog"/>），沒有值就不記錄時程異動歷程。
    /// </summary>
    Task UpdateAsync(Project project, string? changedByUserId = null);

    /// <summary>取得某專案的時程（預計開始／結束時間）異動歷程，新的在前。</summary>
    Task<List<Models.ScheduleChangeLog>> GetScheduleChangeLogsAsync(int projectId);

    Task<ProjectSubTask> AddSubTaskAsync(int projectId, ProjectSubTask subTask);

    Task UpdateSubTaskAsync(ProjectSubTask subTask);

    Task SetSubTaskCompletionAsync(int subTaskId, bool isCompleted);

    Task DeleteSubTaskAsync(int subTaskId);
}
