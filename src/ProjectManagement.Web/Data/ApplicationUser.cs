using Microsoft.AspNetCore.Identity;
using ProjectManagement.Web.Models;

namespace ProjectManagement.Web.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// 顯示名稱（登入帳號之外，畫面上顯示用的姓名）。
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 所屬單位（可為 null，例如剛建立尚未指定單位的帳號）。
    /// </summary>
    public int? DepartmentId { get; set; }

    public Department? Department { get; set; }

    /// <summary>
    /// 是否啟用；停用的帳號無法登入，但既有資料仍保留（避免刪除造成外鍵/歷程資料遺失）。
    /// </summary>
    public bool IsActive { get; set; } = true;
}
