using System.ComponentModel.DataAnnotations;
using ProjectManagement.Web.Data;

namespace ProjectManagement.Web.Models;

/// <summary>
/// 一般需求的進度異動歷程（稽核用）：誰、何時，把進度從多少改成多少。
/// </summary>
public class GeneralRequirementProgressLog
{
    public int Id { get; set; }

    public int GeneralRequirementId { get; set; }
    public GeneralRequirement? GeneralRequirement { get; set; }

    public int OldProgressPercent { get; set; }
    public int NewProgressPercent { get; set; }

    public RequirementStatus? OldStatus { get; set; }
    public RequirementStatus? NewStatus { get; set; }

    [StringLength(1000)]
    public string? Note { get; set; }

    public string ChangedByUserId { get; set; } = string.Empty;
    public ApplicationUser? ChangedByUser { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
