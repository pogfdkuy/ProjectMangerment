using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Web.Models;

/// <summary>
/// 專案子工作項目。完成度只有「完成／未完成」二態；完成時，整筆預估時數計入專案完成度的分子。
/// </summary>
public class ProjectSubTask
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>預估時數（小時）。</summary>
    [Range(0, 100000)]
    public decimal EstimatedHours { get; set; }

    /// <summary>預計開始時間。</summary>
    public DateTime? PlannedStartDate { get; set; }

    /// <summary>預計結束時間。</summary>
    public DateTime? PlannedEndDate { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
