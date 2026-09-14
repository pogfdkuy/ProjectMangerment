using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Web.Models;

/// <summary>
/// 系統類別（例：SFCS、機況、標籤、品質…）。每個專案/一般需求僅能歸屬一個類別（單選）。
/// 由 Admin 在後台維護，可持續新增。
/// </summary>
public class SystemCategory
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
