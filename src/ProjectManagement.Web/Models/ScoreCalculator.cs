namespace ProjectManagement.Web.Models;

/// <summary>
/// 效益分數計算邏輯共用元件（給 Project 跟 GeneralRequirement 共用，避免兩邊各寫一份）。
/// 用途：需求排序（分數高低排優先順序）與每個人的績效統計，不是傳統定義的 KPI 考核。
///
/// 指標一：工作困難度降低。分別記錄「做之前」與「做之後（預估）」的困難度等級（1~5，等級說明見
/// <see cref="DifficultyLevels"/>），分數採用「降低的幅度」= 之前 - 之後（如果之後比之前還高，
/// 視為沒有降低，計 0 分，不倒扣）。
/// 指標二：效益性，符合以下 3 個子條件的「項目數」對應到分數：
///   主要性：節省金額達每月10萬
///   長期性：屢屢對策失效
///   綜合性：跨2個單位以上的問題
///   符合 0 項 = 0 分，1 項 = 2 分，2 項 = 5 分，3 項 = 10 分。
/// 總分 = 困難度降低分數 + 效益性分數（兩者相加；如果之後想改成不同權重或分開看，這裡是唯一要改的地方）。
/// </summary>
public static class ScoreCalculator
{
    public static readonly (int Level, string Description)[] DifficultyLevels =
    [
        (1, "1 - 完全不會出錯"),
        (2, "2 - 不需要太專心，偶爾出錯"),
        (3, "3 - 需要專心，偶爾出錯"),
        (4, "4 - 專心且要看文件"),
        (5, "5 - 需要專業特定人士非常專注才能做")
    ];

    /// <summary>困難度降低分數 = 做之前的困難度 - 做之後（預估）的困難度，最低為 0（沒有兩者資料時也是 0）。</summary>
    public static int GetDifficultyReductionScore(int? difficultyBefore, int? difficultyAfter)
    {
        if (difficultyBefore is null || difficultyAfter is null) return 0;
        return Math.Max(0, difficultyBefore.Value - difficultyAfter.Value);
    }

    public static int GetBenefitScore(bool hasPrimary, bool hasLongTerm, bool hasCrossUnit)
    {
        var count = (hasPrimary ? 1 : 0) + (hasLongTerm ? 1 : 0) + (hasCrossUnit ? 1 : 0);
        return count switch
        {
            1 => 2,
            2 => 5,
            3 => 10,
            _ => 0
        };
    }

    public static int GetTotalScore(int? difficultyBefore, int? difficultyAfter, bool hasPrimary, bool hasLongTerm, bool hasCrossUnit) =>
        GetDifficultyReductionScore(difficultyBefore, difficultyAfter) + GetBenefitScore(hasPrimary, hasLongTerm, hasCrossUnit);
}
