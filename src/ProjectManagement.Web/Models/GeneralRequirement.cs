using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectManagement.Web.Data;

namespace ProjectManagement.Web.Models;

/// <summary>
/// 一般需求（專案以外的系統小需求）。不拆子項目，完成度／進度由負責人直接手動更新，
/// 每次更新都會寫入 <see cref="GeneralRequirementProgressLog"/> 歷程紀錄。
/// </summary>
public class GeneralRequirement
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>備註（自由填寫的補充說明）。</summary>
    [StringLength(2000)]
    public string? Remarks { get; set; }

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    /// <summary>系統類別（單選）。</summary>
    public int? SystemCategoryId { get; set; }
    public SystemCategory? SystemCategory { get; set; }

    /// <summary>負責人（單一負責人）。</summary>
    public string? ResponsibleUserId { get; set; }
    public ApplicationUser? ResponsibleUser { get; set; }

    /// <summary>
    /// 負責人（手動輸入的姓名）。當這個人不是系統使用者、下拉選單裡找不到時使用。
    /// 只有在 <see cref="ResponsibleUserId"/> 沒有值時才會生效／顯示，兩者同時存在時以 Id 為準。
    /// </summary>
    [StringLength(100)]
    public string? ResponsibleUserName { get; set; }

    /// <summary>需求者（提出這個需求的人）。</summary>
    public string? RequesterUserId { get; set; }
    public ApplicationUser? RequesterUser { get; set; }

    /// <summary>需求者（手動輸入的姓名），規則同 <see cref="ResponsibleUserName"/>。</summary>
    [StringLength(100)]
    public string? RequesterUserName { get; set; }

    /// <summary>開發者（實際負責開發的人）。</summary>
    public string? DeveloperUserId { get; set; }
    public ApplicationUser? DeveloperUser { get; set; }

    /// <summary>開發者（手動輸入的姓名），規則同 <see cref="ResponsibleUserName"/>。</summary>
    [StringLength(100)]
    public string? DeveloperUserName { get; set; }

    public RequirementStatus Status { get; set; } = RequirementStatus.NotStarted;

    /// <summary>直接手動維護的進度百分比（0-100），非計算值。</summary>
    [Range(0, 100)]
    public int ProgressPercent { get; set; }

    /// <summary>評估預計工作時數（小時）。一般需求不像專案有子工作項目，這個是直接手動填的預估值。</summary>
    [Range(0, 100000)]
    public decimal? EstimatedHours { get; set; }

    /// <summary>預計開始時間。</summary>
    public DateTime? PlannedStartDate { get; set; }

    /// <summary>預計結束時間。</summary>
    public DateTime? PlannedEndDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<GeneralRequirementProgressLog> ProgressLogs { get; set; } = [];
    public List<Attachment> Attachments { get; set; } = [];

    // ── 效益分數評估（用來排優先順序、統計每個人的績效，不是正式KPI考核）──

    /// <summary>工作困難度：做之前是多少，1~5，見 <see cref="ScoreCalculator.DifficultyLevels"/>。未評估時為 null。</summary>
    public int? DifficultyBefore { get; set; }

    /// <summary>工作困難度：做之後（預估）是多少，1~5。未評估時為 null。</summary>
    public int? DifficultyAfter { get; set; }

    /// <summary>效益性子條件：主要性（節省金額達每月10萬）。</summary>
    public bool HasPrimaryBenefit { get; set; }

    /// <summary>效益性子條件：長期性（屢屢對策失效）。</summary>
    public bool HasLongTermBenefit { get; set; }

    /// <summary>效益性子條件：綜合性（跨2個單位以上的問題）。</summary>
    public bool HasCrossUnitBenefit { get; set; }

    /// <summary>困難度降低分數 = 做之前 - 做之後（預估），最低 0 分。</summary>
    [NotMapped]
    public int DifficultyReductionScore => ScoreCalculator.GetDifficultyReductionScore(DifficultyBefore, DifficultyAfter);

    /// <summary>效益性分數：符合幾項子條件對照 0/2/5/10 分。</summary>
    [NotMapped]
    public int BenefitScore => ScoreCalculator.GetBenefitScore(HasPrimaryBenefit, HasLongTermBenefit, HasCrossUnitBenefit);

    /// <summary>總分 = 困難度降低分數 + 效益性分數。</summary>
    [NotMapped]
    public int TotalScore => ScoreCalculator.GetTotalScore(DifficultyBefore, DifficultyAfter, HasPrimaryBenefit, HasLongTermBenefit, HasCrossUnitBenefit);

    /// <summary>顯示用姓名：有指派系統使用者就顯示該使用者，否則退回手動輸入的姓名（可能是 null）。</summary>
    [NotMapped]
    public string? ResponsibleDisplayName => ResponsibleUser?.DisplayName ?? ResponsibleUserName;

    /// <summary>顯示用姓名，規則同 <see cref="ResponsibleDisplayName"/>。</summary>
    [NotMapped]
    public string? RequesterDisplayName => RequesterUser?.DisplayName ?? RequesterUserName;

    /// <summary>顯示用姓名，規則同 <see cref="ResponsibleDisplayName"/>。</summary>
    [NotMapped]
    public string? DeveloperDisplayName => DeveloperUser?.DisplayName ?? DeveloperUserName;
}
