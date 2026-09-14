using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Web.Models;

/// <summary>
/// 單位／部門。用於歸屬專案、一般需求與使用者，也是報表分組的依據。
/// </summary>
public class Department
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
